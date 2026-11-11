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
/// Controlador HTTP para el recurso 'empresa' (/api/empresa).
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/empresa'
[Route("api/empresa")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.empresas' (403 si no)
[ExigePermiso("interfaz.empresas")]
// Controlador REST 'EmpresaController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class EmpresaController : ControllerBase
{
    private readonly IServicioEmpresa _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public EmpresaController(IServicioEmpresa servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    // Expone método HTTP GET para listar registros (200 OK con datos o 204 No Content si está vacío)
    [HttpGet]
    // Lista todos los registros disponibles respetando el límite máximo solicitado
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var empresas = await _servicio.ListarAsync(limite);
            if (empresas.Count == 0)
            {
                // Devuelve código HTTP 204 No Content según especificación REST
                return NoContent();
            }

            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                tabla = "empresa",
                limite,
                total = empresas.Count,
                datos = empresas
            });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    // Expone método HTTP GET en subruta '{codigo}' para consulta puntual (200 OK o 404 Not Found)
    [HttpGet("{codigo}")]
    // Busca un registro específico por su código único
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var empresa = await _servicio.ObtenerPorCodigoAsync(codigo);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(empresa);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    // Expone método HTTP POST para crear o ejecutar operaciones transaccionales (201 Created o error 422/409)
    [HttpPost]
    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<IActionResult> Crear([FromBody] EmpresaCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.CrearAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new { estado = 200, mensaje = "Empresa creada exitosamente." });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    // Expone método HTTP PUT para reemplazo completo e idempotente del recurso existente
    [HttpPut("{codigo}")]
    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] EmpresaReemplazo peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasAfectadas = await _servicio.ReemplazarAsync(codigo, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new { estado = 200, mensaje = "Empresa reemplazada exitosamente.", filasAfectadas });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    // Expone método HTTP PATCH para actualización parcial enviando solo las propiedades cambiadas
    [HttpPatch("{codigo}")]
    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<IActionResult> ActualizarParcial(string codigo, [FromBody] EmpresaActualizar peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasAfectadas = await _servicio.ActualizarParcialAsync(codigo, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new { estado = 200, mensaje = "Empresa actualizada exitosamente.", filasAfectadas });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    // Expone método HTTP DELETE para eliminar el registro (o anular lógicamente en el caso de factura)
    [HttpDelete("{codigo}")]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> Eliminar(string codigo)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filasEliminadas = await _servicio.EliminarAsync(codigo);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new { estado = 200, mensaje = "Empresa eliminada exitosamente.", filasEliminadas });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Empresa no encontrada.", detalle = ex.Message });
        }
        // Captura parámetro inválido de entrada y responde código HTTP 400 Bad Request
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new { estado = 409, mensaje = "Conflicto de integridad referencial: la empresa está referenciada por otra tabla.", detalle = ex.Message });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error en el motor de base de datos.", detalle = ex.Message });
        }
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}

