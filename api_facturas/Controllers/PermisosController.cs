using System.Security.Claims;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;
// Atributos [Authorize] y [AllowAnonymous] para proteger o abrir endpoints HTTP (Versión 3)
using Microsoft.AspNetCore.Authorization;
// Componentes base de ASP.NET Core MVC (ControllerBase, IActionResult, [HttpGet], [Route])
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador de Permisos Propios (Versión 3).
/// Expone las rutas autorizadas del usuario autenticado para armar el menú en la interfaz gráfica.
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/[controller]'
[Route("api/[controller]")]
[Authorize] // Exige token JWT válido (401 si no hay token), pero NO exige permiso de ruta
// Controlador REST 'PermisosController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class PermisosController : ControllerBase
{
    private readonly IRepositorioAcceso _repositorioAcceso;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public PermisosController(IRepositorioAcceso repositorioAcceso)
    {
        _repositorioAcceso = repositorioAcceso;
    }

    /// <summary>
    /// Retorna la lista de rutas autorizadas para el usuario en sesión.
    /// El correo se extrae directamente del token JWT, nunca de la URL.
    /// </summary>
    [HttpGet("mios")]
    // Ejecuta la operación 'ObtenerPermisosMios' dentro de la capa correspondiente
    public async Task<IActionResult> ObtenerPermisosMios()
    {
        var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? User.FindFirst(ClaimTypes.Email)?.Value 
                 ?? User.FindFirst("sub")?.Value 
                 ?? User.Identity?.Name;

        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            return StatusCode(401, new
            {
                estado = 401,
                mensaje = "No hay una sesion valida. Inicie sesion."
            });
        }

        var rutas = await _repositorioAcceso.ObtenerRutasPermitidasPorEmailAsync(email);

        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(new
        {
            email = email,
            total = rutas.Count,
            datos = rutas
        });
    }
}

