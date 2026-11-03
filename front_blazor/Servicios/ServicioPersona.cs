using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP para el recurso 'persona'.
/// </summary>
public class ServicioPersona
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


    public ServicioPersona(HttpClient http, EstadoSesion estadoSesion)
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


    public async Task<List<Persona>> ListarAsync(int limite = 1000)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/persona?limite={limite}");
        if (respuesta.StatusCode == HttpStatusCode.NoContent)
        {
            return new List<Persona>();
        }

        respuesta.EnsureSuccessStatusCode();
        var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Persona>>();
        return sobre?.Datos ?? new List<Persona>();
    }

    public async Task<Persona?> ObtenerPorCodigoAsync(string codigo)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/persona/{Uri.EscapeDataString(codigo)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Persona>();
    }

    public async Task<ResultadoOperacion> CrearAsync(Persona persona)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/persona", new
            {
                codigo = persona.Codigo,
                nombre = persona.Nombre,
                email = persona.Email,
                telefono = persona.Telefono
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Persona creada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Persona persona)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PutAsJsonAsync($"api/persona/{Uri.EscapeDataString(persona.Codigo)}", new
            {
                nombre = persona.Nombre,
                email = persona.Email,
                telefono = persona.Telefono
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Ficha completa guardada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> GuardarSoloLoCambiadoAsync(string codigo, Dictionary<string, object> camposModificados)
    {
        Autorizar();
        try
        {
            var json = JsonSerializer.Serialize(camposModificados);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _http.PatchAsync($"api/persona/{Uri.EscapeDataString(codigo)}", contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Cambios guardados exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> EliminarAsync(string codigo)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.DeleteAsync($"api/persona/{Uri.EscapeDataString(codigo)}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Persona eliminada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

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

