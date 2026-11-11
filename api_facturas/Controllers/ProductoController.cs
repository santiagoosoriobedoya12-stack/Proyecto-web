// Filtro personalizado [ExigePermiso] que valida roles y rutas en base de datos (Versión 3)
using ApiFacturas.Autorizacion;
// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de lógica y reglas de negocio
using ApiFacturas.Servicios;
// Atributos [Authorize] y [AllowAnonymous] para proteger o abrir endpoints HTTP (Versión 3)
using Microsoft.AspNetCore.Authorization;
// Componentes base de ASP.NET Core MVC (ControllerBase, IActionResult, [HttpGet], [Route])
using Microsoft.AspNetCore.Mvc;
// Driver oficial ADO.NET para conectarse de forma asíncrona a Microsoft SQL Server
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
// Define el prefijo de ruta URL en la API para este recurso: 'api/producto'
[Route("api/producto")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.productos' (403 si no)
[ExigePermiso("interfaz.productos")]
// Controlador REST 'ProductoController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class ProductoController : ControllerBase
{
    private readonly IServicioProducto _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ProductoController(IServicioProducto servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF1: Listar productos con query string opcional ?limite=N (por defecto 1000).
    /// Si la tabla está vacía, retorna 204 No Content sin cuerpo.
    /// </summary>
    [HttpGet]
    // Lista todos los registros disponibles respetando el límite máximo solicitado
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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
    // Busca un registro específico por su código único
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var producto = await _servicio.ObtenerPorCodigoAsync(codigo);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(producto);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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
    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<IActionResult> Crear([FromBody] ProductoCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.CrearAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto creado exitosamente."
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
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
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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
    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] ProductoReemplazo peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(codigo, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto reemplazado exitosamente.",
                filasAfectadas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: el producto tiene renglones de factura asociados.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error en el motor de base de datos.",
                detalle = ex.Message
            });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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
    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<IActionResult> ActualizarParcial(string codigo, [FromBody] ProductoActualizar peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(codigo, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto actualizado exitosamente.",
                filasAfectadas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
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
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error en el motor de base de datos.",
                detalle = ex.Message
            });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> Eliminar(string codigo)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(codigo);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Producto eliminado exitosamente.",
                filasEliminadas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Producto no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                estado = 400,
                mensaje = "Parámetros inválidos.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: el producto tiene renglones de factura asociados.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                estado = 500,
                mensaje = "Error en el motor de base de datos.",
                detalle = ex.Message
            });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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

