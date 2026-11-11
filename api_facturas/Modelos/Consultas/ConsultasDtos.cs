namespace ApiFacturas.Modelos.Consultas;

/// <summary>
/// DTO (Data Transfer Object) inmutable para la Consulta 1: Consolidado Maestro de Facturación.
/// Cruza 6 tablas: factura, cliente, persona (cliente), empresa (cliente), vendedor y persona (vendedor).
/// </summary>
/// <param name="NumeroFactura">Identificador único correlativo de la factura (PK en 'factura').</param>
/// <param name="Fecha">Fecha y hora de expedición de la factura.</param>
/// <param name="Total">Monto total liquidado de la factura.</param>
/// <param name="Estado">Estado actual de la factura ('activa' o 'anulada').</param>
/// <param name="ClienteNombre">Nombre unificado del cliente (persona o razón social de empresa vía COALESCE).</param>
/// <param name="ClienteTipo">Clasificación calculada: 'Persona Natural', 'Empresa Jurídica' o 'Persona y Empresa'.</param>
/// <param name="ClienteContacto">Dato de contacto principal (correo electrónico o código de empresa).</param>
/// <param name="VendedorNombre">Nombre completo del vendedor que atendió y emitió la factura.</param>
/// <param name="VendedorCarnet">Número de carnet institucional del vendedor.</param>
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
/// <param name="CodigoProducto">Código único del producto (PK en 'producto').</param>
/// <param name="NombreProducto">Nombre comercial del producto.</param>
/// <param name="ValorUnitario">Precio unitario base de venta.</param>
/// <param name="StockActual">Unidades físicas disponibles en bodega.</param>
/// <param name="UnidadesVendidas">Suma acumulada de cantidades vendidas en facturas activas.</param>
/// <param name="TotalRecaudado">Suma acumulada de subtotales generados por este producto.</param>
/// <param name="TotalFacturasPresente">Número de facturas distintas donde se ha vendido el producto.</param>
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
/// <param name="IdVendedor">Identificador único del vendedor.</param>
/// <param name="Carnet">Número de carnet institucional del vendedor.</param>
/// <param name="NombreVendedor">Nombre de la persona asociada al vendedor.</param>
/// <param name="EmailVendedor">Correo electrónico del vendedor.</param>
/// <param name="TotalFacturas">Cantidad total de facturas emitidas por el vendedor.</param>
/// <param name="FacturasActivas">Cantidad de facturas válidas y vigentes.</param>
/// <param name="FacturasAnuladas">Cantidad de facturas que fueron anuladas posteriormente.</param>
/// <param name="MontoTotalVendido">Total de dinero recaudado en facturas activas.</param>
/// <param name="TicketPromedio">Promedio facturado por cada venta activa (MontoTotal / Activas).</param>
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
/// <param name="IdCliente">Identificador único del cliente.</param>
/// <param name="NombreTitular">Nombre de la persona natural o razón social titular.</param>
/// <param name="EmpresaAsociada">Empresa corporativa a la que pertenece o 'Particular'.</param>
/// <param name="CreditoAsignado">Límite o cupo de crédito aprobado.</param>
/// <param name="TotalFacturas">Número de compras efectuadas en facturas activas.</param>
/// <param name="MontoTotalComprado">Suma monetaria histórica comprada por el cliente.</param>
/// <param name="VariedadProductosComprados">Cantidad de códigos de productos diferentes que ha adquirido.</param>
/// <param name="FechaUltimaCompra">Fecha y hora de la transacción más reciente.</param>
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
/// <param name="CodigoProducto">Código del producto.</param>
/// <param name="NombreProducto">Nombre del producto.</param>
/// <param name="StockActual">Existencias físicas actuales en bodega.</param>
/// <param name="ValorUnitario">Precio unitario de venta.</param>
/// <param name="TotalVendidoHistorico">Total de unidades despachadas históricamente.</param>
/// <param name="NivelAlerta">Semáforo de inventario: 'CRÍTICO' (<=5), 'BAJO' (<=15) o 'ESTABLE'.</param>
/// <param name="FechaUltimaVenta">Fecha de la última factura en que se despachó el producto.</param>
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
/// <param name="EmailUsuario">Correo electrónico del usuario (identificador de autenticación).</param>
/// <param name="RolesAsignados">Nombres de roles concatenados con STRING_AGG.</param>
/// <param name="CantidadRoles">Número de roles distintos asignados al usuario.</param>
/// <param name="CantidadRutasPermitidas">Cantidad de permisos de acceso a endpoints/vistas.</param>
/// <param name="RutasDetalle">Listado consolidado de patrones de rutas autorizadas.</param>
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
/// <param name="NumeroFactura">Número de factura a la que pertenece el renglón.</param>
/// <param name="FechaFactura">Fecha de emisión de la factura.</param>
/// <param name="ClienteNombre">Nombre del cliente comprador.</param>
/// <param name="CodigoProducto">Código del producto en este renglón.</param>
/// <param name="NombreProducto">Nombre del producto en este renglón.</param>
/// <param name="Cantidad">Unidades despachadas en este renglón.</param>
/// <param name="PrecioUnitarioCalculado">Precio unitario efectivo (subtotal / cantidad).</param>
/// <param name="SubtotalRenglon">Monto parcial del renglón (precio * cantidad).</param>
/// <param name="TotalFactura">Monto total de la factura completa.</param>
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
/// <param name="IdCliente">Identificador del cliente.</param>
/// <param name="PersonaNombre">Nombre de la persona asociada.</param>
/// <param name="EmpresaNombre">Nombre de la empresa asociada.</param>
/// <param name="LimiteCredito">Cupo de crédito asignado al cliente.</param>
/// <param name="TotalFacturadoActivo">Suma de facturas activas generadas por el cliente.</param>
/// <param name="CreditoDisponible">Diferencia disponible (Límite - Facturado).</param>
/// <param name="EstadoCredito">Semáforo financiero: 'NORMAL', 'RIESGO ALTO' (>=80%) o 'EXCEDIDO'.</param>
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
/// <param name="IdCliente">Identificador del cliente.</param>
/// <param name="NombrePersona">Nombre de la persona registrada.</param>
/// <param name="EmailContacto">Correo electrónico de contacto.</param>
/// <param name="TelefonoContacto">Teléfono de contacto.</param>
/// <param name="NombreEmpresa">Razón social de empresa o 'Particular'.</param>
/// <param name="Credito">Límite de crédito disponible sin usar.</param>
/// <param name="FacturasAsociadas">Cantidad de facturas activas (siempre 0 para este reporte).</param>
/// <param name="EstadoActividad">Descripción fija del estado comercial del cliente.</param>
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
/// <param name="NumeroFactura">Número de la factura anulada.</param>
/// <param name="FechaFactura">Fecha en que fue emitida y luego anulada.</param>
/// <param name="VendedorResponsable">Nombre del vendedor que generó la factura.</param>
/// <param name="CarnetVendedor">Carnet institucional del vendedor.</param>
/// <param name="IdClienteAfectado">Identificador del cliente al que se le anuló.</param>
/// <param name="MontoAnulado">Monto total de dinero no percibido debido a la anulación.</param>
/// <param name="CantidadRenglonesRevertidos">Total de líneas de productos devueltas al inventario.</param>
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
/// <param name="VentasTotales">Suma total del dinero facturado en facturas con estado 'activa'.</param>
/// <param name="TotalFacturas">Conteo histórico absoluto de facturas registradas en la base de datos.</param>
/// <param name="FacturasActivas">Conteo de facturas actualmente vigentes y no anuladas.</param>
/// <param name="FacturasAnuladas">Conteo de facturas que fueron anuladas contablemente.</param>
/// <param name="TotalClientes">Número total de clientes registrados en el sistema.</param>
/// <param name="TotalVendedores">Número total de vendedores registrados en el equipo comercial.</param>
/// <param name="TotalProductos">Catálogo total de productos registrados en inventario.</param>
/// <param name="ProductosStockCritico">Cantidad de productos con existencias iguales o inferiores a 15.</param>
/// <param name="TicketPromedio">Valor promedio monetario por cada factura activa emitida.</param>
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
