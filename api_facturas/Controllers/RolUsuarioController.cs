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
/// Controlador HTTP para la tabla puente 'rol_usuario' (/api/rol-usuario).
/// 
/// Características (RF4):
/// 1. Consulta por ambos extremos: /usuario/{email} y /rol/{idrol}.
/// 2. Inserción con control de duplicados (409 Conflict).
/// 3. Eliminación requiriendo ambas claves.
/// 4. Sin verbos PUT ni PATCH.
/// </summary>
[ApiController]
// Define el prefijo de ruta URL en la API para este recurso: 'api/rol-usuario'
[Route("api/rol-usuario")]
// Versión 3: Bloquea peticiones sin token JWT válido devolviendo HTTP 401 Unauthorized
[Authorize]
// Versión 3: Exige en SQL Server ('verificar_acceso_ruta') que el rol actual tenga permiso a 'interfaz.roles' (403 si no)
[ExigePermiso("interfaz.roles")]
// Controlador REST 'RolUsuarioController': recibe peticiones HTTP, invoca la capa de servicio y traduce excepciones a códigos de estado HTTP
public class RolUsuarioController : ControllerBase
{
    private readonly IServicioRolUsuario _servicio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public RolUsuarioController(IServicioRolUsuario servicio)
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
            tabla = "rol_usuario",
            total = lista.Count,
            datos = lista
        });
    }

    // Expone método HTTP GET en subruta 'usuario/{email}' para consulta puntual (200 OK o 404 Not Found)
    [HttpGet("usuario/{email}")]
    // Consulta registros filtrados según criterio (ObtenerPorUsuario)
    public async Task<IActionResult> ObtenerPorUsuario(string email)
    {
        var lista = await _servicio.ObtenerPorUsuarioAsync(email);
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
    public async Task<IActionResult> Agregar([FromBody] RolUsuarioCrear peticion)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.AgregarAsync(peticion);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Rol asignado al usuario exitosamente.",
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
                mensaje = "Conflicto de clave foránea: el usuario o el rol no existe.",
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
    [HttpDelete("{email}/{idrol:int}")]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> EliminarPorRuta(string email, int idrol)
    {
        return await EjecutarEliminacion(email, idrol);
    }

    // Expone método HTTP DELETE para eliminar el registro (o anular lógicamente en el caso de factura)
    [HttpDelete]
    // Elimina el registro especificado validando su existencia previa
    public async Task<IActionResult> EliminarPorQuery([FromQuery] string fkemail, [FromQuery] int fkidrol)
    {
        return await EjecutarEliminacion(fkemail, fkidrol);
    }

    // Ejecuta la operación 'EjecutarEliminacion' dentro de la capa correspondiente
    private async Task<IActionResult> EjecutarEliminacion(string email, int idrol)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await _servicio.EliminarAsync(email, idrol);
            // Devuelve código HTTP 200 OK con el resultado serializado en JSON
            return Ok(new
            {
                estado = 200,
                mensaje = "Relación rol-usuario eliminada exitosamente."
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
