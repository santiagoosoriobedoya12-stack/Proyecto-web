using ApiFacturas.Autorizacion;
using ApiFacturas.Modelos.Consultas;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP REST para el Tablero de Control Ejecutivo (/api/dashboard).
/// 
/// REGLAS DE SEGURIDAD (Versión 3 y 4):
/// 1. [Authorize]: Exige token JWT Bearer emitido al identificarse en la sesión.
/// 2. [ExigePermiso("interfaz.inicio")]: Valida dinámicamente con la base de datos que el rol tenga permiso de inicio.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
[ExigePermiso("interfaz.inicio")]
public class DashboardController : ControllerBase
{
    private readonly IServicioConsultas _servicio;

    /// <summary>
    /// Constructor con inyección de dependencias del servicio de consultas analíticas.
    /// </summary>
    public DashboardController(IServicioConsultas servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// Devuelve las métricas consolidadas en tiempo real para el tablero gerencial:
    /// Ventas totales, facturas activas/anuladas, clientes, vendedores, productos y stock crítico.
    /// GET /api/dashboard/kpis
    /// </summary>
    [HttpGet("kpis")]
    public async Task<ActionResult<DashboardKpisDto>> ObtenerKpis()
    {
        var kpis = await _servicio.ObtenerDashboardKpisAsync();
        return Ok(kpis); // Código HTTP 200 OK con el objeto JSON del Dashboard
    }
}
