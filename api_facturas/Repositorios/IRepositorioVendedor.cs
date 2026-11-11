// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para Vendedor.
/// </summary>
public interface IRepositorioVendedor
{
    Task<List<Vendedor>> ObtenerTodosAsync(int limite);
    Task<Vendedor?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Vendedor vendedor);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}

