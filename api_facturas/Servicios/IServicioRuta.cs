using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Ruta.
/// </summary>
public interface IServicioRuta
{
    Task<List<Ruta>> ListarAsync(int limite);
    Task<Ruta> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(RutaCrear peticion);
    Task<int> ReemplazarAsync(int id, RutaReemplazo peticion);
    Task<int> ActualizarParcialAsync(int id, RutaActualizar peticion);
    Task<int> EliminarAsync(int id);
}

