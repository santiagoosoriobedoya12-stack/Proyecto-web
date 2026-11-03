using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP para el recurso 'usuario'.
/// </summary>
public class ServicioUsuario
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


    public ServicioUsuario(HttpClient http, EstadoSesion estadoSesion)
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


    public async Task<List<Usuario>> ListarAsync(int limite = 1000)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/usuario?limite={limite}");
        if (respuesta.StatusCode == HttpStatusCode.NoContent)
        {
            return new List<Usuario>();
        }

        respuesta.EnsureSuccessStatusCode();
        var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Usuario>>();
        return sobre?.Datos ?? new List<Usuario>();
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/usuario/{Uri.EscapeDataString(email)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Usuario>();
    }

    public async Task<ResultadoOperacion> CrearAsync(Usuario usuario)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/usuario", new
            {
                email = usuario.Email,
                contrasena = usuario.Contrasena
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario creado exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Usuario usuario)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PutAsJsonAsync($"api/usuario/{Uri.EscapeDataString(usuario.Email)}", new
            {
                contrasena = usuario.Contrasena
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Contraseña actualizada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> GuardarSoloLoCambiadoAsync(string email, Dictionary<string, object> camposModificados)
    {
        Autorizar();
        try
        {
            var json = JsonSerializer.Serialize(camposModificados);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _http.PatchAsync($"api/usuario/{Uri.EscapeDataString(email)}", contenido);

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

    public async Task<ResultadoOperacion> EliminarAsync(string email)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.DeleteAsync($"api/usuario/{Uri.EscapeDataString(email)}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario eliminado exitosamente." };
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

