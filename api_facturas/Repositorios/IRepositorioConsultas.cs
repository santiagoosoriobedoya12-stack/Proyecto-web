using ApiFacturas.Modelos.Consultas;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de acceso a datos analíticos para la Versión 4.
/// Desacopla la capa de servicio del motor SQL Server (Dapper), permitiendo pruebas unitarias o cambio de motor.
/// Cada método C1..C10 ejecuta una consulta multitabla que cruza entre 4 y 6 tablas de 'bdfacturas'.
/// </summary>
public interface IRepositorioConsultas
{
    /// <summary>C1: Consolidado maestro de facturas con cliente (persona/empresa) y vendedor (6 tablas).</summary>
    Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync();

    /// <summary>C2: Ranking de productos más vendidos, unidades y recaudación neta (4 tablas).</summary>
    Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync();

    /// <summary>C3: Productividad por vendedor, facturas activas/anuladas y ticket promedio (5 tablas).</summary>
    Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync();

    /// <summary>C4: Segmentación de clientes VIP por volumen comprado y variedad (5 tablas).</summary>
    Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync();

    /// <summary>C5: Alerta preventiva de stock crítico y rotación histórica (4 tablas).</summary>
    Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync();

    /// <summary>C6: Auditoría de seguridad consolidando usuarios, roles y rutas autorizadas (5 tablas).</summary>
    Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync();

    /// <summary>C7: Desglose renglón por renglón de cada factura emitida (5 tablas).</summary>
    Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync();

    /// <summary>C8: Utilización de cupo de crédito y exposición financiera por cliente (4 tablas).</summary>
    Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync();

    /// <summary>C9: Detección de clientes registrados sin compras activas (4 tablas).</summary>
    Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync();

    /// <summary>C10: Auditoría de facturas anuladas e impacto financiero revertido (5 tablas).</summary>
    Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync();

    /// <summary>KPIs gerenciales en tiempo real para el Tablero de Control (Dashboard).</summary>
    Task<DashboardKpisDto> ObtenerDashboardKpisAsync();
}
