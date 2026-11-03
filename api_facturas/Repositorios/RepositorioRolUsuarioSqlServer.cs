using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioRolUsuario en SQL Server con Dapper.
/// </summary>
public class RepositorioRolUsuarioSqlServer : IRepositorioRolUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioRolUsuarioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<RolUsuario>> ObtenerTodosAsync()
    {
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            ORDER BY ru.fkemail, ru.fkidrol";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<RolUsuario>(sql);
        return resultado.AsList();
    }

    public async Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email)
    {
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            WHERE ru.fkemail = @email
            ORDER BY r.nombre";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<RolUsuario>(sql, new { email });
        return resultado.AsList();
    }

    public async Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol)
    {
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            WHERE ru.fkidrol = @idRol
            ORDER BY ru.fkemail";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<RolUsuario>(sql, new { idRol });
        return resultado.AsList();
    }

    public async Task<bool> ExisteAsync(string email, int idRol)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM rol_usuario 
            WHERE fkemail = @email AND fkidrol = @idRol";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var cantidad = await conexion.ExecuteScalarAsync<int>(sql, new { email, idRol });
        return cantidad > 0;
    }

    public async Task<int> AgregarAsync(string email, int idRol)
    {
        const string sql = @"
            INSERT INTO rol_usuario (fkemail, fkidrol) 
            VALUES (@email, @idRol)";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { email, idRol });
    }

    public async Task<int> EliminarAsync(string email, int idRol)
    {
        const string sql = @"
            DELETE FROM rol_usuario 
            WHERE fkemail = @email AND fkidrol = @idRol";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { email, idRol });
    }
}
