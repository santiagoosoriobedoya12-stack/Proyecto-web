using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP cliente para la entidad 'cliente' (/api/cliente).
/// Consume los endpoints REST de la API desde la interfaz Blazor Server.
/// </summary>
public class ServicioCliente
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


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
        Autorizar();
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
        Autorizar();
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
        Autorizar();
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
        Autorizar();
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
        Autorizar();
        try
        {
            var respuesta = await _http.DeleteAsync($"api/cliente/{id}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Cliente eliminado exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
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

