// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para Ruta.
/// </summary>
public interface IRepositorioRuta
{
    Task<List<Ruta>> ObtenerTodosAsync(int limite);
    Task<Ruta?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Ruta ruta);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}

