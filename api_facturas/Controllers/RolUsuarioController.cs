using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para la tabla puente 'rol_usuario' (/api/rol-usuario).
/// 
/// Características (RF4):
/// 1. Consulta por ambos extremos: /usuario/{email} y /rol/{idrol}.
/// 2. Inserción con control de duplicados (409 Conflict).
/// 3. Eliminación requiriendo ambas claves.
/// 4. Sin verbos PUT ni PATCH.
/// </summary>
[ApiController]
[Route("api/rol-usuario")]
[Authorize]
[ExigePermiso("interfaz.roles")]
public class RolUsuarioController : ControllerBase
{
    private readonly IServicioRolUsuario _servicio;

    public RolUsuarioController(IServicioRolUsuario servicio)
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
            tabla = "rol_usuario",
            total = lista.Count,
            datos = lista
        });
    }

    [HttpGet("usuario/{email}")]
    public async Task<IActionResult> ObtenerPorUsuario(string email)
    {
        var lista = await _servicio.ObtenerPorUsuarioAsync(email);
        return Ok(lista);
    }

    [HttpGet("rol/{idrol:int}")]
    public async Task<IActionResult> ObtenerPorRol(int idrol)
    {
        var lista = await _servicio.ObtenerPorRolAsync(idrol);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] RolUsuarioCrear peticion)
    {
        try
        {
            await _servicio.AgregarAsync(peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Rol asignado al usuario exitosamente.",
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
                mensaje = "Conflicto de clave foránea: el usuario o el rol no existe.",
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

    [HttpDelete("{email}/{idrol:int}")]
    public async Task<IActionResult> EliminarPorRuta(string email, int idrol)
    {
        return await EjecutarEliminacion(email, idrol);
    }

    [HttpDelete]
    public async Task<IActionResult> EliminarPorQuery([FromQuery] string fkemail, [FromQuery] int fkidrol)
    {
        return await EjecutarEliminacion(fkemail, fkidrol);
    }

    private async Task<IActionResult> EjecutarEliminacion(string email, int idrol)
    {
        try
        {
            await _servicio.EliminarAsync(email, idrol);
            return Ok(new
            {
                estado = 200,
                mensaje = "Relación rol-usuario eliminada exitosamente."
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
