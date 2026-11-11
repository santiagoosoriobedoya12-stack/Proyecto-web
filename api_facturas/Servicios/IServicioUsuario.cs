// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato de negocio para Usuario.
/// </summary>
public interface IServicioUsuario
{
    Task<List<Usuario>> ListarAsync(int limite);
    Task<Usuario> ObtenerPorEmailAsync(string email);
    Task CrearAsync(UsuarioCrear peticion);
    Task<int> ReemplazarAsync(string email, UsuarioReemplazo peticion);
    Task<int> ActualizarParcialAsync(string email, UsuarioActualizar peticion);
    Task<int> EliminarAsync(string email);
}

