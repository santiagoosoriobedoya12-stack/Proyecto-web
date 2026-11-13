# Bitácora de Prompts — Versión 3: Seguridad JWT, BCrypt y RBAC

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para implementar la autenticación con JSON Web Tokens (JWT), el hash de contraseñas con BCrypt, las dependencias de autorización RBAC en FastAPI y la gestión de sesiones seguras en Flask con cookies `HttpOnly`.

---

## 1. Prompt de Base de Datos y Semilla Criptográfica (Python)

```text
Actúa como un experto en ciberseguridad, Python 3.12 y MariaDB.
1. Define en db/init.sql las 3 tablas de seguridad del sistema:
   - 'rol': id AUTO_INCREMENT, nombre VARCHAR(100), descripcion, activo TINYINT(1) DEFAULT 1.
   - 'usuario': id AUTO_INCREMENT, username VARCHAR(100) UNIQUE, password_hash VARCHAR(255), email VARCHAR(150) UNIQUE, nombre_completo, activo TINYINT(1) DEFAULT 1.
   - 'rol_usuario': usuario_id FK, rol_id FK, PRIMARY KEY (usuario_id, rol_id).
2. Crea el módulo api_mapa/semilla.py con una función idempotente asegurar_usuarios_semilla():
   - Inserta los 3 roles del sistema: 'Administrador', 'Investigador', 'Consulta'.
   - Crea 3 usuarios de prueba con contraseñas hasheadas mediante bcrypt (costo 12):
     * admin / admin123 -> Rol Administrador
     * investigador / inv123 -> Rol Investigador
     * consulta / consulta123 -> Rol Consulta
   - Se ejecuta en el ciclo de vida (lifespan) de FastAPI sin bloquear el servidor.
```

---

## 2. Prompts de Generación del Spec Kit (Documentos 2 a 8)

### Prompt para `2_spec.md` (Especificación Formal v3)
```text
Genera la especificación formal 2_spec.md para la Versión 3:
- Detalla los requisitos funcionales de autenticación con JWT (POST /api/login).
- Especifica la matriz de roles RBAC: Administrador (total), Investigador (CRUD módulo), Consulta (solo lectura GET).
- Define la frontera de seguridad 401 Unauthorized vs 403 Forbidden.
- Especifica el almacenamiento seguro del JWT en servidor Flask mediante cookies HttpOnly (cero localStorage).
```

### Prompt para `3_plan.md` y `4_research.md` (Plan Técnico e Investigación)
```text
Genera 3_plan.md y 4_research.md para la Versión 3:
- Justifica la elección de PyJWT y bcrypt sobre otras alternativas.
- Analiza la vulnerabilidad XSS de localStorage frente a cookies HttpOnly y justifica la arquitectura elegida.
- Explica la verificación de roles en tiempo real consultando la base de datos para revocación inmediata de acceso.
```

### Prompt para `5_data_model.md` y `6_contracts.md` (Modelo y Contratos REST)
```text
Genera 5_data_model.md y 6_contracts.md para la v3:
- Especifica las 3 tablas de seguridad (rol, usuario, rol_usuario) con password_hash y borrado lógico activo.
- Define el contrato de POST /api/login, GET /api/permisos/mios y los CRUDs administrativos de usuarios y roles.
```

### Prompt para `7_quickstart.md` y `8_tasks.md` (Puesta en Marcha y Tareas)
```text
Genera 7_quickstart.md y 8_tasks.md para la v3:
- Pasos para probar los 3 roles en el frontend y en Swagger.
- Checklist de tareas de T3.1 a T3.11 marcadas como completadas.
```

---

## 3. Prompt del Servicio de Autenticación y Token JWT

