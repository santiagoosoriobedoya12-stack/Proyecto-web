using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'empresa' (/api/empresa).
/// </summary>
[ApiController]
[Route("api/empresa")]
public class EmpresaController : ControllerBase
{
    private readonly IServicioEmpresa _servicio;

    public EmpresaController(IServicioEmpresa servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var empresas = await _servicio.ListarAsync(limite);
            if (empresas.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "empresa",
                limite,
                total = empresas.Count,
                datos = empresas
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
            var empresa = await _servicio.ObtenerPorCodigoAsync(codigo);
            return Ok(empresa);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
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
    public async Task<IActionResult> Crear([FromBody] EmpresaCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new { estado = 200, mensaje = "Empresa creada exitosamente." });
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
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] EmpresaReemplazo peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(codigo, peticion);
            return Ok(new { estado = 200, mensaje = "Empresa reemplazada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
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
    public async Task<IActionResult> ActualizarParcial(string codigo, [FromBody] EmpresaActualizar peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(codigo, peticion);
            return Ok(new { estado = 200, mensaje = "Empresa actualizada exitosamente.", filasAfectadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
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
            return Ok(new { estado = 200, mensaje = "Empresa eliminada exitosamente.", filasEliminadas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
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

