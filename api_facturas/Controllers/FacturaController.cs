using System.Text.Json;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso Maestro-Detalle 'factura' (/api/factura).
/// 
/// CARACTERÍSTICAS DE LA VERSIÓN 2 (RF3):
/// 1. Cuatro operaciones exclusivas: Listar, Consultar una, Crear y Anular.
/// 2. Todas operadas 100% mediante Procedimientos Almacenados.
/// 3. Sin DELETE físico en la API: la regla del negocio es la anulación lógica.
/// 4. Subtotales, Total y Stock gestionados automáticamente por el motor de base de datos.
/// </summary>
[ApiController]
[Route("api/factura")]
public class FacturaController : ControllerBase
{
    private readonly IServicioFactura _servicio;

    public FacturaController(IServicioFactura servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF3: Listar todas las facturas con sus nombres resueltos y productos anidados.
    /// Procedimiento: sp_listar_facturas_y_productosporfactura.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var facturas = await _servicio.ListarAsync();
            if (facturas.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "factura",
                total = facturas.Count,
                datos = facturas
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
    /// RF3: Consultar una factura por su número.
    /// Procedimiento: sp_consultar_factura_y_productosporfactura.
    /// Si no existe, responde HTTP 404 Not Found.
    /// </summary>
    [HttpGet("{numero:int}")]
    public async Task<IActionResult> Consultar(int numero)
    {
        try
        {
            var factura = await _servicio.ConsultarAsync(numero);
            return Ok(factura);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Factura no encontrada.",
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
    /// RF3: Emitir una nueva factura en una sola transacción.
    /// Procedimiento: sp_insertar_factura_y_productosporfactura.
    /// 
    /// Códigos de respuesta:
    /// - 201 Created: Factura creada exitosamente.
    /// - 409 Conflict: Cliente o Vendedor referenciado no existe (FK error 547).
    /// - 422 Unprocessable Entity: Colección de productos vacía o inválida.
    /// - 500 Internal Server Error: Stock insuficiente (trigger lanza error 50001).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] FacturaCrear peticion)
    {
        try
        {
            var facturaCreada = await _servicio.CrearAsync(peticion);
            return Ok(facturaCreada);
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
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: el cliente o vendedor no existe.",
                detalle = ex.Message
            });
        }
        catch (SqlException ex)
        {
            // Captura errores lanzados por el motor (ej: THROW 50001 de stock insuficiente)
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error en el motor de base de datos.",
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
    /// RF3: Anulación lógica de factura (borrado lógico).
    /// Procedimiento: sp_anular_factura.
    /// 
    /// Soporta POST y PATCH a /api/factura/{numero}/anular.
    /// Devuelve el stock descontado previamente a los productos involucrados.
    /// - Si la factura no existe -> 404
    /// - Si la factura ya está previamente anulada -> 409 Conflict
    /// </summary>
    [HttpPost("{numero:int}/anular")]
    [HttpPatch("{numero:int}/anular")]
    public async Task<IActionResult> Anular(int numero)
    {
        try
        {
            var resultadoJson = await _servicio.AnularAsync(numero);
            var respuesta = JsonSerializer.Deserialize<object>(resultadoJson);

            return Ok(respuesta);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Factura no encontrada.",
                detalle = ex.Message
            });
        }
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la factura ya se encuentra anulada.",
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
}
