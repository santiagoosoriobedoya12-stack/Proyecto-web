using System.Data;
using ApiFacturas.Modelos.Consultas;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Repositorio de alto rendimiento para las 10 Consultas Multitabla Complejas y el Tablero de Control (Dashboard).
/// 
/// REGLAS DE ARQUITECTURA:
/// 1. Micro-ORM Dapper: Mapeo directo y ultra liviano desde SQL Server hacia DTOs inmutables en memoria.
/// 2. Sin Change-Tracking: Al ser consultas de sólo lectura analítica, no se genera sobrecarga de contexto ni locks persistentes.
/// 3. Procesamiento en el Motor: Las sumas, promedios, conteos y agrupamientos los ejecuta SQL Server aprovechando índices y hardware del servidor.
/// 4. Respeto al Modelo: Cada consulta navega por el grafo relacional cruzando entre 4 y 6 tablas de las 12 existentes en 'bdfacturas'.
/// </summary>
public class RepositorioConsultasSqlServer : IRepositorioConsultas
{
    // Cadena de conexión inyectada desde la configuración (appsettings o variables de entorno de Docker)
    private readonly string _cadenaConexion;

    /// <summary>
    /// Constructor: Recibe la cadena de conexión a la base de datos SQL Server y valida que no sea nula.
    /// </summary>
    public RepositorioConsultasSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    /// <summary>
    /// CONSULTA 1: Consolidado Maestro de Facturación con Detalle de Partes.
    /// Cruza 6 tablas: factura (f) -> cliente (c) -> persona (pc) / empresa (ec) -> vendedor (v) -> persona (pv).
    /// Resuelve el titular del cliente usando COALESCE para admitir personas naturales o jurídicas.
    /// </summary>
    public async Task<IEnumerable<ConsultaFacturaDetalladaDto>> ObtenerC1ConsolidadoFacturacionAsync()
    {
        // Sentencia SQL analítica con JOINs explícitos y resolución condicional de tipos
        const string sql = @"
            SELECT 
                f.numero AS NumeroFactura,
                f.fecha AS Fecha,
                f.total AS Total,
                f.estado AS Estado,
                COALESCE(pc.nombre, ec.nombre, 'Sin titular') AS ClienteNombre,
                CASE 
                    WHEN pc.codigo IS NOT NULL AND ec.codigo IS NOT NULL THEN 'Persona y Empresa'
                    WHEN pc.codigo IS NOT NULL THEN 'Persona Natural'
                    ELSE 'Empresa Jurídica'
                END AS ClienteTipo,
                COALESCE(pc.email, ec.codigo) AS ClienteContacto,
                pv.nombre AS VendedorNombre,
                v.carnet AS VendedorCarnet
            FROM factura f
            INNER JOIN cliente c ON f.fkidcliente = c.id
            LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
            LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
            INNER JOIN vendedor v ON f.fkidvendedor = v.id
            INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
            ORDER BY f.fecha DESC;";

        // Apertura asíncrona de conexión que se cerrará automáticamente al salir del bloque using
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Dapper ejecuta la consulta de forma no bloqueante y mapea automáticamente las columnas a las propiedades del DTO
        return await conexion.QueryAsync<ConsultaFacturaDetalladaDto>(sql);
    }

    /// <summary>
    /// CONSULTA 2: Ranking de Productos más Vendidos y Rentabilidad Generada.
    /// Cruza 4 tablas: producto (p) -> productosporfactura (pf) -> factura (f) -> cliente (c).
    /// Agrupa por producto y calcula volumen de unidades y dinero neto en facturas activas.
    /// </summary>
    public async Task<IEnumerable<ConsultaRankingProductosDto>> ObtenerC2RankingProductosAsync()
    {
        const string sql = @"
            SELECT 
                p.codigo AS CodigoProducto,
                p.nombre AS NombreProducto,
                p.valorunitario AS ValorUnitario,
                p.stock AS StockActual,
                COALESCE(SUM(pf.cantidad), 0) AS UnidadesVendidas,
                COALESCE(SUM(pf.subtotal), 0) AS TotalRecaudado,
                COUNT(DISTINCT f.numero) AS TotalFacturasPresente
            FROM producto p
            INNER JOIN productosporfactura pf ON p.codigo = pf.fkcodproducto
            INNER JOIN factura f ON pf.fknumfactura = f.numero
            INNER JOIN cliente c ON f.fkidcliente = c.id
            WHERE f.estado = 'activa'
            GROUP BY p.codigo, p.nombre, p.valorunitario, p.stock
            ORDER BY TotalRecaudado DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaRankingProductosDto>(sql);
    }

