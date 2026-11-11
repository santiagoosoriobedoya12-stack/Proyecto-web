// Enumeraciones ADO.NET (DbType, ParameterDirection) para parámetros OUTPUT en Stored Procedures
using System.Data;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioFactura sobre SQL Server.
/// 
/// REGLAS CLAVE DE LA ESPECIFICACIÓN:
/// 1. NO escribe SQL plano de tablas: delega 100% en los 4 procedimientos almacenados oficiales.
/// 2. Lee los parámetros de salida OUTPUT (@p_resultado) generados por el motor como JSON.
/// 3. Traduce las excepciones específicas de T-SQL (THROW 50001, 50002, 50003, 50010 y 547)
///    a las excepciones de dominio del sistema (NoEncontradoExcepcion, ConflictoExcepcion, etc.).
/// </summary>
public class RepositorioFacturaSqlServer : IRepositorioFactura
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioFacturaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    /// <summary>
    /// Invoca 'sp_listar_facturas_y_productosporfactura'.
    /// Devuelve todas las facturas con sus nombres ya resueltos y los productos anidados.
    /// </summary>
    public async Task<List<Factura>> ListarAsync()
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync(
            "sp_listar_facturas_y_productosporfactura",
            parametros,
            commandType: CommandType.StoredProcedure
        );

        var resultadoJson = parametros.Get<string>("@p_resultado");
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(resultadoJson) || resultadoJson == "[]")
        {
            return new List<Factura>();
        }

        var facturas = JsonSerializer.Deserialize<List<Factura>>(resultadoJson, JsonOpts);
        return facturas ?? new List<Factura>();
    }

    /// <summary>
    /// Invoca 'sp_consultar_factura_y_productosporfactura'.
    /// Si la factura no existe, el procedimiento lanza THROW 50003 y se traduce a NoEncontradoExcepcion (404).
    /// </summary>
    public async Task<Factura> ConsultarAsync(int numero)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_numero", numero);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "sp_consultar_factura_y_productosporfactura",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var resultadoJson = parametros.Get<string>("@p_resultado");
            // Valida que el identificador o cadena requerida no llegue nula ni vacía
            if (string.IsNullOrWhiteSpace(resultadoJson))
            {
                // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
                throw new NoEncontradoExcepcion($"Factura {numero} no encontrada.");
            }

            using var doc = JsonDocument.Parse(resultadoJson);
            var raiz = doc.RootElement;

            var factura = raiz.GetProperty("factura").Deserialize<Factura>(JsonOpts)
                ?? throw new NoEncontradoExcepcion($"Factura {numero} no encontrada.");

            if (raiz.TryGetProperty("productos", out var productosElem) && productosElem.ValueKind == JsonValueKind.Array)
            {
                factura.Productos = productosElem.Deserialize<List<FacturaDetalle>>(JsonOpts) ?? new();
            }

            return factura;
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 50003 || ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"Factura {numero} no existe.");
        }
    }

    /// <summary>
    /// Invoca 'sp_insertar_factura_y_productosporfactura'.
    /// Inserta el encabezado y sus renglones en una transacción atómica gestionada por el motor.
    /// El disparador 'trg_prodfact_insert' calcula automáticamente subtotales, descuenta stock y fija el total.
    /// </summary>
    public async Task<Factura> CrearAsync(FacturaCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        var productosJson = JsonSerializer.Serialize(peticion.Productos);

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_fkidcliente", peticion.Fkidcliente!.Value);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_fkidvendedor", peticion.Fkidvendedor!.Value);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_productos", productosJson);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_minimo_detalle", 1);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "sp_insertar_factura_y_productosporfactura",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var resultadoJson = parametros.Get<string>("@p_resultado");
            using var doc = JsonDocument.Parse(resultadoJson);
            var raiz = doc.RootElement;

            var factura = raiz.GetProperty("factura").Deserialize<Factura>(JsonOpts)!;
            if (raiz.TryGetProperty("productos", out var productosElem) && productosElem.ValueKind == JsonValueKind.Array)
            {
                factura.Productos = productosElem.Deserialize<List<FacturaDetalle>>(JsonOpts) ?? new();
            }

            return factura;
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            // Violación de clave foránea: el cliente o vendedor no existe
            throw new ConflictoExcepcion("El cliente o vendedor seleccionado no existe en la base de datos.", ex);
        }
    }

    /// <summary>
    /// Invoca 'sp_anular_factura'.
    /// Cambia el estado a 'anulada' y devuelve el stock a los productos afectados.
    /// - Si la factura no existe -> 404
    /// - Si ya está previamente anulada -> 409
    /// </summary>
    public async Task<string> AnularAsync(int numero)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_numero", numero);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "sp_anular_factura",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return parametros.Get<string>("@p_resultado");
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 50010 && ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"Factura {numero} no existe.");
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 50010 && ex.Message.Contains("anulada", StringComparison.OrdinalIgnoreCase))
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion($"Factura {numero} ya está anulada.", ex);
        }
    }
}
