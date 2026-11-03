using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'cliente' (/api/cliente) — Versión 2 / Versión 3.
/// 
/// Novedades de la Versión 2:
/// 1. Claves Foráneas: 'fkcodpersona' (obligatoria) y 'fkcodempresa' (opcional).
/// 2. Código HTTP 409 Conflict: Cuando se envía una clave foránea que no existe en la base de datos
///    (error 547 de SQL Server) o cuando una eliminación viola la integridad referencial.
/// 3. Mapeo del molde estándar de 5 verbos: GET (listar/consultar), POST, PUT, PATCH, DELETE.
/// </summary>
[ApiController]
[Route("api/cliente")]
[Authorize]
[ExigePermiso("interfaz.clientes")]
public class ClienteController : ControllerBase
{
    private readonly IServicioCliente _servicio;

    public ClienteController(IServicioCliente servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF1: Listar clientes con soporte de ?limite=N.
    /// Si la tabla no tiene filas, responde 204 No Content.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var clientes = await _servicio.ListarAsync(limite);
            if (clientes.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "cliente",
                limite,
                total = clientes.Count,
                datos = clientes
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

    /// <summary>
    /// RF1: Consultar un cliente específico por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var cliente = await _servicio.ObtenerPorIdAsync(id);
            return Ok(cliente);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
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

    /// <summary>
    /// RF1: Crear un nuevo cliente (POST).
    /// El ID es autogenerado por IDENTITY. Si la FK no existe en persona o empresa, responde 409 Conflict.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ClienteCrear peticion)
    {
        try
        {
            var nuevoId = await _servicio.CrearAsync(peticion);
            var clienteCreado = await _servicio.ObtenerPorIdAsync(nuevoId);

            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente creado exitosamente.",
                id = nuevoId,
                datos = clienteCreado
            });
        }
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            // Error 547: FOREIGN KEY constraint violation (la persona o empresa referenciada no existe)
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error en el motor de base de datos.",
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

    /// <summary>
    /// RF1: Reemplazar completamente un cliente (PUT).
    /// Si la clave foránea no existe, responde 409 Conflict.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] ClienteReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.ReemplazarAsync(id, peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente reemplazado exitosamente.",
                filasAfectadas = filas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
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

    /// <summary>
    /// RF1: Actualizar parcialmente un cliente (PATCH).
    /// </summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] ClienteActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarParcialAsync(id, peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente actualizado exitosamente.",
                filasAfectadas = filas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
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

    /// <summary>
    /// RF1: Eliminar un cliente por su ID (DELETE).
    /// Si el cliente ya tiene facturas, el motor impedirá el borrado (409 Conflict).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(id);
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente eliminado exitosamente.",
                filasAfectadas = filas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: no se puede eliminar el cliente porque tiene registros relacionados.",
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