    /// <summary>
    /// CONSULTA 3: Desempeño Comercial y Productividad por Vendedor.
    /// Cruza 5 tablas: vendedor (v) -> persona (pv) -> factura (f) -> productosporfactura (pf) -> producto (p).
    /// Emplea LEFT JOIN para incluir a todos los vendedores y suma condicional con CASE para separar facturas activas de anuladas.
    /// </summary>
    public async Task<IEnumerable<ConsultaRendimientoVendedoresDto>> ObtenerC3RendimientoVendedoresAsync()
    {
        const string sql = @"
            SELECT 
                v.id AS IdVendedor,
                v.carnet AS Carnet,
                pv.nombre AS NombreVendedor,
                pv.email AS EmailVendedor,
                COUNT(DISTINCT f.numero) AS TotalFacturas,
                SUM(CASE WHEN f.estado = 'activa' THEN 1 ELSE 0 END) AS FacturasActivas,
                SUM(CASE WHEN f.estado = 'anulada' THEN 1 ELSE 0 END) AS FacturasAnuladas,
                COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS MontoTotalVendido,
                COALESCE(AVG(CASE WHEN f.estado = 'activa' THEN f.total ELSE NULL END), 0) AS TicketPromedio
            FROM vendedor v
            INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
            LEFT JOIN factura f ON v.id = f.fkidvendedor
            LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
            LEFT JOIN producto p ON pf.fkcodproducto = p.codigo
            GROUP BY v.id, v.carnet, pv.nombre, pv.email
            ORDER BY MontoTotalVendido DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaRendimientoVendedoresDto>(sql);
    }

    /// <summary>
    /// CONSULTA 4: Segmentación de Clientes de Alto Valor (Clientes VIP / Regla de Pareto).
    /// Cruza 5 tablas: cliente (c) -> persona (pc) -> empresa (ec) -> factura (f) -> productosporfactura (pf).
    /// Identifica a los clientes con mayor volumen de compras, variedad de productos adquiridos y fecha de última compra.
    /// </summary>
    public async Task<IEnumerable<ConsultaClientesAltoValorDto>> ObtenerC4ClientesAltoValorAsync()
    {
        const string sql = @"
            SELECT 
                c.id AS IdCliente,
                COALESCE(pc.nombre, ec.nombre, 'Sin nombre') AS NombreTitular,
                COALESCE(ec.nombre, 'Particular') AS EmpresaAsociada,
                c.credito AS CreditoAsignado,
                COUNT(DISTINCT f.numero) AS TotalFacturas,
                COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS MontoTotalComprado,
                COUNT(DISTINCT pf.fkcodproducto) AS VariedadProductosComprados,
                MAX(f.fecha) AS FechaUltimaCompra
            FROM cliente c
            LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
            LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
            INNER JOIN factura f ON c.id = f.fkidcliente
            LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
            WHERE f.estado = 'activa'
            GROUP BY c.id, pc.nombre, ec.nombre, c.credito
            ORDER BY MontoTotalComprado DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaClientesAltoValorDto>(sql);
    }

    /// <summary>
    /// CONSULTA 5: Alerta Preventiva de Stock Crítico y Rotación de Demanda.
    /// Cruza 4 tablas: producto (p) -> productosporfactura (pf) -> factura (f) -> vendedor (v).
    /// Clasifica el nivel de riesgo de desabastecimiento según el inventario físico remanente.
    /// </summary>
    public async Task<IEnumerable<ConsultaStockCriticoDto>> ObtenerC5StockCriticoAsync()
    {
        const string sql = @"
            SELECT 
                p.codigo AS CodigoProducto,
                p.nombre AS NombreProducto,
                p.stock AS StockActual,
                p.valorunitario AS ValorUnitario,
                COALESCE(SUM(pf.cantidad), 0) AS TotalVendidoHistorico,
                CASE 
                    WHEN p.stock <= 5 THEN 'CRÍTICO'
                    WHEN p.stock <= 15 THEN 'BAJO'
                    ELSE 'ESTABLE'
                END AS NivelAlerta,
                MAX(f.fecha) AS FechaUltimaVenta
            FROM producto p
            LEFT JOIN productosporfactura pf ON p.codigo = pf.fkcodproducto
            LEFT JOIN factura f ON pf.fknumfactura = f.numero
            LEFT JOIN vendedor v ON f.fkidvendedor = v.id
            GROUP BY p.codigo, p.nombre, p.stock, p.valorunitario
            ORDER BY p.stock ASC, TotalVendidoHistorico DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaStockCriticoDto>(sql);
    }

