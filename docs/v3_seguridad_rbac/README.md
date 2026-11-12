# Versión 3 — El Control de Acceso: Seguridad, Autenticación y Autorización RBAC

> **Módulo:** Seguridad en Servicios Web y Control de Acceso  
> **Alcance:** Ponerle la puerta a lo que ya existe (proteger los 12 recursos con JWT y RBAC en Python).  
> **Enfoque Acumulativo:** $v1 \subset v2 \subset v3$ (Las 12 tablas de la v1 y v2 ya existen y operan; la v3 agrega la autenticación y la autorización).

---

## 1. Propósito de la Versión 3: «Ponerle la puerta a lo que ya existe»

Con la v2 cerrada, las **12 tablas** de `bdfacturas` son operables por la API y tienen su interfaz gráfica. Cualquiera que llegue a la dirección puede hacer cualquier cosa. **Eso es lo que la v3 arregla.**

> **Lo que la v3 NO hace es CRUD:** `usuario`, `rol`, `rol_usuario`, `ruta` y `rutarol` ya tienen su CRUD (construidos en la v1 y v2). Administrar la tabla de permisos y **hacerlos valer** son dos cosas distintas, y están en versiones distintas.
> 
> **La v3 no agrega una sola tabla.** Agrega el momento en que el sistema pregunta: *«¿y usted quién es?»*.

### Las tres piezas que llegan, y en este orden estricto:

```text
   credenciales               token                   permiso
        │                       │                        │
   AUTENTICA ────────►       SESIÓN     ────────►    AUTORIZA
   ¿quién es?              (PyJWT HS256)             ¿puede esto?
        │                       │                        │
  ServicioSesion     exige_usuario_autenticado    exige_permiso("...")
    + BCrypt                (Depends)            + verificar_acceso_ruta
```

1. **La contraseña deja de viajar en claro:** Se guarda con hash `bcrypt` (costo 12) y se verifica contra el hash.
2. **La sesión:** Quien se identifica recibe un token JWT que acompaña cada petición en la cabecera `Authorization: Bearer <token>`.
3. **El permiso:** Cada operación comprueba si el rol del usuario puede entrar a esa interfaz usando el procedimiento almacenado **`verificar_acceso_ruta`**, que ya existe en la base de datos.

---

## 2. Diferenciación de Códigos HTTP: 401 vs 403

| Código HTTP | Significado Exacto | Cuándo se emite |
|---|---|---|
| **`401 Unauthorized`** | **«No sé quién es usted»** | No llegó el token, el token es inválido, está alterado o venció. También si fallan las credenciales en `/api/sesion`. |
| **`403 Forbidden`** | **«Sé quién es, y no puede»** | El token es válido y está firmado, pero el rol del usuario no tiene la ruta permitida en `rutarol`. |

> **Nota técnica:** Por seguridad, `POST /api/sesion` responde exactamente el **mismo mensaje** ante correo inexistente o contraseña errónea: `{"estado":401,"mensaje":"El correo o la contrasena no son correctos."}`. Decir «ese correo no existe» le confirmaría a un atacante qué correos sí existen.

---

## 3. Decisiones Clave de Arquitectura (v3)

1. **El permiso lo decide la base (`verificar_acceso_ruta`):**
   - El procedimiento almacenado cruza `usuario → rol_usuario → rutarol → ruta`.
   - **No se duplica la decisión de permisos en Python**.
   - **No se guardan los permisos dentro del token JWT:** Si se guardaran en el token, quitarle un permiso a un rol no surtiría efecto hasta que el token expire. Al consultarlo en cada petición, **quitar un permiso surte efecto de inmediato sin volver a identificarse**.
2. **El token vive en el servidor (`front_python/`):**
   - La sesión autenticada vive en el diccionario en memoria del servidor FastAPI indexada por cookie `HttpOnly` (`factucorp_sid`). El token JWT nunca baja a `localStorage` ni al DOM.
3. **El menú no es la protección:**
   - El endpoint `GET /api/permisos/mios` solo sirve para armar el menú visual en Jinja2 (`base.html`).
   - Si un usuario escribe la URL a mano (ej: `/usuarios`), la API responde **403 Forbidden** y la pantalla muestra alerta sin filas.

---

## 4. Índice de Documentos de la Versión 3

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación funcional oficial, alcance y los **10 criterios de aceptación** de la v3. |
| [3_plan.md](3_plan.md) | Plan técnico: `PyJWT`, `bcrypt` costo 12, dependencias FastAPI (`exige_permiso`) y sesión en servidor Python. |
| [4_research.md](4_research.md) | Investigación y decisiones: SHA-256 vs bcrypt, costo 12, permisos en BD vs token, `leeway=0`. |
| [5_data_model.md](5_data_model.md) | Las 5 tablas del control de acceso (`usuario`, `rol`, `ruta`, `rol_usuario`, `rutarol`), el SP `verificar_acceso_ruta` y las contraseñas hash. |
| [6_contracts.md](6_contracts.md) | Contratos exactos: `POST /api/sesion`, `GET /api/permisos/mios`, formato del 401 y 403, y matriz de respuestas por rol. |
| [7_quickstart.md](7_quickstart.md) | Guía de prueba de humo con `curl` para los 10 criterios, contraseñas conocidas y verificación en navegador. |
| [8_tasks.md](8_tasks.md) | Orden inmutable de construcción por fases (Fase 0 a Fase 10), con verificación paso a paso. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA utilizados para diseñar y construir la seguridad, autenticación y RBAC en Python. |
