// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioRolUsuario en SQL Server con Dapper.
/// </summary>
public class RepositorioRolUsuarioSqlServer : IRepositorioRolUsuario
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioRolUsuarioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<RolUsuario>> ObtenerTodosAsync()
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            ORDER BY ru.fkemail, ru.fkidrol";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<RolUsuario>(sql);
        return resultado.AsList();
    }

    public async Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            WHERE ru.fkemail = @email
            ORDER BY r.nombre";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<RolUsuario>(sql, new { email });
        return resultado.AsList();
    }

    public async Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON r.id = ru.fkidrol
            WHERE ru.fkidrol = @idRol
            ORDER BY ru.fkemail";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<RolUsuario>(sql, new { idRol });
        return resultado.AsList();
    }

    // Ejecuta la operación 'ExisteAsync' dentro de la capa correspondiente
    public async Task<bool> ExisteAsync(string email, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT COUNT(1) 
            FROM rol_usuario 
            WHERE fkemail = @email AND fkidrol = @idRol";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        var cantidad = await conexion.ExecuteScalarAsync<int>(sql, new { email, idRol });
        return cantidad > 0;
    }

    // Ejecuta la operación 'AgregarAsync' dentro de la capa correspondiente
    public async Task<int> AgregarAsync(string email, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO rol_usuario (fkemail, fkidrol) 
            VALUES (@email, @idRol)";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { email, idRol });
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(string email, int idRol)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            DELETE FROM rol_usuario 
            WHERE fkemail = @email AND fkidrol = @idRol";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { email, idRol });
    }
}
