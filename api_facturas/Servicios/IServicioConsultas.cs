using ApiFacturas.Modelos.Consultas;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato de la capa de dominio/aplicación para los reportes analíticos y el tablero gerencial (Versión 4).
/// El controlador HTTP depende de esta interfaz y no conoce SQL ni Dapper directamente.
/// </summary>
public interface IServicioConsultas
{
    /// <summary>Obtiene el consolidado maestro de facturación (C1 — 6 tablas).</summary>
    Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync();

    /// <summary>Obtiene el ranking de productos más vendidos (C2 — 4 tablas).</summary>
    Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync();

    /// <summary>Obtiene el desempeño comercial por vendedor (C3 — 5 tablas).</summary>
    Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync();

    /// <summary>Obtiene la segmentación de clientes de alto valor (C4 — 5 tablas).</summary>
    Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync();

    /// <summary>Obtiene la alerta preventiva de stock crítico (C5 — 4 tablas).</summary>
    Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync();

    /// <summary>Obtiene la auditoría de seguridad, roles y rutas (C6 — 5 tablas).</summary>
    Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync();

    /// <summary>Obtiene los renglones detallados por factura (C7 — 5 tablas).</summary>
    Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync();

    /// <summary>Obtiene la utilización de línea de crédito (C8 — 4 tablas).</summary>
    Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync();

    /// <summary>Obtiene los clientes inactivos sin compras activas (C9 — 4 tablas).</summary>
    Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync();

    /// <summary>Obtiene la auditoría de facturas anuladas (C10 — 5 tablas).</summary>
    Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync();

    /// <summary>Obtiene los indicadores consolidados del Dashboard gerencial.</summary>
    Task<DashboardKpisDto> ObtenerDashboardKpisAsync();
}