```text
Implementa el módulo de autenticación de la Versión 3 en FastAPI:
1. Crea los DTOs en peticiones/esquemas.py:
   - LoginPeticion: acepta 'usuario' (username) o 'email' y 'password'.
   - RolCrear, RolReemplazo, RolActualizar.
   - UsuarioCrear, UsuarioReemplazo, UsuarioActualizar (con password opcional en actualización).
   - RolUsuarioCrear (usuario_id, rol_id).
2. Crea ServicioLogin en servicios/servicio_v3.py:
   - Consulta el usuario activo por username o email.
   - Verifica la contraseña con bcrypt.checkpw().
   - Si falla, lanza NoAutenticadoExcepcion ("El usuario/correo o la contrasena no son correctos").
   - Consulta los roles activos del usuario en MariaDB.
   - Genera el token JWT firmado con algoritmo HS256, expiración configurable y payload:
     { sub, usuario_id, username, email, roles, iss, aud, iat, exp }.
3. Crea el router router_login en controladores/controlador_v3.py:
   - POST /api/login y POST /api/sesion (ambos públicos).
   - Retorna la entidad Sesion: { token, tipo: "Bearer", expira_en_segundos, username, email, roles }.
```

---

## 3. Prompt de Dependencias de Autorización RBAC en Tiempo Real

```text
Implementa la autorización basada en roles (RBAC) en FastAPI:
1. En autorizacion/dependencias.py, crea la dependencia `exige_usuario_autenticado`:
   - Lee la cabecera HTTP 'Authorization: Bearer <token>' con HTTPBearer(auto_error=False).
   - Si no viene o no es Bearer -> lanza NoAutenticadoExcepcion (HTTP 401).
   - Decodifica el token con PyJWT verificando firma HS256, issuer y audience.
   - Si el token está expirado o corrupto -> lanza NoAutenticadoExcepcion (HTTP 401).
   - Consulta en MariaDB los roles vigentes del usuario en tiempo real (para que revocar un rol tenga efecto inmediato sin esperar a que el token expire).
   - Si no tiene roles activos -> lanza NoAutenticadoExcepcion (401).
   - Retorna dict con { id, username, email, roles }.
2. Crea la factoría `exige_roles(*roles_permitidos)`:
   - Comprueba si el conjunto de roles del usuario se cruza con los roles permitidos.
   - Si no tiene ningún rol permitido -> lanza SinPermisoExcepcion (HTTP 403 Forbidden).
3. Protege las capas acumulativamente:
   - v1 y v2: GET requiere roles 'Administrador', 'Investigador', 'Consulta' (solo lectura).
   - v1 y v2: POST, PUT, PATCH, DELETE requiere roles 'Administrador', 'Investigador'.
   - v3 (/api/rol, /api/usuario, /api/rol-usuario): exclusivo para rol 'Administrador'.
```

---

## 4. Prompt de Frontend Flask con Sesión Segura y Cookies HttpOnly

```text
Implementa la gestión de sesiones y control de vista en Flask:
1. En front_flask/servicios/cliente_api.py:
   - Almacena las sesiones en memoria del servidor Flask: SESIONES[sid] = { token, username, email, roles, puede_escribir, es_admin }.
   - Emite al navegador una cookie llamada 'mapa_sid' con banderas HttpOnly=True y SameSite="Lax".
   - El token JWT jamás se expone a JavaScript ni se almacena en localStorage.
   - En cada petición hacia api-mapa (8117), inyecta automáticamente la cabecera Authorization: Bearer <token>.
2. En front_flask/app.py:
   - Rutas /sesion y /login (GET y POST) con formulario de acceso.
   - Ruta /cerrar-sesion (POST) que borra la sesión en el servidor y elimina la cookie mapa_sid.
   - Protege los CRUDs de /usuarios, /roles y /roles-usuario validando sesion.es_admin.
3. En las plantillas Jinja2:
   - base.html: renderiza el menú "v3 Administración" únicamente cuando sesion.es_admin == True.
   - recurso_crud.html, desarrolla.html, puente.html: oculta los botones de crear, editar y retirar cuando el usuario tiene rol 'Consulta' (sesion.puede_escribir == False).
```
