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
/// Controlador HTTP para la tabla puente 'rutarol' (/api/rutarol).
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/rutarol'
[Route("api/rutarol")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.permisos' (403 si no)
[ExigePermiso("interfaz.permisos")]
// Controlador REST 'RutaRolController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class RutaRolController : ControllerBase
{
    private readonly IServicioRutaRol _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RutaRolController(IServicioRutaRol servicio)
    {
        _servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));
    }

    // Expone método HTTP GET para listar registros (200 OK con datos o 204 No Content si está vacío)
    [HttpGet]
    // Consulta en base de datos la colección completa de registros con límite TOP
    public async Task<IActionResult> ObtenerTodos()
    {
        var lista = await _servicio.ObtenerTodosAsync();
        if (lista.Count == 0)
            return NoContent();

        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(new
        {
            tabla = "rutarol",
            total = lista.Count,
            datos = lista
        });
    }

    // Expone método HTTP GET en subruta 'ruta/{idruta:int}' para consulta puntual (200 OK o 404 Not Found)
    [HttpGet("ruta/{idruta:int}")]
    // Consulta registros filtrados según criterio (ObtenerPorRuta)
    public async Task<IActionResult> ObtenerPorRuta(int idruta)
    {
        var lista = await _servicio.ObtenerPorRutaAsync(idruta);
        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(lista);
    }

    // Expone método HTTP GET en subruta 'rol/{idrol:int}' para consulta puntual (200 OK o 404 Not Found)
    [HttpGet("rol/{idrol:int}")]
    // Consulta registros filtrados según criterio (ObtenerPorRol)
    public async Task<IActionResult> ObtenerPorRol(int idrol)
    {
        var lista = await _servicio.ObtenerPorRolAsync(idrol);
        // Devuelve código HTTP 200 OK con el resultado serializado en JSON
        return Ok(lista);
    }

    // Expone método HTTP POST para crear o ejecutar operaciones transaccionales (201 Created o error 422/409)
    [HttpPost]
    // Ejecuta la operación 'Agregar' dentro de la capa correspondiente
    public async Task<IActionResult> Agregar([FromBody] RutaRolCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.AgregarAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Permiso de ruta asignado al rol exitosamente.",
                datos = peticion
            });
        }
        // Captura conflicto de clave foránea o estado y responde código HTTP 409 Conflict
        catch (ConflictoExcepcion ex)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la relación ya existe.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto: la relación ya existe en la base de datos.",
                detalle = ex.Message
            });
        }
        // Captura errores nativos de SQL Server (ej. 547 FK o 2627 duplicado) y responde 409 o 500
        catch (SqlException ex) when (ex.Number == 547)
        {
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "Conflicto de clave foránea: la ruta o el rol no existe.",
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
    [HttpDelete("{idruta:int}/{idrol:int}")]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> EliminarPorRuta(int idruta, int idrol)
    {
        return await EjecutarEliminacion(idruta, idrol);
    }

    // Expone método HTTP DELETE para eliminar el registro (o anular lógicamente en el caso de factura)
    [HttpDelete]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> EliminarPorQuery([FromQuery] int fkidruta, [FromQuery] int fkidrol)
    {
        return await EjecutarEliminacion(fkidruta, fkidrol);
    }

    // Ejecuta la operación 'EjecutarEliminacion' dentro de la capa correspondiente
    private async Task<IActionResult> EjecutarEliminacion(int idruta, int idrol)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.EliminarAsync(idruta, idrol);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Permiso de ruta eliminado del rol exitosamente."
            });
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new
            {
                estado = 404,
                mensaje = "Relación no encontrada.",
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
