// Enumeraciones ADO.NET (DbType, ParameterDirection) para parámetros OUTPUT en Stored Procedures
using System.Data;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioUsuarioConRoles con Dapper en SQL Server.
/// Ejecuta los procedimientos almacenados oficiales pasando los parámetros ya preparados.
/// </summary>
public class RepositorioUsuarioConRolesSqlServer : IRepositorioUsuarioConRoles
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioUsuarioConRolesSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<UsuarioConRoles>> ListarAsync()
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync(
            "listar_usuarios_con_roles",
            parametros,
            commandType: CommandType.StoredProcedure
        );

        var json = parametros.Get<string>("@p_resultado");
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<UsuarioConRoles>();

        return JsonSerializer.Deserialize<List<UsuarioConRoles>>(json, JsonOpts) ?? new List<UsuarioConRoles>();
    }

    // Ejecuta la operación 'ConsultarAsync' dentro de la capa correspondiente
    public async Task<UsuarioConRoles> ConsultarAsync(string email)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_email", email);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "consultar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            // Valida que el identificador o cadena requerida no llegue nula ni vacía
            if (string.IsNullOrWhiteSpace(json))
                throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");

            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)
                ?? throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 50008 || ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<UsuarioConRoles> CrearAsync(string email, string contrasena, string rolesJson)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_email", email);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_contrasena", contrasena);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_roles_json", rolesJson);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "crear_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)!;
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion($"El usuario '{email}' ya existe en el sistema.", ex);
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion("Uno de los roles especificados no existe en la base de datos.", ex);
        }
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<UsuarioConRoles> ActualizarAsync(string email, string? contrasena, string rolesJson)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_email", email);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_contrasena", string.IsNullOrEmpty(contrasena) ? (object)DBNull.Value : contrasena);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_roles", rolesJson);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "actualizar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var json = parametros.Get<string>("@p_resultado");
            return JsonSerializer.Deserialize<UsuarioConRoles>(json, JsonOpts)!;
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion("Uno de los roles especificados no existe.", ex);
        }
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<string> EliminarAsync(string email)
    {
        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);

        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_email", email);
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("@p_resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);

        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
            await conexion.ExecuteAsync(
                "eliminar_usuario_con_roles",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return parametros.Get<string>("@p_resultado");
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 50006 || ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase))
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"Usuario '{email}' no existe.");
        }
    }
}
