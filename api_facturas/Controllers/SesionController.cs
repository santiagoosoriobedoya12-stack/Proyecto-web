// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de lógica y reglas de negocio
using ApiFacturas.Servicios;
// Atributos [Authorize] y [AllowAnonymous] para proteger o abrir endpoints HTTP (Versión 3)
using Microsoft.AspNetCore.Authorization;
// Componentes base de ASP.NET Core MVC (ControllerBase, IActionResult, [HttpGet], [Route])
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador de Sesión e Identificación (Versión 3).
/// Es el único controlador abierto de la API decorado con [AllowAnonymous].
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/[controller]'
[Route("api/[controller]")]
// Controlador REST 'SesionController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class SesionController : ControllerBase
{
    private readonly IServicioSesion _servicioSesion;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public SesionController(IServicioSesion servicioSesion)
    {
        _servicioSesion = servicioSesion;
    }

    /// <summary>
    /// Inicia sesión recibiendo las credenciales estrictamente en el cuerpo JSON (POST).
    /// </summary>
    [HttpPost]
    // Permite invocar este endpoint sin token JWT (necesario para login o diagnóstico público)
    [AllowAnonymous]
    // Ejecuta la operación 'IniciarSesion' dentro de la capa correspondiente
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

        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(sesion);
    }
}

