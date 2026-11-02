using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Repositorio de Persona en SQL Server con Dapper.
/// </summary>
public class RepositorioPersonaSqlServer : IRepositorioPersona
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre", "email", "telefono"
    };

    public RepositorioPersonaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Persona>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) codigo, nombre, email, telefono 
            FROM persona 
            ORDER BY codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Persona>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Persona?> ObtenerPorCodigoAsync(string codigo)
    {
        const string sql = @"
            SELECT codigo, nombre, email, telefono 
            FROM persona 
            WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Persona>(sql, new { codigo });
    }

    public async Task CrearAsync(Persona persona)
    {
        const string sql = @"
            INSERT INTO persona (codigo, nombre, email, telefono) 
            VALUES (@Codigo, @Nombre, @Email, @Telefono)";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, persona);
    }

    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (datos == null || datos.Count == 0)
            return 0;

        var clausulasSet = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("codigo_clave", codigo);

        foreach (var par in datos)
        {
            var columna = par.Key.ToLower();
            if (ColumnasPermitidas.Contains(columna))
            {
                clausulasSet.Add($"{columna} = @{columna}");
                parametros.Add(columna, par.Value);
            }
        }

        if (clausulasSet.Count == 0)
            return 0;

        var sql = $"UPDATE persona SET {string.Join(", ", clausulasSet)} WHERE codigo = @codigo_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        const string sql = "DELETE FROM persona WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}
