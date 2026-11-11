using System.Security.Claims;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;
// Atributos [Authorize] y [AllowAnonymous] para proteger o abrir endpoints HTTP (Versión 3)
using Microsoft.AspNetCore.Authorization;
// Componentes base de ASP.NET Core MVC (ControllerBase, IActionResult, [HttpGet], [Route])
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiFacturas.Autorizacion;

/// <summary>
/// Filtro de autorización RBAC para la Versión 3.
/// Intercepta cada petición y comprueba dinámicamente si el rol del usuario logueado
/// tiene permiso para acceder a la interfaz consultando el Stored Procedure 'verificar_acceso_ruta'.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ExigePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _nombreRuta;

    public ExigePermisoAttribute(string nombreRuta)
    {
        _nombreRuta = nombreRuta;
    }

    // Ejecuta la operación 'OnAuthorizationAsync' dentro de la capa correspondiente
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Si el endpoint tiene [AllowAnonymous] (ej. /api/sesion o /), omitir validación
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
        {
            return;
        }

        var user = context.HttpContext.User;

        // 2. Si no está autenticado, la tubería de JwtBearer responderá 401
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            return;
        }

        // 3. Extraer el email del usuario logueado de los claims
        var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? user.FindFirst(ClaimTypes.Email)?.Value 
                 ?? user.FindFirst("sub")?.Value 
                 ?? user.Identity.Name;

        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            context.Result = new ObjectResult(new
            {
                estado = 401,
                mensaje = "No hay una sesion valida. Inicie sesion."
            })
            {
                StatusCode = 401
            };
            return;
        }

        // 4. Inyectar el repositorio de acceso
        var repositorioAcceso = context.HttpContext.RequestServices.GetRequiredService<IRepositorioAcceso>();

        // 5. Traducir el nombre de la ruta a su ID numérico en la tabla 'ruta'
        var idRuta = await repositorioAcceso.ObtenerIdRutaPorNombreAsync(_nombreRuta);
        if (!idRuta.HasValue)
        {
            // Tropiezo 4 evitado (3_plan.md): Si la ruta no está en la tabla, falla cerrado
            context.Result = new ObjectResult(new
            {
                estado = 403,
                mensaje = "Su rol no tiene permiso para esta operacion.",
                ruta = _nombreRuta
            })
            {
                StatusCode = 403
            };
            return;
        }

        // 6. Consultar el procedimiento almacenado verificar_acceso_ruta
        // Criterio 6: El permiso lo resuelve el SP, nunca un JOIN en C#
        // Criterio 7: Se consulta en cada petición, por lo que quitar un permiso surte efecto inmediato
        bool tieneAcceso = await repositorioAcceso.VerificarAccesoAsync(email, idRuta.Value);

        if (!tieneAcceso)
        {
            // Criterio 5 / 6_contracts.md E: Retornar 403 con la ruta que faltó
            context.Result = new ObjectResult(new
            {
                estado = 403,
                mensaje = "Su rol no tiene permiso para esta operacion.",
                ruta = _nombreRuta
            })
            {
                StatusCode = 403
            };
        }
    }
}

