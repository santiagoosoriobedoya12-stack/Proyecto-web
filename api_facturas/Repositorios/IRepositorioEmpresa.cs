// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para la entidad Empresa.
/// </summary>
public interface IRepositorioEmpresa
{
    Task<List<Empresa>> ObtenerTodosAsync(int limite);
    Task<Empresa?> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(Empresa empresa);
    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);
    Task<int> EliminarAsync(string codigo);
}

