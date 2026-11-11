using ApiFacturas.Modelos.Consultas;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación del servicio de aplicación para las 10 consultas multitabla y el Dashboard (Versión 4).
/// Actúa como capa intermedia entre los controladores HTTP y el repositorio de base de datos,
/// garantizando la separación estricta en 3 capas (Controlador -> Servicio -> Repositorio).
/// </summary>
public class ServicioConsultas : IServicioConsultas
{
    // Dependencia hacia la abstracción del repositorio (inyectada por el contenedor de DI en Program.cs)
    private readonly IRepositorioConsultas _repositorio;

    /// <summary>
    /// Recibe el repositorio de consultas por inyección de dependencias y valida que no sea nulo.
    /// </summary>
    public ServicioConsultas(IRepositorioConsultas repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync() =>
        _repositorio.ObtenerC1ConsolidadoFacturacionAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync() =>
        _repositorio.ObtenerC2RankingProductosAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync() =>
        _repositorio.ObtenerC3RendimientoVendedoresAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync() =>
        _repositorio.ObtenerC4ClientesAltoValorAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync() =>
        _repositorio.ObtenerC5StockCriticoAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync() =>
        _repositorio.ObtenerC6AuditoriaSeguridadAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync() =>
        _repositorio.ObtenerC7RenglonesFacturaAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync() =>
        _repositorio.ObtenerC8CarteraCreditoAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync() =>
        _repositorio.ObtenerC9ClientesInactivosAsync();

    /// <inheritdoc />
    public Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync() =>
        _repositorio.ObtenerC10FacturasAnuladasAsync();

    /// <inheritdoc />
    public Task<DashboardKpisDto> ObtenerDashboardKpisAsync() =>
        _repositorio.ObtenerDashboardKpisAsync();
}
