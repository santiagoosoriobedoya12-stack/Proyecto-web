// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
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

