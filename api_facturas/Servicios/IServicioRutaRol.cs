// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

// Contrato 'IServicioRutaRol': abstrae las operaciones disponibles para permitir inyección de dependencias y pruebas unitarias
public interface IServicioRutaRol
{
    Task<List<RutaRol>> ObtenerTodosAsync();
    Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta);
    Task<List<RutaRol>> ObtenerPorRolAsync(int idRol);
    Task AgregarAsync(RutaRolCrear peticion);
    Task EliminarAsync(int idRuta, int idRol);
}
