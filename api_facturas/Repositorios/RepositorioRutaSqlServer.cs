using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Repositorio de Ruta contra SQL Server.
/// </summary>
public class RepositorioRutaSqlServer : IRepositorioRuta
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "ruta", "descripcion"
    };

    public RepositorioRutaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Ruta>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) id, ruta AS NombreRuta, descripcion 
            FROM ruta 
            ORDER BY id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Ruta>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Ruta?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id, ruta AS NombreRuta, descripcion 
            FROM ruta 
            WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Ruta>(sql, new { id });
    }

    public async Task<int> CrearAsync(Ruta ruta)
    {
        const string sql = @"
            INSERT INTO ruta (ruta, descripcion) 
            VALUES (@NombreRuta, @Descripcion);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, ruta);
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

        var sql = $"UPDATE ruta SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM ruta WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

