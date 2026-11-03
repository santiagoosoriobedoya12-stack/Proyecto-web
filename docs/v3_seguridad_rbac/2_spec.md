# Especificación — Versión 3: Seguridad, Autenticación y Autorización Basada en Roles (RBAC)

> | | |
> |---|---|
> | **La ruta** | [docs/README.md](../README.md) |
> | **Lo anterior** | [v1](../v1_sin_fk/README.md) — 6 tablas sin FK · [v2](../v2_con_fk/README.md) — 12 tablas completas |
>
> **La v3 INCLUYE la v1 y la v2.** Las 12 tablas de la base de datos `bdfacturas`, los procedimientos almacenados, el disparador contable y las interfaces de Blazor se mantienen 100% operativos. La v3 agrega la **capa transversal de seguridad**.

---

## 1. Propósito de la Versión 3

Cerrar la brecha de seguridad del sistema. En la v1 y v2, el sistema era de libre acceso: cualquier cliente HTTP podía crear productos, anular facturas o alterar clientes.

La **Versión 3** introduce:
1. **Autenticación (Identidad):** Ningún usuario opera la API o el front sin identificarse previamente mediante correo y contraseña válidos.
2. **Autorización RBAC (Permisos):** Cada ruta y verbo HTTP se valida contra la matriz de permisos de la base de datos (`rol_usuario` → `rutarol` → `ruta`).
3. **Control en Frontend (Blazor):** La interfaz gráfica exige inicio de sesión (`Login.razor`), adapta las opciones del menú de navegación a los roles del usuario logueado, y expulsa al usuario cuando su sesión caduca.

---

## 2. Alcance

### Lo que la v3 agrega a la API:
- **Endpoint de Autenticación (`/api/auth/login`):** Valida credenciales, comprueba el hash de la contraseña y retorna un token JWT firmado.
- **Endpoint de Perfil (`/api/auth/perfil`):** Retorna la información y lista de roles del usuario actualmente autenticado.
- **Middleware de Autenticación:** Intercepta cada petición entrante y valida la firma, emisor y vigencia del token JWT enviado en `Authorization: Bearer <token>`.
- **Middleware de Autorización RBAC:** Consulta el Stored Procedure `verificar_acceso_ruta` para comprobar si alguno de los roles del usuario tiene permiso sobre la ruta solicitada.
- **Dos nuevos códigos de error HTTP:**
  - **`401 Unauthorized`:** Emitido cuando no se envía token, el token es inválido o las credenciales de login son erróneas.
  - **`403 Forbidden`:** Emitido cuando el usuario está autenticado, pero sus roles no tienen permiso para la ruta o acción solicitada.

### Lo que la v3 agrega a la Interfaz Gráfica (Blazor Server):
- **Página de Login (`Components/Pages/Login.razor`):** Formulario para ingresar credenciales y recibir retroalimentación visual clara.
- **Proveedor de Estado de Autenticación (`AuthenticationStateProvider`):** Administra los claims del usuario y notifica cambios de sesión a todos los componentes Blazor.
- **Control de Rutas (`<AuthorizeRouteView>`):** Redirecciona automáticamente a `/login` si un usuario no autenticado intenta acceder a una URL directa.
- **Menú Adaptativo (`NavMenu.razor`):** Oculta los enlaces a módulos que el usuario no tiene permitidos según sus roles.
- **Botón de Cerrar Sesión:** Destruye el token almacenado y reinicia el estado de autenticación.

---

## 3. Requisitos Funcionales

| Requisito | Descripción |
|---|---|
| **RF1: Hashing de Contraseñas** | Las contraseñas en la tabla `usuario` **nunca** se almacenan en texto plano. Se procesan mediante un algoritmo de derivación de claves criptográficamente seguro (PBKDF2 / SHA-256 con Salt). |
| **RF2: Emisión de Tokens JWT** | Al iniciar sesión exitosamente, la API emite un token JWT que incluye claims con el `email`, identificador y la lista de `roles` del usuario, con fecha de expiración configurada. |
| **RF3: Protección de Endpoints** | Todos los endpoints de la API (v1 y v2) quedan protegidos por defecto, excepto: `GET /` (diagnóstico), `/api/auth/login` y la documentación interactiva Swagger. |
| **RF4: Autorización RBAC Dinámica** | El middleware de autorización ejecuta el procedimiento almacenado `verificar_acceso_ruta @email, @ruta` para determinar si el usuario tiene permiso para la ruta solicitada. |
| **RF5: Emisión de 401 Unauthorized** | Toda solicitud a una ruta protegida sin cabecera `Authorization: Bearer <token>` o con token vencido/inválido es rechazada con código 401 y sobre JSON descriptivo. |
| **RF6: Emisión de 403 Forbidden** | Toda solicitud donde el usuario no cuente con el rol requerido para la ruta es rechazada con código 403 y sobre JSON explicativo. |
| **RF7: Experiencia de Login en Blazor** | Formulario en `/login`. Al autenticarse, guarda el token en el almacenamiento seguro de sesión y redirige a la página principal. |
| **RF8: Interceptor HTTP en Blazor** | Todas las llamadas de los servicios tipados de Blazor (`ServicioProducto`, `ServicioFactura`, etc.) adjuntan automáticamente la cabecera `Authorization: Bearer <token>`. |

---

## 4. Criterios de Aceptación (Verificación de la v3)

1. **Criterio 1 (Acceso sin token):** `GET /api/producto` sin encabezado `Authorization` retorna exactamente **`401 Unauthorized`**.
2. **Criterio 2 (Login exitoso):** `POST /api/auth/login` con credenciales válidas retorna **`200 OK`** con el token JWT y los claims del usuario.
3. **Criterio 3 (Login con credenciales incorrectas):** `POST /api/auth/login` con contraseña errónea retorna **`401 Unauthorized`** sin revelar si el error fue el usuario o la contraseña.
4. **Criterio 4 (Acceso exitoso con token):** `GET /api/producto` con `Authorization: Bearer <token_valido>` retorna **`200 OK`** con el catálogo de productos.
5. **Criterio 5 (Acceso denegado por rol 403):** Petición a `/api/factura` con un token de un usuario cuyo rol no está en `rutarol` para esa ruta retorna **`403 Forbidden`**.
6. **Criterio 6 (Hash irreversible):** Una consulta directa a la tabla `usuario` en SQL Server muestra cadenas de hash irreconocibles, nunca contraseñas en claro.
7. **Criterio 7 (Redirección en Front):** Intentar entrar a `http://localhost:8099/facturas` sin estar autenticado redirige automáticamente a `http://localhost:8099/login`.
8. **Criterio 8 (Regresión total v1 y v2):** Con sesión activa de administrador, todos los flujos de la v1 y v2 funcionan con normalidad.

