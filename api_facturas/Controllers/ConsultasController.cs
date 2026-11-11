using ApiFacturas.Autorizacion;
using ApiFacturas.Modelos.Consultas;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP REST para la ejecución de las 10 Consultas Multitabla Complejas (/api/consultas).
/// 
/// REGLAS DE SEGURIDAD (Versión 3 y 4):
/// 1. [Authorize]: Exige obligatoriamente un token JWT válido emitido por el endpoint de inicio de sesión (/api/sesion).
/// 2. [ExigePermiso("interfaz.inicio")]: Verifica en base de datos mediante el Stored Procedure 'verificar_acceso_ruta'
///    que el rol del usuario tenga habilitado el acceso al módulo principal de la aplicación.
/// </summary>
[ApiController]
[Route("api/consultas")]
[Authorize]
[ExigePermiso("interfaz.inicio")]
public class ConsultasController : ControllerBase
{
    // Servicio de dominio que encapsula la lógica de negocio y las llamadas al repositorio Dapper
    private readonly IServicioConsultas _servicio;

    /// <summary>
    /// Inyección de dependencias: ASP.NET Core inyecta la implementación IServicioConsultas registrada en Program.cs.
    /// </summary>
    public ConsultasController(IServicioConsultas servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// C1: Consolidado Maestro de Facturación con Detalle de Partes (6 tablas).
    /// GET /api/consultas/c1
    /// </summary>
    [HttpGet("c1")]
    public async Task<ActionResult<IEnumerable<ConsultaFacturaDetalladaDto>>> ObtenerC1()
    {
        var res = await _servicio.ObtenerC1ConsolidadoFacturacionAsync();
        return Ok(res); // Código HTTP 200 OK con el listado en formato JSON
    }

    /// <summary>
    /// C2: Ranking de Productos más Vendidos y Rentabilidad Generada (4 tablas).
    /// GET /api/consultas/c2
    /// </summary>
    [HttpGet("c2")]
    public async Task<ActionResult<IEnumerable<ConsultaRankingProductosDto>>> ObtenerC2()
    {
        var res = await _servicio.ObtenerC2RankingProductosAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C3: Desempeño Comercial y Productividad por Vendedor (5 tablas).
    /// GET /api/consultas/c3
    /// </summary>
    [HttpGet("c3")]
    public async Task<ActionResult<IEnumerable<ConsultaRendimientoVendedoresDto>>> ObtenerC3()
    {
        var res = await _servicio.ObtenerC3RendimientoVendedoresAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C4: Segmentación de Clientes de Alto Valor / Clientes VIP (5 tablas).
    /// GET /api/consultas/c4
    /// </summary>
    [HttpGet("c4")]
    public async Task<ActionResult<IEnumerable<ConsultaClientesAltoValorDto>>> ObtenerC4()
    {
        var res = await _servicio.ObtenerC4ClientesAltoValorAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C5: Alerta Preventiva de Stock Crítico y Rotación de Demanda (4 tablas).
    /// GET /api/consultas/c5
    /// </summary>
    [HttpGet("c5")]
    public async Task<ActionResult<IEnumerable<ConsultaStockCriticoDto>>> ObtenerC5()
    {
        var res = await _servicio.ObtenerC5StockCriticoAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C6: Auditoría de Seguridad, Roles y Trazabilidad de Permisos (5 tablas).
    /// GET /api/consultas/c6
    /// </summary>
    [HttpGet("c6")]
    public async Task<ActionResult<IEnumerable<ConsultaAuditoriaSeguridadDto>>> ObtenerC6()
    {
        var res = await _servicio.ObtenerC6AuditoriaSeguridadAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C7: Renglones Detallados de Facturación Transaccional (5 tablas).
    /// GET /api/consultas/c7
    /// </summary>
    [HttpGet("c7")]
    public async Task<ActionResult<IEnumerable<ConsultaRenglonesFacturaDto>>> ObtenerC7()
    {
        var res = await _servicio.ObtenerC7RenglonesFacturaAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C8: Utilización de Línea de Crédito y Exposición Financiera (4 tablas).
    /// GET /api/consultas/c8
    /// </summary>
    [HttpGet("c8")]
    public async Task<ActionResult<IEnumerable<ConsultaCarteraCreditoDto>>> ObtenerC8()
    {
        var res = await _servicio.ObtenerC8CarteraCreditoAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C9: Detección de Clientes Inactivos sin Compras Registradas (4 tablas).
    /// GET /api/consultas/c9
    /// </summary>
    [HttpGet("c9")]
    public async Task<ActionResult<IEnumerable<ConsultaClientesInactivosDto>>> ObtenerC9()
    {
        var res = await _servicio.ObtenerC9ClientesInactivosAsync();
        return Ok(res); // Código HTTP 200 OK
    }

    /// <summary>
    /// C10: Auditoría de Facturas Anuladas e Impacto Financiero (5 tablas).
    /// GET /api/consultas/c10
    /// </summary>
    [HttpGet("c10")]
    public async Task<ActionResult<IEnumerable<ConsultaFacturasAnuladasDto>>> ObtenerC10()
    {
        var res = await _servicio.ObtenerC10FacturasAnuladasAsync();
        return Ok(res); // Código HTTP 200 OK
    }
}
