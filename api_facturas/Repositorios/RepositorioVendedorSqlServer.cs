using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioVendedor para SQL Server con Dapper.
/// </summary>
public class RepositorioVendedorSqlServer : IRepositorioVendedor
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "carnet",
        "direccion",
        "fkcodpersona"
    };

    public RepositorioVendedorSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Vendedor>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) id, carnet, direccion, fkcodpersona 
            FROM vendedor 
            ORDER BY id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Vendedor>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Vendedor?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT id, carnet, direccion, fkcodpersona 
            FROM vendedor 
            WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Vendedor>(sql, new { id });
    }

    public async Task<int> CrearAsync(Vendedor vendedor)
    {
        const string sql = @"
            INSERT INTO vendedor (carnet, direccion, fkcodpersona) 
            VALUES (@Carnet, @Direccion, @Fkcodpersona);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, vendedor);
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

        var sql = $"UPDATE vendedor SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int id)
    {
        const string sql = "DELETE FROM vendedor WHERE id = @id";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

