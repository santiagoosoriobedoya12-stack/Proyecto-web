// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
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
/// Controlador HTTP para el recurso Maestro-Detalle 'factura' (/api/factura).
/// 
/// CARACTERÍSTICAS DE LA VERSIÓN 2 (RF3) Y VERSIÓN 3:
/// 1. Cuatro operaciones exclusivas: Listar, Consultar una, Crear y Anular.
/// 2. Todas operadas 100% mediante Procedimientos Almacenados.
/// 3. Sin DELETE físico en la API: la regla del negocio es la anulación lógica.
/// 4. Subtotales, Total y Stock gestionados automáticamente por el motor de base de datos.
/// 5. Protección con Token JWT y permiso 'interfaz.facturas'.
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/factura'
[Route("api/factura")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.facturas' (403 si no)
[ExigePermiso("interfaz.facturas")]
// Controlador REST 'FacturaController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class FacturaController : ControllerBase
{
    private readonly IServicioFactura _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public FacturaController(IServicioFactura servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF3: Listar todas las facturas con sus nombres resueltos y productos anidados.
    /// Procedimiento: sp_listar_facturas_y_productosporfactura.
    /// </summary>
    [HttpGet]
    // Lista todos los registros disponibles respetando el límite máximo solicitado
    public async Task<IActionResult> Listar()
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var facturas = await _servicio.ListarAsync();
            if (facturas.Count == 0)
            {
                // Devuelve código HTTP 204 No Content según especificación REST
                return NoContent();
            }

            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                tabla = "factura",
                total = facturas.Count,
                datos = facturas
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
    /// RF3: Consultar una factura por su número.
    /// Procedimiento: sp_consultar_factura_y_productosporfactura.
    /// Si no existe, responde HTTP 404 Not Found.
    /// </summary>
    [HttpGet("{numero:int}")]
    // Ejecuta la operación 'Consultar' dentro de la capa correspondiente
    public async Task<IActionResult> Consultar(int numero)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var factura = await _servicio.ConsultarAsync(numero);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(factura);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Factura no encontrada.",
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
    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<IActionResult> Crear([FromBody] FacturaCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var facturaCreada = await _servicio.CrearAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(facturaCreada);
        }
        // Captura conflicto de clave foránea o estado y responde código HTTP 409 Conflict
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: el cliente o vendedor no existe.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
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
    /// RF3: Anulación lógica de factura (borrado lógico).
    /// Procedimiento: sp_anular_factura.
    /// 
    /// Soporta POST y PATCH a /api/factura/{numero}/anular.
    /// Devuelve el stock descontado previamente a los productos involucrados.
    /// - Si la factura no existe -> 404
    /// - Si la factura ya está previamente anulada -> 409 Conflict
    /// </summary>
    [HttpPost("{numero:int}/anular")]
    // Expone método HTTP PATCH para actualización parcial enviando solo las propiedades cambiadas
    [HttpPatch("{numero:int}/anular")]
    // Anula lógicamente la factura en SQL Server devolviendo el stock al inventario
    public async Task<IActionResult> Anular(int numero)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var resultadoJson = await _servicio.AnularAsync(numero);
            var respuesta = JsonSerializer.Deserialize<object>(resultadoJson);

            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(respuesta);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Factura no encontrada.",
                detalle = ex.Message
            });
        }
        // Captura conflicto de clave foránea o estado y responde código HTTP 409 Conflict
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la factura ya se encuentra anulada.",
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
