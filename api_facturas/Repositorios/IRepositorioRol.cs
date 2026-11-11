// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para Rol.
/// </summary>
public interface IRepositorioRol
{
    Task<List<Rol>> ObtenerTodosAsync(int limite);
    Task<Rol?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Rol rol);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}

