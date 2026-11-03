using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

public interface IServicioRolUsuario
{
    Task<List<RolUsuario>> ObtenerTodosAsync();
    Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email);
    Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol);
    Task AgregarAsync(RolUsuarioCrear peticion);
    Task EliminarAsync(string email, int idRol);
}
