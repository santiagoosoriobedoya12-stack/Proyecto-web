// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para Usuario.
/// </summary>
public interface IRepositorioUsuario
{
    Task<List<Usuario>> ObtenerTodosAsync(int limite);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task CrearAsync(string email, string contrasena);
    Task<int> ActualizarAsync(string email, Dictionary<string, object> datos);
    Task<int> EliminarAsync(string email);
}

