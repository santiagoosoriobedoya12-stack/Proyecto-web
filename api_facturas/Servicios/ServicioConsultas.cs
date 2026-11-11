using ApiFacturas.Modelos.Consultas;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioConsultas : IServicioConsultas
{
    private readonly IRepositorioConsultas _repositorio;

    public ServicioConsultas(IRepositorioConsultas repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync() =>
        _repositorio.ObtenerC1ConsolidadoFacturacionAsync();

    public Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync() =>
        _repositorio.ObtenerC2RankingProductosAsync();

    public Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync() =>
        _repositorio.ObtenerC3RendimientoVendedoresAsync();

    public Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync() =>
        _repositorio.ObtenerC4ClientesAltoValorAsync();

    public Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync() =>
        _repositorio.ObtenerC5StockCriticoAsync();

    public Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync() =>
        _repositorio.ObtenerC6AuditoriaSeguridadAsync();

    public Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync() =>
        _repositorio.ObtenerC7RenglonesFacturaAsync();

    public Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync() =>
        _repositorio.ObtenerC8CarteraCreditoAsync();

    public Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync() =>
        _repositorio.ObtenerC9ClientesInactivosAsync();

    public Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync() =>
        _repositorio.ObtenerC10FacturasAnuladasAsync();

    public Task<DashboardKpisDto> ObtenerDashboardKpisAsync() =>
        _repositorio.ObtenerDashboardKpisAsync();
}

