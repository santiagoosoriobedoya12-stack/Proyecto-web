using ApiFacturas.Modelos;
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

