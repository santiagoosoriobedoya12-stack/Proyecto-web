# Investigación y Decisiones Técnicas — Versión 3: Seguridad y RBAC

> Documento de fundamentación técnica de las decisiones de diseño adoptadas en la Versión 3.

---

## 1. Decisión 1: ¿Tokens JWT o Cookies de Sesión?

| Opción | Ventajas | Desventajas | Veredicto |
|---|---|---|---|
| **Cookies de Sesión Tradicionales** | El navegador las maneja automáticamente; inmunes a robo por XSS si son `HttpOnly`. | Requieren almacenamiento de estado en servidor (*stateful*) o sesión distribuida (Redis); difíciles de consumir desde clientes móviles o Postman/curl; vulnerables a CSRF. | Descartada para la API REST. |
| **JSON Web Tokens (JWT) Bearer** | **Totalmente sin estado (*stateless*):** el servidor no guarda sesiones en memoria ni en disco; portabilidad universal (funciona idéntico en Blazor, Swagger, curl, Postman o apps móviles); autocontenido (lleva claims de roles en el payload). | El token no puede invalidarse fácilmente antes de expirar sin una lista negra (*blacklist*). | **ELEGIDA.** Estándar indiscutible para APIs REST modernas. |

---

## 2. Decisión 2: ¿Hashing de Contraseñas: Algoritmo y Salting?

### El problema del texto plano o MD5 / SHA-1:
- Almacenar contraseñas en texto plano es una falta grave a la seguridad de la información.
- Algoritmos rápidos como MD5 o SHA-1 son vulnerables a colisiones y a ataques masivos con **Tablas Arcoíris (*Rainbow Tables*)** precalculadas.

### La solución elegida: Hashing con Salt Criptográfica (PBKDF2 / SHA-256):
- **Salt (Sal):** Secuencia aleatoria única de bytes generada para cada usuario al momento de registrar o cambiar la contraseña. Impide que dos usuarios con la misma contraseña compartan el mismo hash.
- **Factor de Trabajo (Iteraciones):** Hace computacionalmente costoso intentar millones de combinaciones por segundo con fuerza bruta.

---

## 3. Decisión 3: ¿Middleware Personalizado o Atributos `[Authorize(Roles = "...")]`?

### El conflicto:
ASP.NET Core provee el atributo `[Authorize(Roles = "Admin,Vendedor")]`, pero:
1. Codificar los roles permitidos en el código C# acopla la aplicación y contradice la base de datos `bdfacturas`, donde la tabla `rutarol` y el procedimiento almacenado `verificar_acceso_ruta` fueron creados precisamente para **administrar los permisos dinámicamente desde la BD**.
2. Si un administrador cambia un permiso en `PermisosPorRol.razor` (asigna una nueva ruta a un rol), con atributos `[Authorize]` habría que recompilar la aplicación.

### La solución elegida: Middleware RBAC conectado al SP:
- Se implementa un **Middleware de Autorización Dinámico**:
  1. Captura la ruta solicitada (`context.Request.Path`).
  2. Extrae el correo del usuario del token JWT.
  3. Ejecuta `EXEC verificar_acceso_ruta @email, @ruta`.
  4. Si retorna 1, permite continuar. Si retorna 0, corta la petición con **HTTP 403 Forbidden**.
- **Resultado:** El sistema es 100% dinámico. Cambiar un permiso en la tabla `rutarol` tiene efecto inmediato sin reiniciar el servidor.

---

## 4. Decisión 4: Diferenciación Estricta entre 401 y 403

| Aspecto | `401 Unauthorized` | `403 Forbidden` |
|---|---|---|
| **Pregunta de evaluación** | ¿Quién eres? | ¿Tienes permiso de hacer esto? |
| **Causa técnica** | Cabecera ausente, token mal formado, token expirado o firma inválida. | Token válido y verificado, pero el rol del usuario no tiene la ruta permitida en `rutarol`. |
| **Acción recomendada al cliente** | Redirigir a `/login` para obtener un nuevo token. | Mostrar mensaje visual: *"No cuentas con privilegios para acceder a este módulo"*. |

