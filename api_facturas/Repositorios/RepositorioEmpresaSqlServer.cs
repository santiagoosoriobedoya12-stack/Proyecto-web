using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Implementación del repositorio de Empresa contra SQL Server con Dapper.
/// SQL a la vista y siempre parametrizado.
/// </summary>
public class RepositorioEmpresaSqlServer : IRepositorioEmpresa
{
    private readonly string _cadenaConexion;

    private static readonly HashSet<string> ColumnasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "nombre"
    };

    public RepositorioEmpresaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion ?? throw new ArgumentNullException(nameof(cadenaConexion));
    }

    public async Task<List<Empresa>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"
            SELECT TOP (@limite) codigo, nombre 
            FROM empresa 
            ORDER BY codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        var resultado = await conexion.QueryAsync<Empresa>(sql, new { limite });
        return resultado.AsList();
    }

    public async Task<Empresa?> ObtenerPorCodigoAsync(string codigo)
    {
        const string sql = @"
            SELECT codigo, nombre 
            FROM empresa 
            WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Empresa>(sql, new { codigo });
    }

    public async Task CrearAsync(Empresa empresa)
    {
        const string sql = @"
            INSERT INTO empresa (codigo, nombre) 
            VALUES (@Codigo, @Nombre)";

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, empresa);
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

        var sql = $"UPDATE empresa SET {string.Join(", ", clausulasSet)} WHERE codigo = @codigo_clave";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        const string sql = "DELETE FROM empresa WHERE codigo = @codigo";

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}
