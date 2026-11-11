namespace FrontFacturas.Modelos.Consultas;

/// <summary>
/// DTO (Data Transfer Object) inmutable para la Consulta 1: Consolidado Maestro de Facturación.
/// Cruza 6 tablas: factura, cliente, persona (cliente), empresa (cliente), vendedor y persona (vendedor).
/// </summary>
public record ConsultaFacturaDetalladaDto(
    int NumeroFactura,
    DateTime Fecha,
    decimal Total,
    string Estado,
    string ClienteNombre,
    string ClienteTipo,
    string ClienteContacto,
    string VendedorNombre,
    int VendedorCarnet
);

/// <summary>
/// DTO para la Consulta 2: Ranking de Productos más Vendidos y Rentabilidad.
/// Cruza 4 tablas: producto, productosporfactura, factura y cliente.
/// </summary>
public record ConsultaRankingProductosDto(
    string CodigoProducto,
    string NombreProducto,
    decimal ValorUnitario,
    int StockActual,
    int UnidadesVendidas,
    decimal TotalRecaudado,
    int TotalFacturasPresente
);

/// <summary>
/// DTO para la Consulta 3: Desempeño Comercial y Productividad por Vendedor.
/// Cruza 5 tablas: vendedor, persona, factura, productosporfactura y producto.
/// </summary>
public record ConsultaRendimientoVendedoresDto(
    int IdVendedor,
    int Carnet,
    string NombreVendedor,
    string EmailVendedor,
    int TotalFacturas,
    int FacturasActivas,
    int FacturasAnuladas,
    decimal MontoTotalVendido,
    decimal TicketPromedio
);

/// <summary>
/// DTO para la Consulta 4: Segmentación de Clientes de Alto Valor (Clientes VIP).
/// Cruza 5 tablas: cliente, persona, empresa, factura y productosporfactura.
/// </summary>
public record ConsultaClientesAltoValorDto(
    int IdCliente,
    string NombreTitular,
    string EmpresaAsociada,
    decimal CreditoAsignado,
    int TotalFacturas,
    decimal MontoTotalComprado,
    int VariedadProductosComprados,
    DateTime? FechaUltimaCompra
);

/// <summary>
/// DTO para la Consulta 5: Alerta Preventiva de Stock Crítico y Rotación.
/// Cruza 4 tablas: producto, productosporfactura, factura y vendedor.
/// </summary>
public record ConsultaStockCriticoDto(
    string CodigoProducto,
    string NombreProducto,
    int StockActual,
    decimal ValorUnitario,
    int TotalVendidoHistorico,
    string NivelAlerta,
    DateTime? FechaUltimaVenta
);

/// <summary>
/// DTO para la Consulta 6: Auditoría de Seguridad, Roles y Trazabilidad de Permisos.
/// Cruza 5 tablas: usuario, rol_usuario, rol, rutarol y ruta.
/// </summary>
public record ConsultaAuditoriaSeguridadDto(
    string EmailUsuario,
    string RolesAsignados,
    int CantidadRoles,
    int CantidadRutasPermitidas,
    string RutasDetalle
);

/// <summary>
/// DTO para la Consulta 7: Renglones Detallados de Facturación Transaccional.
/// Cruza 5 tablas: factura, productosporfactura, producto, cliente y persona.
/// </summary>
public record ConsultaRenglonesFacturaDto(
    int NumeroFactura,
    DateTime FechaFactura,
    string ClienteNombre,
    string CodigoProducto,
    string NombreProducto,
    int Cantidad,
    decimal? PrecioUnitarioCalculado,
    decimal SubtotalRenglon,
    decimal TotalFactura
);

/// <summary>
/// DTO para la Consulta 8: Utilización de Línea de Crédito y Exposición Financiera.
/// Cruza 4 tablas: cliente, persona, empresa y factura.
/// </summary>
public record ConsultaCarteraCreditoDto(
    int IdCliente,
    string PersonaNombre,
    string EmpresaNombre,
    decimal LimiteCredito,
    decimal TotalFacturadoActivo,
    decimal CreditoDisponible,
    string EstadoCredito
);

/// <summary>
/// DTO para la Consulta 9: Detección de Clientes Inactivos sin Compras.
/// Cruza 4 tablas: cliente, persona, empresa y factura (mediante LEFT JOIN + HAVING COUNT=0).
/// </summary>
public record ConsultaClientesInactivosDto(
    int IdCliente,
    string NombrePersona,
    string EmailContacto,
    string TelefonoContacto,
    string NombreEmpresa,
    decimal Credito,
    int FacturasAsociadas,
    string EstadoActividad
);

/// <summary>
/// DTO para la Consulta 10: Auditoría de Facturas Anuladas e Impacto Financiero.
/// Cruza 5 tablas: factura, vendedor, persona, cliente y productosporfactura.
/// </summary>
public record ConsultaFacturasAnuladasDto(
    int NumeroFactura,
    DateTime FechaFactura,
    string VendedorResponsable,
    int CarnetVendedor,
    int IdClienteAfectado,
    decimal MontoAnulado,
    int CantidadRenglonesRevertidos
);

/// <summary>
/// DTO para las métricas consolidadas del Tablero de Control (Dashboard).
/// Agrega indicadores de rendimiento (KPIs) en tiempo real para gerencia.
/// </summary>
public record DashboardKpisDto(
    decimal VentasTotales,
    int TotalFacturas,
    int FacturasActivas,
    int FacturasAnuladas,
    int TotalClientes,
    int TotalVendedores,
    int TotalProductos,
    int ProductosStockCritico,
    decimal TicketPromedio
);
