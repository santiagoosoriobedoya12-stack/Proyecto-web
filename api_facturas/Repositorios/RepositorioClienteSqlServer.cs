using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioCliente sobre Microsoft SQL Server utilizando Dapper.
/// 
/// Características pedagógicas:
/// 1. Emplea consultas SQL parametrizadas para evitar ataques de inyección SQL.
/// 2. Utiliza SCOPE_IDENTITY() para capturar el 'id' autoincremental tras el INSERT.
/// 3. Lista blanca (ColumnasPermitidas) para construir la cláusula UPDATE de forma segura.
/// 4. Las violaciones de restricciones de integridad foránea (FK) disparan SqlException (código 547).
/// </summary>
public class RepositorioClienteSqlServer : IRepositorioCliente
{
    private readonly string _cadenaConexion;

    // Lista blanca de columnas editables para evitar inyecciones en sentencias dinámicas
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "credito",
        "fkcodpersona",
        "fkcodempresa"
    };

    public RepositorioClienteSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    /// <summary>
    /// Consulta los clientes ordenados por 'id', aplicando el límite especificado con TOP.
    /// </summary>
    public async Task<List<Cliente>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) id, credito, fkcodpersona, fkcodempresa 
            FROM cliente 
            ORDER BY id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Cliente>(sql, new { limite });
        return resultado.AsList();
    }

    /// <summary>
    /// Busca un cliente por su clave primaria 'id'.
    /// </summary>
    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id, credito, fkcodpersona, fkcodempresa 
            FROM cliente 
            WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Cliente>(sql, new { id });
    }

    /// <summary>
    /// Inserta el cliente y retorna el nuevo ID asignado por SQL Server con SCOPE_IDENTITY().
    /// Si fkcodpersona o fkcodempresa no existen en la base de datos, el motor lanza SqlException (547).
    /// </summary>
    public async Task<int> CrearAsync(Cliente cliente)
    {
        const string sql = @"
            INSERT INTO cliente (credito, fkcodpersona, fkcodempresa) 
            VALUES (@Credito, @Fkcodpersona, @Fkcodempresa);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, cliente);
    }

    /// <summary>
    /// Actualiza selectivamente las columnas provistas en el diccionario.
    /// Maneja valores nulos en columnas como 'fkcodempresa'.
    /// </summary>
    public async Task<int> ActualizarAsync(int id, Dictionary<string, object?> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("id_clave", id);

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

        var sql = $"UPDATE cliente SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    /// <summary>
    /// Elimina el registro del cliente por su clave primaria.
    /// Si el cliente ya tiene facturas asociadas, el motor lanzará una excepción por integridad referencial (547).
    /// </summary>
    public async Task<int> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM cliente WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

