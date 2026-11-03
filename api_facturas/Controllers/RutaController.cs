using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'ruta' (/api/ruta).
/// Clave primaria autoincremental: id (INT).
/// </summary>
[ApiController]
[Route("api/ruta")]
public class RutaController : ControllerBase
{
    private readonly IServicioRuta _servicio;

    public RutaController(IServicioRuta servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var rutas = await _servicio.ListarAsync(limite);
            if (rutas.Count == 0)
            {
                return NoContent();
            }

            var datos = rutas.Select(r => new
            {
                id = r.Id,
                ruta = r.NombreRuta,
                descripcion = r.Descripcion
            }).ToList();

            return Ok(new
            {
                tabla = "ruta",
                limite,
                total = datos.Count,
                datos
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var r = await _servicio.ObtenerPorIdAsync(id);
            return Ok(new
            {
                id = r.Id,
                ruta = r.NombreRuta,
                descripcion = r.Descripcion
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Ruta no encontrada.", detalle = ex.Message });
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
    public async Task<IActionResult> Crear([FromBody] RutaCrear peticion)
    {
        try
        {
            var nuevoId = await _servicio.CrearAsync(peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Ruta creada exitosamente.",
                id = nuevoId
            });
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] RutaReemplazo peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(id, peticion);
            return Ok(new { estado = 200, mensaje = "Ruta reemplazada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Ruta no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la ruta está referenciada en permisos de roles.", detalle = ex.Message });
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

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] RutaActualizar peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(id, peticion);
            return Ok(new { estado = 200, mensaje = "Ruta actualizada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Ruta no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la ruta está referenciada en permisos de roles.", detalle = ex.Message });
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(id);
            return Ok(new { estado = 200, mensaje = "Ruta eliminada exitosamente.", filasEliminadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Ruta no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la ruta está referenciada en permisos de roles.", detalle = ex.Message });
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

