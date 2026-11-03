using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Rol.
/// </summary>
public interface IServicioRol
{
    Task<List<Rol>> ListarAsync(int limite);
    Task<Rol> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(RolCrear peticion);
    Task<int> ReemplazarAsync(int id, RolReemplazo peticion);
    Task<int> ActualizarParcialAsync(int id, RolActualizar peticion);
    Task<int> EliminarAsync(int id);
}

