using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación SQL Server del acceso y permisos para la Versión 3.
/// Llama directamente al procedimiento almacenado verificar_acceso_ruta compilado en SQL Server.
/// </summary>
public class RepositorioAccesoSqlServer : IRepositorioAcceso
{
    private readonly string _cadenaConexion;

    public RepositorioAccesoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer") 
            ?? throw new InvalidOperationException("La cadena de conexión 'SqlServer' no está configurada.");
    }

    private SqlConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    /// <inheritdoc />
    public async Task<bool> VerificarAccesoAsync(string email, int idRuta)
    {
        using var conexion = CrearConexion();
        var parametros = new DynamicParameters();
        parametros.Add("@p_email", email, DbType.String);
        parametros.Add("@p_fkidruta", idRuta, DbType.Int32);
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        await conexion.ExecuteAsync("verificar_acceso_ruta", parametros, commandType: CommandType.StoredProcedure);

        var resultadoJson = parametros.Get<string>("@p_resultado");
        if (string.IsNullOrWhiteSpace(resultadoJson))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(resultadoJson);
            if (doc.RootElement.TryGetProperty("tiene_acceso", out var tieneAccesoProp))
            {
                return tieneAccesoProp.GetBoolean();
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<int?> ObtenerIdRutaPorNombreAsync(string nombreRuta)
    {
        using var conexion = CrearConexion();
        const string sql = "SELECT id FROM ruta WHERE ruta = @nombreRuta;";
        return await conexion.QueryFirstOrDefaultAsync<int?>(sql, new { nombreRuta });
    }

    /// <inheritdoc />
    public async Task<List<string>> ObtenerRolesPorEmailAsync(string email)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            SELECT r.nombre 
            FROM rol r 
            INNER JOIN rol_usuario ru ON r.id = ru.fkidrol 
            WHERE ru.fkemail = @email;";
        var roles = await conexion.QueryAsync<string>(sql, new { email });
        return roles.ToList();
    }

    /// <inheritdoc />
    public async Task<List<string>> ObtenerRutasPermitidasPorEmailAsync(string email)
    {
        using var conexion = CrearConexion();
        // Nota didáctica: Esta consulta se utiliza exclusivamente para armar el menú visual (Criterio 8).
        // La decisión real de acceso en cada petición HTTP la toma el procedimiento verificar_acceso_ruta (Criterio 6).
        const string sql = @"
            SELECT DISTINCT rt.ruta
            FROM usuario u
            INNER JOIN rol_usuario ru ON u.email = ru.fkemail
            INNER JOIN rutarol rr ON ru.fkidrol = rr.fkidrol
            INNER JOIN ruta rt ON rr.fkidruta = rt.id
            WHERE u.email = @email;";
        var rutas = await conexion.QueryAsync<string>(sql, new { email });
        return rutas.ToList();
    }
}

