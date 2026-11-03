using System.Security.Claims;
using ApiFacturas.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador de Permisos Propios (Versión 3).
/// Expone las rutas autorizadas del usuario autenticado para armar el menú en la interfaz gráfica.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige token JWT válido (401 si no hay token), pero NO exige permiso de ruta
public class PermisosController : ControllerBase
{
    private readonly IRepositorioAcceso _repositorioAcceso;

    public PermisosController(IRepositorioAcceso repositorioAcceso)
    {
        _repositorioAcceso = repositorioAcceso;
    }

    /// <summary>
    /// Retorna la lista de rutas autorizadas para el usuario en sesión.
    /// El correo se extrae directamente del token JWT, nunca de la URL.
    /// </summary>
    [HttpGet("mios")]
    public async Task<IActionResult> ObtenerPermisosMios()
    {
        var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? User.FindFirst(ClaimTypes.Email)?.Value 
                 ?? User.FindFirst("sub")?.Value 
                 ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(email))
        {
            return StatusCode(401, new
            {
                estado = 401,
                mensaje = "No hay una sesion valida. Inicie sesion."
            });
        }

        var rutas = await _repositorioAcceso.ObtenerRutasPermitidasPorEmailAsync(email);

        return Ok(new
        {
            email = email,
            total = rutas.Count,
            datos = rutas
        });
    }
}

