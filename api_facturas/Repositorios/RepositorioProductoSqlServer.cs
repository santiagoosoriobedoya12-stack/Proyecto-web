// Enumeraciones ADO.NET (DbType, ParameterDirection) para parámetros OUTPUT en Stored Procedures
using System.Data;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación del repositorio de Producto contra el motor SQL Server 2022.
/// Reglas de la constitución aplicadas:
/// 1. SQL A LA VISTA: Todo el SQL se escribe a mano; Dapper solo ejecuta y mapea (sin ORM pesado).
/// 2. SQL PARAMETRIZADO: Todos los valores pasan por parámetros (@parametro) para evitar inyecciones SQL.
/// 3. ASÍNCRONO: Conexiones y consultas usan 'await using' y llamadas Async.
/// </summary>
public class RepositorioProductoSqlServer : IRepositorioProducto
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    /// <summary>
    /// Lista blanca de columnas permitidas para actualización.
    /// Evita ataques de inyección SQL o modificaciones en columnas no autorizadas durante el UPDATE.
    /// </summary>
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre", "stock", "valorunitario"
    };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioProductoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    /// <summary>
    /// Consulta productos ordenados por código aplicando límite superior (TOP-N).
    /// En SQL Server, SELECT TOP (@limite) acepta parámetros enteros directamente.
    /// </summary>
    public async Task<List<Producto>> ObtenerTodosAsync(int limite)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT TOP (@limite) codigo, nombre, stock, valorunitario 
            FROM producto 
            ORDER BY codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<Producto>(sql, new { limite });
        return resultado.AsList();
    }

    /// <summary>
    /// Obtiene un producto por su clave primaria exacta.
    /// </summary>
    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT codigo, nombre, stock, valorunitario 
            FROM producto 
            WHERE codigo = @codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Busca un único registro por clave con Dapper; devuelve null si no existe en BD
        return await conexion.QueryFirstOrDefaultAsync<Producto>(sql, new { codigo });
    }

    /// <summary>
    /// Inserta un nuevo producto en la tabla con sus 4 campos obligatorios.
    /// </summary>
    public async Task CrearAsync(Producto producto)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO producto (codigo, nombre, stock, valorunitario) 
            VALUES (@Codigo, @Nombre, @Stock, @Valorunitario)";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync(sql, producto);
    }

    /// <summary>
    /// Actualiza campos dinámicamente tanto para PUT (los 3 campos) como para PATCH (solo los enviados).
    /// El SET se construye exclusivamente con las columnas que estén en la lista blanca.
    /// </summary>
    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("codigo_clave", codigo);

        foreach (var par in datos)
        {
            var columna = par.Key.ToLower();
            if (ColumnasPermitidas.Contains(columna))
            {
                clausulasSet.Add($"{columna} = @{columna}");
                // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
                parametros.Add(columna, par.Value);
            }
        }

        if (clausulasSet.Count == 0)
            return 0;

        // Construye dinámicamente la sentencia SQL segura usando únicamente columnas de la lista blanca
        var sql = $"UPDATE producto SET {string.Join(", ", clausulasSet)} WHERE codigo = @codigo_clave";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, parametros);
    }

    /// <summary>
    /// Elimina el producto por código y devuelve la cantidad de filas afectadas (0 o 1).
    /// </summary>
    public async Task<int> EliminarAsync(string codigo)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = "DELETE FROM producto WHERE codigo = @codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}

