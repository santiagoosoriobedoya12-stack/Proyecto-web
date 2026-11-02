using System.Data;
using ApiFacturas.Modelos;
using Dapper;
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
    private readonly string _cadenaConexion;

    /// <summary>
    /// Lista blanca de columnas permitidas para actualización.
    /// Evita ataques de inyección SQL o modificaciones en columnas no autorizadas durante el UPDATE.
    /// </summary>
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre", "stock", "valorunitario"
    };

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
        const string sql = @"
            SELECT TOP (@limite) codigo, nombre, stock, valorunitario 
            FROM producto 
            ORDER BY codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Producto>(sql, new { limite });
        return resultado.AsList();
    }

    /// <summary>
    /// Obtiene un producto por su clave primaria exacta.
    /// </summary>
    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        const string sql = @"
            SELECT codigo, nombre, stock, valorunitario 
            FROM producto 
            WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Producto>(sql, new { codigo });
    }

    /// <summary>
    /// Inserta un nuevo producto en la tabla con sus 4 campos obligatorios.
    /// </summary>
    public async Task CrearAsync(Producto producto)
    {
        const string sql = @"
            INSERT INTO producto (codigo, nombre, stock, valorunitario) 
            VALUES (@Codigo, @Nombre, @Stock, @Valorunitario)";

        await using var conexion = new SqlConnection(_cadenaConexion);
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
        var parametros = new DynamicParameters();
        parametros.Add("codigo_clave", codigo);

        foreach (var par in datos)
        {
            var columna = par.Key.ToLower();
            if (ColumnasPermitidas.Contains(columna))
            {
                clausulasSet.Add($"{columna} = @{columna}");
                parametros.Add(columna, par.Value);
            }
        }

        if (clausulasSet.Count == 0)
            return 0;

        var sql = $"UPDATE producto SET {string.Join(", ", clausulasSet)} WHERE codigo = @codigo_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    /// <summary>
    /// Elimina el producto por código y devuelve la cantidad de filas afectadas (0 o 1).
    /// </summary>
    public async Task<int> EliminarAsync(string codigo)
    {
        const string sql = "DELETE FROM producto WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}