    /// <summary>
    /// CONSULTA 6: Auditoría de Seguridad, Roles y Trazabilidad de Permisos de Usuario.
    /// Cruza 5 tablas: usuario (u) -> rol_usuario (ru) -> rol (r) -> rutarol (rr) -> ruta (rt).
    /// Emplea STRING_AGG para consolidar los nombres de roles y rutas asignadas en un solo registro por usuario.
    /// </summary>
    public async Task<IEnumerable<ConsultaAuditoriaSeguridadDto>> ObtenerC6AuditoriaSeguridadAsync()
    {
        const string sql = @"
            SELECT 
                u.email AS EmailUsuario,
                STRING_AGG(r.nombre, ', ') AS RolesAsignados,
                COUNT(DISTINCT r.id) AS CantidadRoles,
                COUNT(DISTINCT rt.id) AS CantidadRutasPermitidas,
                STRING_AGG(rt.ruta, ' | ') AS RutasDetalle
            FROM usuario u
            INNER JOIN rol_usuario ru ON u.email = ru.fkemail
            INNER JOIN rol r ON ru.fkidrol = r.id
            LEFT JOIN rutarol rr ON r.id = rr.fkidrol
            LEFT JOIN ruta rt ON rr.fkidruta = rt.id
            GROUP BY u.email
            ORDER BY CantidadRutasPermitidas DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaAuditoriaSeguridadDto>(sql);
    }

    /// <summary>
    /// CONSULTA 7: Renglones Detallados de Facturación Transaccional.
    /// Cruza 5 tablas: factura (f) -> productosporfactura (pf) -> producto (p) -> cliente (c) -> persona (pc).
    /// Expone cada ítem facturado con su precio unitario, cantidad y subtotal para auditoría fiscal.
    /// </summary>
    public async Task<IEnumerable<ConsultaRenglonesFacturaDto>> ObtenerC7RenglonesFacturaAsync()
    {
        const string sql = @"
            SELECT 
                f.numero AS NumeroFactura,
                f.fecha AS FechaFactura,
                COALESCE(pc.nombre, 'Empresa Jurídica') AS ClienteNombre,
                p.codigo AS CodigoProducto,
                p.nombre AS NombreProducto,
                pf.cantidad AS Cantidad,
                pf.subtotal / NULLIF(pf.cantidad, 0) AS PrecioUnitarioCalculado,
                pf.subtotal AS SubtotalRenglon,
                f.total AS TotalFactura
            FROM factura f
            INNER JOIN productosporfactura pf ON f.numero = pf.fknumfactura
            INNER JOIN producto p ON pf.fkcodproducto = p.codigo
            INNER JOIN cliente c ON f.fkidcliente = c.id
            LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
            ORDER BY f.numero DESC, pf.cantidad DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaRenglonesFacturaDto>(sql);
    }

