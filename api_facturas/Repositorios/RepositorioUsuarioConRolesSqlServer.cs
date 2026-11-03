using System.Data;
using System.Text.Json;
using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioUsuarioConRoles con Dapper en SQL Server.
/// Ejecuta los procedimientos almacenados oficiales pasando los parámetros ya preparados.
/// </summary>
public class RepositorioUsuarioConRolesSqlServer : IRepositorioUsuarioConRoles
{
    private readonly string _cadenaConexion;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public RepositorioUsuarioConRolesSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<UsuarioConRoles>> ListarAsync()
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        await conexion.ExecuteAsync(
            "listar_usuarios_con_roles",
            parametros,
            commandType: CommandType.StoredProcedure
        );

        var json = parametros.Get<string>("@p_resultado");
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<UsuarioConRoles>();

        return JsonSerializer.Deserialize<List<UsuarioConRoles>>(json, JsonOpts) ?? new List<UsuarioConRoles>();
    }

    public async Task<UsuarioConRoles> ConsultarAsync(string email)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_email", email);
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        try
        {
            await conexion.ExecuteAsync(
                "consultar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            if (string.IsNullOrWhiteSpace(json))
                throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");

            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)
                ?? throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
        catch (SqlException ex) when (ex.Number == 50008 || ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
    }

    public async Task<UsuarioConRoles> CrearAsync(string email, string contrasena, string rolesJson)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_email", email);
        parametros.Add("@p_contrasena", contrasena);
        parametros.Add("@p_roles_json", rolesJson);
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        try
        {
            await conexion.ExecuteAsync(
                "crear_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)!;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new ConflictoExcepcion($"El usuario '{email}' ya existe en el sistema.", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ConflictoExcepcion("Uno de los roles especificados no existe en la base de datos.", ex);
        }
    }

    public async Task<UsuarioConRoles> ActualizarAsync(string email, string? contrasena, string rolesJson)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_email", email);
        parametros.Add("@p_contrasena", string.IsNullOrEmpty(contrasena) ? (object)DBNull.Value : contrasena);
        parametros.Add("@p_roles", rolesJson);
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        try
        {
            await conexion.ExecuteAsync(
                "actualizar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)!;
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ConflictoExcepcion("Uno de los roles especificados no existe.", ex);
        }
    }

    public async Task<string> EliminarAsync(string email)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);

        var parametros = new DynamicParameters();
        parametros.Add("@p_email", email);
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        try
        {
            await conexion.ExecuteAsync(
                "eliminar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return parametros.Get<string>("@p_resultado");
        }
        catch (SqlException ex) when (ex.Number == 50006 || ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
    }
}
