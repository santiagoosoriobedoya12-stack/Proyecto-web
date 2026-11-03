using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Repositorio de Usuario en SQL Server.
/// Por seguridad, las consultas de lectura SELECT NUNCA traen la columna 'contrasena'.
/// </summary>
public class RepositorioUsuarioSqlServer : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "contrasena"
    };

    public RepositorioUsuarioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Usuario>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) email 
            FROM usuario 
            ORDER BY email";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Usuario>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        const string sql = @"
            SELECT email 
            FROM usuario 
            WHERE email = @email";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Usuario>(sql, new { email });
    }

    public async Task CrearAsync(string email, string contrasena)
    {
        const string sql = @"
            INSERT INTO usuario (email, contrasena) 
            VALUES (@email, @contrasena)";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new { email, contrasena });
    }

    public async Task<int> ActualizarAsync(string email, Dictionary<string, object> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("email_clave", email);

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

        var sql = $"UPDATE usuario SET {string.Join(", ", clausulasSet)} WHERE email = @email_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(string email)
    {
        const string sql = "DELETE FROM usuario WHERE email = @email";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { email });
    }
}

