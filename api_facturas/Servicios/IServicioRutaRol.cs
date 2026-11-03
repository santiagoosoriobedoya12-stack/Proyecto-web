using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

public interface IServicioRutaRol
{
    Task<List<RutaRol>> ObtenerTodosAsync();
    Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta);
    Task<List<RutaRol>> ObtenerPorRolAsync(int idRol);
    Task AgregarAsync(RutaRolCrear peticion);
    Task EliminarAsync(int idRuta, int idRol);
}
