using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'usuario' (/api/usuario).
/// Clave primaria: email (string).
/// Nunca expone las contraseñas en las respuestas.
/// </summary>
[ApiController]
[Route("api/usuario")]
public class UsuarioController : ControllerBase
{
    private readonly IServicioUsuario _servicio;

    public UsuarioController(IServicioUsuario servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var usuarios = await _servicio.ListarAsync(limite);
            if (usuarios.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "usuario",
                limite,
                total = usuarios.Count,
                datos = usuarios
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> ObtenerPorEmail(string email)
    {
        try
        {
            var usuario = await _servicio.ObtenerPorEmailAsync(email);
            return Ok(usuario);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Usuario no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new { estado = 200, mensaje = "Usuario creado exitosamente." });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    [HttpPut("{email}")]
    public async Task<IActionResult> Reemplazar(string email, [FromBody] UsuarioReemplazo peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(email, peticion);
            return Ok(new { estado = 200, mensaje = "Usuario actualizado exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Usuario no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: el usuario tiene roles o registros asociados.", detalle = ex.Message });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    [HttpPatch("{email}")]
    public async Task<IActionResult> ActualizarParcial(string email, [FromBody] UsuarioActualizar peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(email, peticion);
            return Ok(new { estado = 200, mensaje = "Usuario actualizado exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Usuario no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: el usuario tiene roles o registros asociados.", detalle = ex.Message });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    [HttpDelete("{email}")]
    public async Task<IActionResult> Eliminar(string email)
    {
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(email);
            return Ok(new { estado = 200, mensaje = "Usuario eliminado exitosamente.", filasEliminadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Usuario no encontrado.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: el usuario tiene roles o registros asociados.", detalle = ex.Message });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}

