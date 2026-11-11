// Códigos de estado HTTP estándar (NoContent, Unauthorized, Forbidden, Conflict)
using System.Net;
// Métodos de extensión para enviar y deserializar JSON directamente con HttpClient
using System.Net.Http.Json;
using System.Text;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Clases DTO usadas por los componentes de Blazor para mostrar y editar datos
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP cliente para la entidad 'cliente' (/api/cliente).
/// Consume los endpoints REST de la API desde la interfaz Blazor Server.
/// </summary>
public class ServicioCliente
{
    // Cliente HTTP inyectado para comunicarse con la API REST
    private readonly HttpClient _http;
    // Estado de sesión Scoped que conserva el token JWT en memoria del circuito Blazor
    private readonly EstadoSesion _estadoSesion;


    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioCliente(HttpClient http, EstadoSesion estadoSesion)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _estadoSesion = estadoSesion ?? throw new ArgumentNullException(nameof(estadoSesion));
    }

    /// <summary>
    /// Inyecta el token Bearer en el cliente HTTP si hay una sesión activa en el circuito (Versión 3).
    /// </summary>
    private void Autorizar()
    {
        if (_estadoSesion.EstaAutenticado)
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _estadoSesion.Token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }


    /// <summary>
    /// Consulta el listado de clientes. Si la API responde 204 No Content, devuelve una lista vacía.
    /// Si la API no está disponible, captura la excepción y retorna lista vacía para mantener la UI en pie.
    /// </summary>
    public async Task<List<Cliente>> ListarAsync(int limite = 1000)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.GetAsync($"api/cliente?limite={limite}");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<Cliente>();
            }

            respuesta.EnsureSuccessStatusCode();
            var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Cliente>>();
            return sobre?.Datos ?? new List<Cliente>();
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException)
        {
            // Resiliencia C10/C18: Si la API se cae, el frontend se mantiene en pie mostrando 0 filas
            return new List<Cliente>();
        }
    }

    /// <summary>
    /// Consulta un cliente individual por su ID.
    /// </summary>
    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        var respuesta = await _http.GetAsync($"api/cliente/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Cliente>();
    }

    /// <summary>
    /// Crea un cliente enviando un POST a /api/cliente.
    /// </summary>
    public async Task<ResultadoOperacion> CrearAsync(Cliente cliente)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/cliente", new
            {
                credito = cliente.Credito,
                fkcodpersona = cliente.Fkcodpersona,
                fkcodempresa = string.IsNullOrWhiteSpace(cliente.Fkcodempresa) ? null : cliente.Fkcodempresa
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Cliente creado exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    /// <summary>
    /// Reemplazo completo (PUT /api/cliente/{id}).
    /// </summary>
    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Cliente cliente)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.PutAsJsonAsync($"api/cliente/{cliente.Id}", new
            {
                credito = cliente.Credito,
                fkcodpersona = cliente.Fkcodpersona,
                fkcodempresa = string.IsNullOrWhiteSpace(cliente.Fkcodempresa) ? null : cliente.Fkcodempresa
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Ficha completa de cliente guardada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    /// <summary>
    /// Actualización parcial (PATCH /api/cliente/{id}).
    /// </summary>
    public async Task<ResultadoOperacion> GuardarSoloLoCambiadoAsync(int id, Dictionary<string, object?> camposModificados)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var json = JsonSerializer.Serialize(camposModificados);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _http.PatchAsync($"api/cliente/{id}", contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Cambios parciales guardados exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    /// <summary>
    /// Elimina un cliente por su ID (DELETE /api/cliente/{id}).
    /// </summary>
    public async Task<ResultadoOperacion> EliminarAsync(int id)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.DeleteAsync($"api/cliente/{id}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Cliente eliminado exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    /// <summary>
    /// Extrae de forma segura el mensaje y detalles de error (400, 404, 409, 422, 500)
    /// enviados por la API en formato estándar JSON.
    /// </summary>
    private static async Task<ResultadoOperacion> ExtraerErrorAsync(HttpResponseMessage respuesta)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ErrorApi>();
            if (error != null)
            {
                var mensaje = error.Mensaje ?? "Ocurrió un error al procesar la solicitud.";
                if (!string.IsNullOrWhiteSpace(error.Detalle))
                {
                    mensaje += $" ({error.Detalle})";
                }

                return new ResultadoOperacion
                {
                    Exitoso = false,
                    Mensaje = mensaje,
                    ErroresValidacion = error.Errores
                };
            }
        }
        catch { }

        return new ResultadoOperacion
        {
            Exitoso = false,
            Mensaje = $"Error del servidor: HTTP {(int)respuesta.StatusCode} ({respuesta.ReasonPhrase})"
        };
    }
}

