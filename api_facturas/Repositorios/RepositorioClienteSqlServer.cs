// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// Micro-ORM Dapper: mapea consultas SQL parametrizadas directamente a objetos C# sin sobrecarga de ORM pesado
using Dapper;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación de IRepositorioCliente sobre Microsoft SQL Server utilizando Dapper.
/// 
/// Características pedagógicas:
/// 1. Emplea consultas SQL parametrizadas para evitar ataques de inyección SQL.
/// 2. Utiliza SCOPE_IDENTITY() para capturar el 'id' autoincremental tras el INSERT.
/// 3. Lista blanca (ColumnasPermitidas) para construir la cláusula UPDATE de forma segura.
/// 4. Las violaciones de restricciones de integridad foránea (FK) disparan SqlException (código 547).
/// </summary>
public class RepositorioClienteSqlServer : IRepositorioCliente
{
    // Cadena de conexión hacia SQL Server leída desde appsettings.json o variable de entorno en Docker
    private readonly string _cadenaConexion;

    // Lista blanca de columnas editables para evitar inyecciones en sentencias dinámicas
    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "credito",
        "fkcodpersona",
        "fkcodempresa"
    };

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RepositorioClienteSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    /// <summary>
    /// Consulta los clientes ordenados por 'id', aplicando el límite especificado con TOP.
    /// </summary>
    public async Task<List<Cliente>> ObtenerTodosAsync(int limite)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT TOP (@limite) id, credito, fkcodpersona, fkcodempresa 
            FROM cliente 
            ORDER BY id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta el SELECT con Dapper de forma asíncrona y recolecta los registros
        var resultado = await conexion.QueryAsync<Cliente>(sql, new { limite });
        return resultado.AsList();
    }

    /// <summary>
    /// Busca un cliente por su clave primaria 'id'.
    /// </summary>
    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            SELECT id, credito, fkcodpersona, fkcodempresa 
            FROM cliente 
            WHERE id = @id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Busca un único registro por clave con Dapper; devuelve null si no existe en BD
        return await conexion.QueryFirstOrDefaultAsync<Cliente>(sql, new { id });
    }

    /// <summary>
    /// Inserta el cliente y retorna el nuevo ID asignado por SQL Server con SCOPE_IDENTITY().
    /// Si fkcodpersona o fkcodempresa no existen en la base de datos, el motor lanza SqlException (547).
    /// </summary>
    public async Task<int> CrearAsync(Cliente cliente)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = @"
            INSERT INTO cliente (credito, fkcodpersona, fkcodempresa) 
            VALUES (@Credito, @Fkcodpersona, @Fkcodempresa);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>(sql, cliente);
    }

    /// <summary>
    /// Actualiza selectivamente las columnas provistas en el diccionario.
    /// Maneja valores nulos en columnas como 'fkcodempresa'.
    /// </summary>
    public async Task<int> ActualizarAsync(int id, Dictionary<string, object?> datos)
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
        var sql = $"UPDATE cliente SET {string.Join(", ", clausulasSet)} WHERE id = @id_clave";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, parametros);
    }

    /// <summary>
    /// Elimina el registro del cliente por su clave primaria.
    /// Si el cliente ya tiene facturas asociadas, el motor lanzará una excepción por integridad referencial (547).
    /// </summary>
    public async Task<int> EliminarAsync(int id)
    {
        // Consulta SQL explícita con parámetros nombrados (@param) para garantizar seguridad y rendimiento
        const string sql = "DELETE FROM cliente WHERE id = @id";

        // Abre conexión asíncrona a SQL Server; 'await using' garantiza cerrarla y liberarla automáticamente
        await using var conexion = new SqlConnection(_cadenaConexion);
        // Ejecuta la modificación en BD y retorna el conteo de filas afectadas
        return await conexion.ExecuteAsync(sql, new { id });
    }
}