    /// <summary>
    /// CONSULTA 8: Utilización de Línea de Crédito y Exposición Financiera.
    /// Cruza 4 tablas: cliente (c) -> persona (pc) -> empresa (ec) -> factura (f).
    /// Compara el cupo de crédito otorgado frente al saldo facturado activo, calculando el cupo restante y el semáforo de riesgo.
    /// </summary>
    public async Task<IEnumerable<ConsultaCarteraCreditoDto>> ObtenerC8CarteraCreditoAsync()
    {
        const string sql = @"
            SELECT 
                c.id AS IdCliente,
                COALESCE(pc.nombre, 'Sin persona') AS PersonaNombre,
                COALESCE(ec.nombre, 'Sin empresa') AS EmpresaNombre,
                c.credito AS LimiteCredito,
                COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS TotalFacturadoActivo,
                (c.credito - COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0)) AS CreditoDisponible,
                CASE 
                    WHEN COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) > c.credito THEN 'EXCEDIDO'
                    WHEN COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) >= (c.credito * 0.8) THEN 'RIESGO ALTO'
                    ELSE 'NORMAL'
                END AS EstadoCredito
            FROM cliente c
            LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
            LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
            LEFT JOIN factura f ON c.id = f.fkidcliente
            GROUP BY c.id, pc.nombre, ec.nombre, c.credito
            ORDER BY TotalFacturadoActivo DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaCarteraCreditoDto>(sql);
    }

    /// <summary>
    /// CONSULTA 9: Detección de Clientes Inactivos sin Compras Registradas.
    /// Cruza 4 tablas: cliente (c) -> persona (pc) -> empresa (ec) -> factura (f).
    /// Emplea LEFT JOIN filtrado por HAVING COUNT(facturas activas) = 0 para extraer clientes potenciales sin movimiento.
    /// </summary>
    public async Task<IEnumerable<ConsultaClientesInactivosDto>> ObtenerC9ClientesInactivosAsync()
    {
        const string sql = @"
            SELECT 
                c.id AS IdCliente,
                COALESCE(pc.nombre, 'Persona no registrada') AS NombrePersona,
                pc.email AS EmailContacto,
                pc.telefono AS TelefonoContacto,
                COALESCE(ec.nombre, 'Particular') AS NombreEmpresa,
                c.credito AS Credito,
                COUNT(f.numero) AS FacturasAsociadas,
                'Sin compras activas registradas' AS EstadoActividad
            FROM cliente c
            LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
            LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
            LEFT JOIN factura f ON c.id = f.fkidcliente AND f.estado = 'activa'
            GROUP BY c.id, pc.nombre, pc.email, pc.telefono, ec.nombre, c.credito
            HAVING COUNT(f.numero) = 0
            ORDER BY c.id ASC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaClientesInactivosDto>(sql);
    }

    /// <summary>
    /// CONSULTA 10: Auditoría de Facturas Anuladas e Impacto Financiero.
    /// Cruza 5 tablas: factura (f) -> vendedor (v) -> persona (pv) -> cliente (c) -> productosporfactura (pf).
    /// Totaliza el valor de ventas revertidas, el vendedor emisor y el número de renglones devueltos a stock.
    /// </summary>
    public async Task<IEnumerable<ConsultaFacturasAnuladasDto>> ObtenerC10FacturasAnuladasAsync()
    {
        const string sql = @"
            SELECT 
                f.numero AS NumeroFactura,
                f.fecha AS FechaFactura,
                pv.nombre AS VendedorResponsable,
                v.carnet AS CarnetVendedor,
                c.id AS IdClienteAfectado,
                f.total AS MontoAnulado,
                COUNT(pf.fkcodproducto) AS CantidadRenglonesRevertidos
            FROM factura f
            INNER JOIN vendedor v ON f.fkidvendedor = v.id
            INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
            INNER JOIN cliente c ON f.fkidcliente = c.id
            LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
            WHERE f.estado = 'anulada'
            GROUP BY f.numero, f.fecha, pv.nombre, v.carnet, c.id, f.total
            ORDER BY f.fecha DESC;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<ConsultaFacturasAnuladasDto>(sql);
    }

    /// <summary>
    /// MÉTRICAS DEL TABLERO DE CONTROL (DASHBOARD KPIs):
    /// Ejecuta una sola consulta agregada calculando totales en vivo directamente en el motor:
    /// - Suma total de dinero en facturas activas
    /// - Conteo de facturas activas vs anuladas
    /// - Conteo total de clientes, vendedores y productos
    /// - Productos en riesgo de inventario (stock <= 15)
    /// - Ticket promedio por venta activa
    /// </summary>
    public async Task<DashboardKpisDto> ObtenerDashboardKpisAsync()
    {
        const string sql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN estado = 'activa' THEN total ELSE 0 END), 0) AS VentasTotales,
                COUNT(*) AS TotalFacturas,
                SUM(CASE WHEN estado = 'activa' THEN 1 ELSE 0 END) AS FacturasActivas,
                SUM(CASE WHEN estado = 'anulada' THEN 1 ELSE 0 END) AS FacturasAnuladas,
                (SELECT COUNT(*) FROM cliente) AS TotalClientes,
                (SELECT COUNT(*) FROM vendedor) AS TotalVendedores,
                (SELECT COUNT(*) FROM producto) AS TotalProductos,
                (SELECT COUNT(*) FROM producto WHERE stock <= 15) AS ProductosStockCritico,
                COALESCE(AVG(CASE WHEN estado = 'activa' THEN total ELSE NULL END), 0) AS TicketPromedio
            FROM factura;";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<DashboardKpisDto>(sql);
    }
}
