using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'persona' (/api/persona).
/// </summary>
[ApiController]
[Route("api/persona")]
[Authorize]
[ExigePermiso("interfaz.personas")]
public class PersonaController : ControllerBase
{
    private readonly IServicioPersona _servicio;

    public PersonaController(IServicioPersona servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var personas = await _servicio.ListarAsync(limite);
            if (personas.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "persona",
                limite,
                total = personas.Count,
                datos = personas
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

    [HttpGet("{codigo}")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        try
        {
            var persona = await _servicio.ObtenerPorCodigoAsync(codigo);
            return Ok(persona);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Persona no encontrada.", detalle = ex.Message });
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
    public async Task<IActionResult> Crear([FromBody] PersonaCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new { estado = 200, mensaje = "Persona creada exitosamente." });
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

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] PersonaReemplazo peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(codigo, peticion);
            return Ok(new { estado = 200, mensaje = "Persona reemplazada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Persona no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la persona está asociada como cliente o vendedor.", detalle = ex.Message });
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

    [HttpPatch("{codigo}")]
    public async Task<IActionResult> ActualizarParcial(string codigo, [FromBody] PersonaActualizar peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(codigo, peticion);
            return Ok(new { estado = 200, mensaje = "Persona actualizada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Persona no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la persona está asociada como cliente o vendedor.", detalle = ex.Message });
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

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(codigo);
            return Ok(new { estado = 200, mensaje = "Persona eliminada exitosamente.", filasEliminadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Persona no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la persona está asociada como cliente o vendedor.", detalle = ex.Message });
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

