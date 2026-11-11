// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioVendedor para SQL Server con Dapper.
/// </summary>
public class RepositorioVendedorSqlServer : IRepositorioVendedor
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    // Lista blanca de columnas permitidas en actualizaciones PATCH (previene inyección SQL dinámica)
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "carnet",
        "direccion",
        "fkcodpersona"
    };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioVendedorSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Vendedor>> ObtenerTodosAsync(int limite)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT TOP (@limite) id, carnet, direccion, fkcodpersona 
            FROM vendedor 
            ORDER BY id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<Vendedor>(sql, new { limite });
        return resultado.AsList();
    }

    // Busca un registro específico por su identificador numérico
    public async Task<Vendedor?> ObtenerPorIdAsync(int id)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT id, carnet, direccion, fkcodpersona 
            FROM vendedor 
            WHERE id = @id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Busca un único registro por clave con Dapper; devuelve null si no existe en BD
        return await conexion.QueryFirstOrDefaultAsync<Vendedor>(sql, new { id });
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<int> CrearAsync(Vendedor vendedor)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO vendedor (carnet, direccion, fkcodpersona) 
            VALUES (@Carnet, @Direccion, @Fkcodpersona);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, vendedor);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        // Contenedor de parámetros Dapper seguro para valores de entrada y salidas OUTPUT de Stored Procedures
        var parametros = new DynamicParameters();
        // Agrega un parámetro SQL tipado al comando Dapper sin concatenar cadenas
        parametros.Add("id_clave", id);

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
        var sql = $"UPDATE vendedor SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, parametros);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(int id)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = "DELETE FROM vendedor WHERE id = @id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

