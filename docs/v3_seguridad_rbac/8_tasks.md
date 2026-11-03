# Tareas — Versión 3: Seguridad, Autenticación y Autorización RBAC

> **Versión 3** — Plan de construcción ordenado por fases verificables.
> Cada fase termina en un componente comprobable y funcional.
> Requisitos: [2_spec.md](2_spec.md) · Técnica: [3_plan.md](3_plan.md) · Contratos: [6_contracts.md](6_contracts.md) · Smoke Test: [7_quickstart.md](7_quickstart.md).

---

## Fase 0 — Dependencias y Configuración Base
- [ ] Instalar o verificar dependencias de JWT en `api_facturas`:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `System.IdentityModel.Tokens.Jwt`
- [ ] Configurar parámetros en `appsettings.json` (Clave secreta `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiracionHoras`).
- [ ] Verificar que el Stored Procedure `verificar_acceso_ruta` esté activo en la base de datos SQL Server.
- [ ] Actualizar el endpoint de diagnóstico `GET /` para reportar `"version": "v3"`.

---

## Fase 1 — Criptografía y Hash de Contraseñas
- [ ] Crear interfaces e implementaciones de hashing seguro:
  - `IServicioCriptografia.cs` y `ServicioCriptografia.cs` (PBKDF2 / SHA-256 con Salt aleatoria por usuario).
- [ ] Actualizar el seed de contraseñas de los usuarios existentes en la base de datos para que contengan contraseñas procesadas con hash.
- [ ] Actualizar `UsuarioController`, `ServicioUsuario` y `ServicioUsuarioConRoles` para aplicar hash a las contraseñas al crearse o actualizarse.

---

## Fase 2 — Emisión de Tokens JWT y Login en la API
- [ ] Crear modelos y DTOs de autenticación:
  - `Peticiones/LoginPeticion.cs` (`email`, `contrasena`).
  - `Peticiones/LoginRespuesta.cs` (`token`, `tipo`, `expiraEnSegundos`, `usuario`).
- [ ] Implementar repositorio y servicio de seguridad:
  - `IRepositorioSeguridad.cs` y `RepositorioSeguridadSqlServer.cs`.
  - `IServicioToken.cs` y `ServicioToken.cs` (emisión y firma con Claims: `email`, `roles`).
  - `IServicioSeguridad.cs` y `ServicioSeguridad.cs` (valida hash y retorna token o error 401).
- [ ] Crear `Controllers/AuthController.cs`:
  - `POST /api/auth/login` (200 OK con token o 401 si falla).
  - `GET /api/auth/perfil` (retorna claims del usuario autenticado).

---

## Fase 3 — Middlewares de Seguridad y Control de Acceso RBAC
- [ ] Configurar autenticación en `api_facturas/Program.cs` (`builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...`).
- [ ] Implementar middleware interceptor de autorización:
  - `Middlewares/MiddlewareAutorizacionRbac.cs`.
  - Excluye rutas públicas (`/`, `/api/auth/login`, Swagger).
  - Extrae `email` del claim del token.
  - Invoca `verificar_acceso_ruta @email, @ruta`.
  - Si no tiene acceso, interrumpe la tubería y retorna `403 Forbidden` con sobre JSON estándar.
- [ ] Registrar `app.UseAuthentication()` y `app.UseAuthorization()` en el orden estricto de la tubería de middleware.

---

## Fase 4 — Frontend Blazor: Autenticación y Login
- [ ] Implementar cliente HTTP de autenticación en `front_blazor`:
  - `Servicios/ServicioAutenticacionFront.cs`.
- [ ] Implementar proveedor de estado de Blazor:
  - `Servicios/ProveedorAutenticacionCustom.cs` (hereda de `AuthenticationStateProvider`).
- [ ] Crear la página de inicio de sesión:
  - `Components/Pages/Login.razor` (formulario con email, contraseña, indicador de carga y alertas de error).
- [ ] Implementar manejador de delegación (`ManejadorTokenDelegado.cs`) para adjuntar automáticamente el token Bearer en todas las llamadas `HttpClient` hacia la API.

---

## Fase 5 — Protección de Vistas y Menús en Blazor
- [ ] Configurar `<CascadingAuthenticationState>` y `<AuthorizeRouteView>` en `Components/Routes.razor` para redirigir visitantes anónimos a `/login`.
- [ ] Actualizar `Components/Layout/NavMenu.razor` con directivas `<AuthorizeView Roles="...">` para mostrar únicamente los enlaces autorizados para cada rol.
- [ ] Agregar barra superior de usuario en el layout con el email del usuario logueado, sus roles y el botón de **Cerrar Sesión**.

---

## Fase 6 — Verificación, Regresión y Cierre
- [ ] Ejecutar el Smoke Test de seguridad con `curl` ([7_quickstart.md](7_quickstart.md)):
  - 401 sin token.
  - 200 con login exitoso.
  - 200 en productos con token.
  - 403 con usuario sin permiso a factura.
- [ ] Ejecutar la prueba de regresión de las 12 tablas de la v1 y v2 para asegurar que no se hayan roto funcionalidades previas.

