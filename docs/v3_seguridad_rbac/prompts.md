# Bitácora de Prompts — Versión 3: El Control de Acceso (Seguridad, JWT y RBAC)

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para implementar la autenticación con JWT, el hash de contraseñas con BCrypt, el middleware de autorización RBAC con Stored Procedures, y la gestión de sesiones en Blazor Server.

---

## 1. Prompt de Semilla y Criptografía (Fases 1 y 2)
```text
Actúa como un experto en ciberseguridad y .NET 10.
1. Actualiza el script SQL db/bdfacturas.sql para re-sembrar la tabla 'usuario' utilizando hashes BCrypt con factor de costo 12 ($2a$12$).
2. Garantiza que las contraseñas en claro coincidan con las del quickstart:
   - admin@correo.com: admin123
   - vendedor1@correo.com: vendedor123
   - cliente1@correo.com: cliente123
3. En ApiFacturas.csproj agrega los paquetes:
   - Microsoft.AspNetCore.Authentication.JwtBearer (9.0.10)
   - BCrypt.Net-Next (4.0.3)
4. Configura en appsettings.json y docker-compose.yml la clave secreta Jwt:Key de 32+ caracteres, Issuer y Audience.
```

---

## 2. Prompt del Módulo de Autenticación y Token (Fases 3 y 4)
```text
Implementa el módulo de autenticación de la Versión 3 en ASP.NET Core:
1. Crea Modelos/Sesion.cs y Peticiones/SesionCrear.cs ({ email, contrasena } en el cuerpo).
2. Crea IServicioSesion y ServicioSesion:
   - Consulta el usuario por email.
   - Comprueba la contraseña contra el hash usando BCrypt.Verify().
   - Genera el token JWT firmado con SymmetricSecurityKey conteniendo claims de email y roles.
3. Crea SesionController.cs con POST /api/sesion:
   - Decorado con [AllowAnonymous].
   - Si las credenciales son incorrectas (email inexistente o contraseña errada), responde exactamente el MISMO error 401:
     { "estado": 401, "mensaje": "El correo o la contrasena no son correctos." }
4. En Program.cs registra AddAuthentication().AddJwtBearer() con:
   - ClockSkew = TimeSpan.Zero (para no tolerar desfase de expiración).
   - Evento OnChallenge que escribe el cuerpo JSON descriptivo para 401:
     { "estado": 401, "mensaje": "No hay una sesion valida. Inicie sesion." }
   - Registra app.UseAuthentication() antes de app.UseAuthorization().
```

---

## 3. Prompt de Autorización RBAC Dinámica (Fases 5 y 6)
```text
Implementa el control de acceso basado en roles mediante Stored Procedure:
1. Crea IRepositorioAcceso y RepositorioAccesoSqlServer:
   - Ejecuta 'EXEC verificar_acceso_ruta @p_email, @p_fkidruta, @p_resultado OUTPUT'.
   - Traduce el nombre de la ruta (ej: 'interfaz.usuarios') a su ID numérico en la tabla 'ruta'.
   - Si la ruta no existe, falla cerrado (tiene_acceso = false).
2. Crea el filtro de autorización Autorizacion/ExigePermisoAttribute.cs:
   - Intercepta la petición y valida el permiso en cada llamada consultando a la base de datos (para que quitar un permiso surta efecto inmediato sin re-loguearse).
   - Si no tiene permiso, retorna HTTP 403 Forbidden:
     { "estado": 403, "mensaje": "Su rol no tiene permiso para esta operacion.", "ruta": "interfaz.x" }
3. Decora los 12 controladores de negocio con [Authorize] y [ExigePermiso("interfaz.x")].
4. Crea Controllers/PermisosController.cs con GET /api/permisos/mios:
   - Extrae el email del token JWT.
   - Retorna la lista de rutas autorizadas para armar el menú del front.
```

---

## 4. Prompt de Frontend Blazor Server (Fases 7, 8 y 9)
```text
Implementa la gestión de sesiones y menús dinámicos en Blazor Server:
1. Crea Servicios/EstadoSesion.cs registrado como SCOPED en DI (uno por circuito de usuario, nunca singleton).
2. Crea Servicios/ServicioSesion.cs para consumir POST /api/sesion y guardar la sesión en EstadoSesion.
3. Crea Components/Pages/Sesion.razor para inicio de sesión con alertas de error y Components/Layout/SesionActual.razor para botón de cerrar sesión.
4. En los 12 servicios HTTP de Blazor (ServicioProducto, ServicioCliente, etc.), agrega el método Autorizar() para inyectar la cabecera 'Authorization: Bearer <token>' antes de cada llamada.
5. En Components/App.razor, configura el modo interactivo sin renderizado previo:
   <Routes @rendermode="@(new InteractiveServerRenderMode(prerender: false))" />
6. En Components/Layout/NavMenu.razor, consume GET /api/permisos/mios para renderizar únicamente las opciones del menú autorizadas para el rol del usuario conectado.
```

