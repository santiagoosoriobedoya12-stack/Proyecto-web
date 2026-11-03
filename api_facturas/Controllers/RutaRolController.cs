using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para la tabla puente 'rutarol' (/api/rutarol).
/// </summary>
[ApiController]
[Route("api/rutarol")]
[Authorize]
[ExigePermiso("interfaz.permisos")]
public class RutaRolController : ControllerBase
{
    private readonly IServicioRutaRol _servicio;

    public RutaRolController(IServicioRutaRol servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var lista = await _servicio.ObtenerTodosAsync();
        if (lista.Count == 0)
            return NoContent();

        return Ok(new
        {
            tabla = "rutarol",
            total = lista.Count,
            datos = lista
        });
    }

    [HttpGet("ruta/{idruta:int}")]
    public async Task<IActionResult> ObtenerPorRuta(int idruta)
    {
        var lista = await _servicio.ObtenerPorRutaAsync(idruta);
        return Ok(lista);
    }

    [HttpGet("rol/{idrol:int}")]
    public async Task<IActionResult> ObtenerPorRol(int idrol)
    {
        var lista = await _servicio.ObtenerPorRolAsync(idrol);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] RutaRolCrear peticion)
    {
        try
        {
            await _servicio.AgregarAsync(peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Permiso de ruta asignado al rol exitosamente.",
                datos = peticion
            });
        }
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la relación ya existe.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la relación ya existe en la base de datos.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la ruta o el rol no existe.",
                detalle = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error interno del servidor.",
                detalle = ex.Message
            });
        }
    }

    [HttpDelete("{idruta:int}/{idrol:int}")]
    public async Task<IActionResult> EliminarPorRuta(int idruta, int idrol)
    {
        return await EjecutarEliminacion(idruta, idrol);
    }

    [HttpDelete]
    public async Task<IActionResult> EliminarPorQuery([FromQuery] int fkidruta, [FromQuery] int fkidrol)
    {
        return await EjecutarEliminacion(fkidruta, fkidrol);
    }

    private async Task<IActionResult> EjecutarEliminacion(int idruta, int idrol)
    {
        try
        {
            await _servicio.EliminarAsync(idruta, idrol);
            return Ok(new
            {
                estado = 200,
                mensaje = "Permiso de ruta eliminado del rol exitosamente."
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Relación no encontrada.",
                detalle = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error interno del servidor.",
                detalle = ex.Message
            });
        }
    }
}
