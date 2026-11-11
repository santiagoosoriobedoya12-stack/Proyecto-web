// Enumeraciones ADO.NET (DbType, ParameterDirection) para parámetros OUTPUT en Stored Procedures
using System.Data;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación SQL Server del acceso y permisos para la Versión 3.
/// Llama directamente al procedimiento almacenado verificar_acceso_ruta compilado en SQL Server.
/// </summary>
public class RepositorioAccesoSqlServer : IRepositorioAcceso
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioAccesoSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer") 
            ?? throw new InvalidOperationException("La cadena de conexión 'SqlServer' no está configurada.");
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    private SqlConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    /// <inheritdoc />
    public async Task<bool> VerificarAccesoAsync(string email, int idRuta)
    {
        using var conexion = CrearConexion();
        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_email", email, DbType.String);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_fkidruta", idRuta, DbType.Int32);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync("verificar_acceso_ruta", parametros, commandType: CommandType.StoredProcedure);

        var resultadoJson = parametros.Get<string>("@p_resultado");
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(resultadoJson))
        {
            return false;
        }

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = "SELECT id FROM ruta WHERE ruta = @nombreRuta;";
        // Busca un único registro por clave con Dapper; devuelve null si no existe en BD
        return await conexion.QueryFirstOrDefaultAsync<int?>(sql, new { nombreRuta });
    }

    /// <inheritdoc />
    public async Task<List<string>> ObtenerRolesPorEmailAsync(string email)
    {
        using var conexion = CrearConexion();
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
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

