// Enumeraciones ADO.NET (DbType, ParameterDirection) para parámetros OUTPUT en Stored Procedures
using System.Data;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

// Repositorio SQL Server 'RepositorioRutaRolSqlServer': ejecuta sentencias SQL o Stored Procedures mediante Dapper
public class RepositorioRutaRolSqlServer : IRepositorioRutaRol
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioRutaRolSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<RutaRol>> ObtenerTodosAsync()
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync(
            "listar_rutarol",
            parametros,
            commandType: CommandType.StoredProcedure
        );

        var json = parametros.Get<string>("@p_resultado");
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<RutaRol>();

        return JsonSerializer.Deserialize<List<RutaRol>>(json, JsonOpts) ?? new List<RutaRol>();
    }

    public async Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT rr.fkidruta, rt.ruta, rr.fkidrol, r.nombre AS rol
            FROM rutarol rr
            JOIN ruta rt ON rt.id = rr.fkidruta
            JOIN rol r ON r.id = rr.fkidrol
            WHERE rr.fkidruta = @idRuta
            ORDER BY r.nombre";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<RutaRol>(sql, new { idRuta });
        return resultado.AsList();
    }

    public async Task<List<RutaRol>> ObtenerPorRolAsync(int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT rr.fkidruta, rt.ruta, rr.fkidrol, r.nombre AS rol
            FROM rutarol rr
            JOIN ruta rt ON rt.id = rr.fkidruta
            JOIN rol r ON r.id = rr.fkidrol
            WHERE rr.fkidrol = @idRol
            ORDER BY rt.ruta";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<RutaRol>(sql, new { idRol });
        return resultado.AsList();
    }

    // Ejecuta la operación 'ExisteAsync' dentro de la capa correspondiente
    public async Task<bool> ExisteAsync(int idRuta, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT COUNT(1) 
            FROM rutarol 
            WHERE fkidruta = @idRuta AND fkidrol = @idRol";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        var cantidad = await conexion.ExecuteScalarAsync<int>(sql, new { idRuta, idRol });
        return cantidad > 0;
    }

    // Ejecuta la operación 'AgregarAsync' dentro de la capa correspondiente
    public async Task<int> AgregarAsync(int idRuta, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO rutarol (fkidruta, fkidrol) 
            VALUES (@idRuta, @idRol)";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { idRuta, idRol });
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(int idRuta, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            DELETE FROM rutarol 
            WHERE fkidruta = @idRuta AND fkidrol = @idRol";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { idRuta, idRol });
    }
}
