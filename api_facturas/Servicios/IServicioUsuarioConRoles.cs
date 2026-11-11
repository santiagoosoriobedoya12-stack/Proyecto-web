// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

// Contrato 'IServicioUsuarioConRoles': abstrae las operaciones disponibles para permitir inyección de dependencias y pruebas unitarias
public interface IServicioUsuarioConRoles
{
    Task<List<UsuarioConRoles>> ListarAsync();
    Task<UsuarioConRoles> ConsultarAsync(string email);
    Task<UsuarioConRoles> CrearAsync(UsuarioConRolesCrear peticion);
    Task<UsuarioConRoles> ActualizarAsync(string email, UsuarioConRolesActualizar peticion);
    Task EliminarAsync(string email);
}
