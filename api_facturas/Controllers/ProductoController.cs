using ApiFacturas.Autorizacion;
using ApiFacturas.Excepciones;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Controllers;

/// <summary>
/// Controlador HTTP para el recurso 'producto' (/api/producto).
/// Responsabilidades del controlador:
/// 1. Recibir peticiones HTTP y mapear parámetros (de ruta, query string o cuerpo JSON).
/// 2. Invocar los métodos del servicio de negocio (IServicioProducto).
/// 3. Capturar excepciones y traducirlas a sus respectivos códigos de estado HTTP:
///    - ArgumentException -> HTTP 400 (Bad Request)
///    - NoEncontradoExcepcion -> HTTP 404 (Not Found)
///    - SqlException u otras -> HTTP 500 (Internal Server Error) con el detalle del motor.
/// </summary>
[ApiController]
[Route("api/producto")]
[Authorize]
[ExigePermiso("interfaz.productos")]
public class ProductoController : ControllerBase
{
    private readonly IServicioProducto _servicio;

    public ProductoController(IServicioProducto servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF1: Listar productos con query string opcional ?limite=N (por defecto 1000).
    /// Si la tabla está vacía, retorna 204 No Content sin cuerpo.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var productos = await _servicio.ListarAsync(limite);

            if (productos.Count == 0)
            {
                // Criterio C1: La tabla vacía es un resultado válido, no un error -> HTTP 204 sin cuerpo
                return NoContent();
            }

            // Envoltura estándar del contrato: {tabla, limite, total, datos:[...]}
            return Ok(new
            {
                tabla = "producto",
                limite,
                total = productos.Count,
                datos = productos
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
    /// RF2: Obtener producto por código (/api/producto/{codigo}).
    /// Si no existe, retorna HTTP 404.
    /// </summary>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        try
        {
            var producto = await _servicio.ObtenerPorCodigoAsync(codigo);
            return Ok(producto);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
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
    /// RF3: Crear producto (POST /api/producto).
    /// El cuerpo se valida automáticamente con la petición ProductoCrear (si falla -> 422).
    /// Si la clave primaria ya existe en la BD, la violación de PK retorna 500 con el mensaje del motor.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProductoCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto creado exitosamente."
            });
        }
        catch (SqlException ex)
        {
            // Criterio C4: Violación de PK duplicada la defiende la base de datos -> HTTP 500 con detalle
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
    /// RF4: Reemplazar producto completo (PUT /api/producto/{codigo}).
    /// Todos los campos del body son obligatorios.
    /// </summary>
    [HttpPut("{codigo}")]
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] ProductoReemplazo peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(codigo, peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto reemplazado exitosamente.",
                filasAfectadas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
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
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: el producto tiene renglones de factura asociados.",
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
    /// RF5: Actualización PARCIAL (PATCH /api/producto/{codigo}).
    /// Solo se modifican los campos enviados en el cuerpo JSON.
    /// Si el cuerpo está vacío, devuelve HTTP 400.
    /// </summary>
    [HttpPatch("{codigo}")]
    public async Task<IActionResult> ActualizarParcial(string codigo, [FromBody] ProductoActualizar peticion)
    {
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(codigo, peticion);
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto actualizado exitosamente.",
                filasAfectadas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
                detalle = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            // Criterio C5: Body vacío en PATCH retorna 400 (regla de negocio rota)
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
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
    /// RF6: Eliminar producto (DELETE /api/producto/{codigo}).
    /// Si no existe (o ya fue eliminado en una llamada anterior), retorna HTTP 404.
    /// </summary>
    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(codigo);
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto eliminado exitosamente.",
                filasEliminadas
            });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
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
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: el producto tiene renglones de factura asociados.",
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

