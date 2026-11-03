using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de autenticación y sesiones para la Versión 3.
/// </summary>
public interface IServicioSesion
{
    /// <summary>
    /// Valida las credenciales enviadas contra la base de datos y emite el token JWT si coinciden.
    /// Retorna null si el correo no existe o la contraseña no coincide (sin detallar cuál falló).
    /// </summary>
    Task<Sesion?> IniciarSesionAsync(SesionCrear peticion);
}

