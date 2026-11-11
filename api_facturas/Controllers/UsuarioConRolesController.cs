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
/// Controlador HTTP para el recurso compuesto 'api/usuario-con-roles' (RF5).
/// Protegido con [Authorize] y [ExigePermiso("interfaz.usuarios")].
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/usuario-con-roles'
[Route("api/usuario-con-roles")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.usuarios' (403 si no)
[ExigePermiso("interfaz.usuarios")]
// Controlador REST 'UsuarioConRolesController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class UsuarioConRolesController : ControllerBase
{
    private readonly IServicioUsuarioConRoles _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public UsuarioConRolesController(IServicioUsuarioConRoles servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    // Expone método HTTP GET para listar registros (200 OK con datos o 204 No Content si está vacío)
    [HttpGet]
    // Lista todos los registros disponibles respetando el límite máximo solicitado
    public async Task<IActionResult> Listar()
    {
        var lista = await _servicio.ListarAsync();
        if (lista.Count == 0)
            return NoContent();

        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(new
        {
            tabla = "usuario_con_roles",
            total = lista.Count,
            datos = lista
        });
    }

    // Expone método HTTP GET en subruta '{email}' para consulta puntual (200 OK o 404 Not Found)
    [HttpGet("{email}")]
    // Ejecuta la operación 'Consultar' dentro de la capa correspondiente
    public async Task<IActionResult> Consultar(string email)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var usuario = await _servicio.ConsultarAsync(email);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(usuario);
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
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
    }

    // Expone método HTTP POST para crear o ejecutar operaciones transaccionales (201 Created o error 422/409)
    [HttpPost]
    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<IActionResult> Crear([FromBody] UsuarioConRolesCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(creado);
        }
        // Captura conflicto de clave foránea o estado y responde código HTTP 409 Conflict
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: usuario duplicado o rol inexistente.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: el usuario ya existe en el sistema.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: uno de los roles no existe.",
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

    // Expone método HTTP PUT para reemplazo completo e idempotente del recurso existente
    [HttpPut("{email}")]
    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<IActionResult> Actualizar(string email, [FromBody] UsuarioConRolesActualizar peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var actualizado = await _servicio.ActualizarAsync(email, peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Usuario y roles actualizados exitosamente.",
                datos = actualizado
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
                detalle = ex.Message
            });
        }
        // Captura conflicto de clave foránea o estado y responde código HTTP 409 Conflict
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto con los roles asignados.",
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

    // Expone método HTTP DELETE para eliminar el registro (o anular lógicamente en el caso de factura)
    [HttpDelete("{email}")]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> Eliminar(string email)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.EliminarAsync(email);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Usuario y sus asignaciones de roles eliminados exitosamente."
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Usuario no encontrado.",
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
