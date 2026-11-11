// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación del repositorio de Empresa contra SQL Server con Dapper.
/// SQL a la vista y siempre parametrizado.
/// </summary>
public class RepositorioEmpresaSqlServer : IRepositorioEmpresa
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    // Lista blanca de columnas permitidas en actualizaciones PATCH (previene inyección SQL dinámica)
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre"
    };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioEmpresaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Empresa>> ObtenerTodosAsync(int limite)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT TOP (@limite) codigo, nombre 
            FROM empresa 
            ORDER BY codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<Empresa>(sql, new { limite });
        return resultado.AsList();
    }

    // Busca un registro específico por su código único
    public async Task<Empresa?> ObtenerPorCodigoAsync(string codigo)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT codigo, nombre 
            FROM empresa 
            WHERE codigo = @codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Busca un único registro por clave con Dapper; devuelve null si no existe en BD
        return await conexion.QueryFirstOrDefaultAsync<Empresa>(sql, new { codigo });
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task CrearAsync(Empresa empresa)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO empresa (codigo, nombre) 
            VALUES (@Codigo, @Nombre)";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta comando SQL (INSERT/UPDATE/DELETE o SP) sin retornar filas
        await conexion.ExecuteAsync(sql, empresa);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("codigo_clave", codigo);

        foreach (var par in datos)
        {
            var columna = par.Key.ToLower();
            if (ColumnasPermitidas.Contains(columna))
            {
                clausulasSet.Add($"{columna} = @{columna}");
                // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
                parametros.Add(columna, par.Value);
            }
        }

        if (clausulasSet.Count == 0)
            return 0;

        // Construye dinámicamente la sentencia SQL segura usando únicamente columnas de la lista blanca
        var sql = $"UPDATE empresa SET {string.Join(", ", clausulasSet)} WHERE codigo = @codigo_clave";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, parametros);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(string codigo)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = "DELETE FROM empresa WHERE codigo = @codigo";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}

