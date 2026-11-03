using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

public interface IServicioUsuarioConRoles
{
    Task<List<UsuarioConRoles>> ListarAsync();
    Task<UsuarioConRoles> ConsultarAsync(string email);
    Task<UsuarioConRoles> CrearAsync(UsuarioConRolesCrear peticion);
    Task<UsuarioConRoles> ActualizarAsync(string email, UsuarioConRolesActualizar peticion);
    Task EliminarAsync(string email);
}
