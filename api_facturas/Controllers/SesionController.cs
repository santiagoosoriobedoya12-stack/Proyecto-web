using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador de Sesión e Identificación (Versión 3).
/// Es el único controlador abierto de la API decorado con [AllowAnonymous].
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SesionController : ControllerBase
{
    private readonly IServicioSesion _servicioSesion;

    public SesionController(IServicioSesion servicioSesion)
    {
        _servicioSesion = servicioSesion;
    }

    /// <summary>
    /// Inicia sesión recibiendo las credenciales estrictamente en el cuerpo JSON (POST).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> IniciarSesion([FromBody] SesionCrear peticion)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new
            {
                estado = 422,
                mensaje = "Datos de petición incompletos o inválidos.",
                errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        var sesion = await _servicioSesion.IniciarSesionAsync(peticion);

        if (sesion == null)
        {
            // Criterio 2 / 6_contracts.md A:
            // "El mismo error para los dos casos: correo inexistente o contraseña equivocada.
            // Decir 'ese correo no existe' le confirmaría a un desconocido cuáles sí existen."
            return StatusCode(401, new
            {
                estado = 401,
                mensaje = "El correo o la contrasena no son correctos."
            });
        }

        return Ok(sesion);
    }
}

