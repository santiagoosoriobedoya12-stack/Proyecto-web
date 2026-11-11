using ApiFacturas.Modelos.Consultas;

namespace ApiFacturas.Servicios;

public interface IServicioConsultas
{
    Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync();
    Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync();
    Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync();
    Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync();
    Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync();
    Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync();
    Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync();
    Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync();
    Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync();
    Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync();
    Task<DashboardKpisDto> ObtenerDashboardKpisAsync();
}

