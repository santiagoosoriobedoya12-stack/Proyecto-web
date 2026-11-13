# Especificación — Versión 3: Control de Acceso (Seguridad, JWT y RBAC)

> **Versión 3** del desarrollo incremental: Cierre del sistema ($v1 \subset v2 \subset v3$) mediante autenticación con JSON Web Tokens (JWT), cifrado de contraseñas con BCrypt, autorización RBAC (Administrador, Investigador, Consulta), frontera 401 vs 403 y sesiones seguras en Flask con cookies `HttpOnly`.

---

## 1. Propósito de la v3
Cerrar la seguridad del sistema protegiendo de forma transversal los endpoints de la v1 y la v2, e incorporando las 3 tablas de gestión de usuarios y roles (`rol`, `usuario`, `rol_usuario`).

---

## 2. Requisitos Funcionales
- **RF1 — Login y Emisión de JWT:** `POST /api/login` recibe usuario/correo y contraseña. Valida el hash BCrypt y emite un token JWT firmado (`HS256`) con tiempo de expiración y roles asignados. Credenciales inválidas devuelven HTTP 401 Unauthorized.
- **RF2 — Roles del Sistema (RBAC):**
  - `Administrador`: Acceso total a las 21 tablas del sistema y gestión de usuarios/roles.
  - `Investigador`: CRUD completo sobre el módulo de investigación (v1 y v2); HTTP 403 Forbidden en gestión de usuarios y roles.
  - `Consulta`: Solo lectura (`GET`) en v1 y v2; HTTP 403 Forbidden al intentar crear, editar o retirar registros, y en gestión de usuarios.
- **RF3 — Verificación en Tiempo Real:** Las dependencias de FastAPI validan la firma del token y consultan los roles activos en MariaDB en cada llamada, de modo que revocar un rol tenga efecto inmediato.
- **RF4 — Gestión de Usuarios y Roles (Solo Admin):** Endpoints `/api/rol`, `/api/usuario`, `/api/rol-usuario` con contraseñas cifradas en BCrypt (costo 12).
- **RF5 — Sesiones en Frontend Flask:** El JWT se conserva en memoria del servidor (`SESIONES`) detrás de una cookie segura `HttpOnly` (`mapa_sid`).
- **RF6 — Semilla Automática Idempotente:** Al iniciar la API, `api_mapa/semilla.py` asegura la existencia de `admin`, `investigador` y `consulta`.

---

## 3. Requisitos No Funcionales
- **RNF1 — Cero Contraseñas en Texto Plano:** Todas las contraseñas se hashean mediante `bcrypt.hashpw` con salt seguro.
- **RNF2 — Frontera de Códigos HTTP:**
  - 401 Unauthorized: falta token, expiró o está corrupto.
  - 403 Forbidden: token válido, pero el rol del usuario no tiene permisos.
  - 422 Unprocessable Entity: error de esquema en los datos de entrada.
- **RNF3 — Protección Contra XSS:** El token JWT nunca se expone a `localStorage` ni a JavaScript en el navegador.
