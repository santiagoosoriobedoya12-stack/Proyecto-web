using System.Data;
using System.Text.Json;
using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

public class RepositorioRutaRolSqlServer : IRepositorioRutaRol
{
    private readonly string _cadenaConexion;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public RepositorioRutaRolSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<RutaRol>> ObtenerTodosAsync()
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        await conexion.ExecuteAsync(
            "listar_rutarol",
            parametros,
            commandType: CommandType.StoredProcedure
        );

        var json = parametros.Get<string>("@p_resultado");
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<RutaRol>();

        return JsonSerializer.Deserialize<List<RutaRol>>(json, JsonOpts) ?? new List<RutaRol>();
    }

    public async Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta)
    {
        const string sql = @"
            SELECT rr.fkidruta, rt.ruta, rr.fkidrol, r.nombre AS rol
            FROM rutarol rr
            JOIN ruta rt ON rt.id = rr.fkidruta
            JOIN rol r ON r.id = rr.fkidrol
            WHERE rr.fkidruta = @idRuta
            ORDER BY r.nombre";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<RutaRol>(sql, new { idRuta });
        return resultado.AsList();
    }

    public async Task<List<RutaRol>> ObtenerPorRolAsync(int idRol)
    {
        const string sql = @"
            SELECT rr.fkidruta, rt.ruta, rr.fkidrol, r.nombre AS rol
            FROM rutarol rr
            JOIN ruta rt ON rt.id = rr.fkidruta
            JOIN rol r ON r.id = rr.fkidrol
            WHERE rr.fkidrol = @idRol
            ORDER BY rt.ruta";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<RutaRol>(sql, new { idRol });
        return resultado.AsList();
    }

    public async Task<bool> ExisteAsync(int idRuta, int idRol)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM rutarol 
            WHERE fkidruta = @idRuta AND fkidrol = @idRol";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var cantidad = await conexion.ExecuteScalarAsync<int>(sql, new { idRuta, idRol });
        return cantidad > 0;
    }

    public async Task<int> AgregarAsync(int idRuta, int idRol)
    {
        const string sql = @"
            INSERT INTO rutarol (fkidruta, fkidrol) 
            VALUES (@idRuta, @idRol)";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { idRuta, idRol });
    }

    public async Task<int> EliminarAsync(int idRuta, int idRol)
    {
        const string sql = @"
            DELETE FROM rutarol 
            WHERE fkidruta = @idRuta AND fkidrol = @idRol";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { idRuta, idRol });
    }
}
