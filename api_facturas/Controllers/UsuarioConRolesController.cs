using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso compuesto 'api/usuario-con-roles' (RF5).
/// Protegido con [Authorize] y [ExigePermiso("interfaz.usuarios")].
/// </summary>
[ApiController]
[Route("api/usuario-con-roles")]
[Authorize]
[ExigePermiso("interfaz.usuarios")]
public class UsuarioConRolesController : ControllerBase
{
    private readonly IServicioUsuarioConRoles _servicio;

    public UsuarioConRolesController(IServicioUsuarioConRoles servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _servicio.ListarAsync();
        if (lista.Count == 0)
            return NoContent();

        return Ok(new
        {
            tabla = "usuario_con_roles",
            total = lista.Count,
            datos = lista
        });
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> Consultar(string email)
    {
        try
        {
            var usuario = await _servicio.ConsultarAsync(email);
            return Ok(usuario);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
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
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioConRolesCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(creado);
        }
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: usuario duplicado o rol inexistente.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: el usuario ya existe en el sistema.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: uno de los roles no existe.",
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

    [HttpPut("{email}")]
    public async Task<IActionResult> Actualizar(string email, [FromBody] UsuarioConRolesActualizar peticion)
    {
        try
        {
            var actualizado = await _servicio.ActualizarAsync(email, peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Usuario y roles actualizados exitosamente.",
                datos = actualizado
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
                detalle = ex.Message
            });
        }
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto con los roles asignados.",
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

    [HttpDelete("{email}")]
    public async Task<IActionResult> Eliminar(string email)
    {
        try
        {
            await _servicio.EliminarAsync(email);
            return Ok(new
            {
                estado = 200,
                mensaje = "Usuario y sus asignaciones de roles eliminados exitosamente."
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
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
