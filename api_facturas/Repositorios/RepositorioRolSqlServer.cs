using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Repositorio de Rol en SQL Server con Dapper.
/// Utiliza SCOPE_IDENTITY() para capturar la clave primaria autogenerada en el INSERT.
/// </summary>
public class RepositorioRolSqlServer : IRepositorioRol
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre"
    };

    public RepositorioRolSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Rol>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) id, nombre 
            FROM rol 
            ORDER BY id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Rol>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id, nombre 
            FROM rol 
            WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Rol>(sql, new { id });
    }

    public async Task<int> CrearAsync(Rol rol)
    {
        const string sql = @"
            INSERT INTO rol (nombre) 
            VALUES (@Nombre);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, rol);
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
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

        var sql = $"UPDATE rol SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM rol WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

