using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para Persona.
/// </summary>
public interface IRepositorioPersona
{
    Task<List<Persona>> ObtenerTodosAsync(int limite);
    Task<Persona?> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(Persona persona);
    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);
    Task<int> EliminarAsync(string codigo);
}

