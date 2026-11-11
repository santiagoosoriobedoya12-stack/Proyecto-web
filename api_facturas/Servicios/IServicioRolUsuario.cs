// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

// Contrato 'IServicioRolUsuario': abstrae las operaciones disponibles para permitir inyección de dependencias y pruebas unitarias
public interface IServicioRolUsuario
{
    Task<List<RolUsuario>> ObtenerTodosAsync();
    Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email);
    Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol);
    Task AgregarAsync(RolUsuarioCrear peticion);
    Task EliminarAsync(string email, int idRol);
}
