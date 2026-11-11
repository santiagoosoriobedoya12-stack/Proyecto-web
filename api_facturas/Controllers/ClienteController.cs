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
/// Controlador HTTP para el recurso 'cliente' (/api/cliente) — Versión 2 / Versión 3.
/// 
/// Novedades de la Versión 2:
/// 1. Claves Foráneas: 'fkcodpersona' (obligatoria) y 'fkcodempresa' (opcional).
/// 2. Código HTTP 409 Conflict: Cuando se envía una clave foránea que no existe en la base de datos
///    (error 547 de SQL Server) o cuando una eliminación viola la integridad referencial.
/// 3. Mapeo del molde estándar de 5 verbos: GET (listar/consultar), POST, PUT, PATCH, DELETE.
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/cliente'
[Route("api/cliente")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.clientes' (403 si no)
[ExigePermiso("interfaz.clientes")]
// Controlador REST 'ClienteController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class ClienteController : ControllerBase
{
    private readonly IServicioCliente _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ClienteController(IServicioCliente servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    /// <summary>
    /// RF1: Listar clientes con soporte de ?limite=N.
    /// Si la tabla no tiene filas, responde 204 No Content.
    /// </summary>
    [HttpGet]
    // Lista todos los registros disponibles respetando el límite máximo solicitado
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var clientes = await _servicio.ListarAsync(limite);
            if (clientes.Count == 0)
            {
                // Devuelve código HTTP 204 No Content según especificación REST
                return NoContent();
            }

            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                tabla = "cliente",
                limite,
                total = clientes.Count,
                datos = clientes
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
    /// RF1: Consultar un cliente específico por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    // Busca un registro específico por su identificador numérico
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var cliente = await _servicio.ObtenerPorIdAsync(id);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(cliente);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
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
    /// RF1: Crear un nuevo cliente (POST).
    /// El ID es autogenerado por IDENTITY. Si la FK no existe en persona o empresa, responde 409 Conflict.
    /// </summary>
    [HttpPost]
    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<IActionResult> Crear([FromBody] ClienteCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var nuevoId = await _servicio.CrearAsync(peticion);
            var clienteCreado = await _servicio.ObtenerPorIdAsync(nuevoId);

            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente creado exitosamente.",
                id = nuevoId,
                datos = clienteCreado
            });
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
            // Error 547: FOREIGN KEY constraint violation (la persona o empresa referenciada no existe)
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
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
    /// RF1: Reemplazar completamente un cliente (PUT).
    /// Si la clave foránea no existe, responde 409 Conflict.
    /// </summary>
    [HttpPut("{id:int}")]
    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<IActionResult> Reemplazar(int id, [FromBody] ClienteReemplazo peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filas = await _servicio.ReemplazarAsync(id, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente reemplazado exitosamente.",
                filasAfectadas = filas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
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
    /// RF1: Actualizar parcialmente un cliente (PATCH).
    /// </summary>
    [HttpPatch("{id:int}")]
    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] ClienteActualizar peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filas = await _servicio.ActualizarParcialAsync(id, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente actualizado exitosamente.",
                filasAfectadas = filas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la persona o empresa referenciada no existe.",
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
    /// RF1: Eliminar un cliente por su ID (DELETE).
    /// Si el cliente ya tiene facturas, el motor impedirá el borrado (409 Conflict).
    /// </summary>
    [HttpDelete("{id:int}")]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> Eliminar(int id)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var filas = await _servicio.EliminarAsync(id);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Cliente eliminado exitosamente.",
                filasAfectadas = filas
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Cliente no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de integridad referencial: no se puede eliminar el cliente porque tiene registros relacionados.",
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
}

