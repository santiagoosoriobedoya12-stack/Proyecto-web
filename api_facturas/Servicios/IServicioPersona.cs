using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Persona.
/// </summary>
public interface IServicioPersona
{
    Task<List<Persona>> ListarAsync(int limite);
    Task<Persona> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(PersonaCrear peticion);
    Task<int> ReemplazarAsync(string codigo, PersonaReemplazo peticion);
    Task<int> ActualizarParcialAsync(string codigo, PersonaActualizar peticion);
    Task<int> EliminarAsync(string codigo);
}

