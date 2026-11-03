using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP exclusivo para el recurso 'empresa'.
/// </summary>
public class ServicioEmpresa
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


    public ServicioEmpresa(HttpClient http, EstadoSesion estadoSesion)
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


    public async Task<List<Empresa>> ListarAsync(int limite = 1000)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/empresa?limite={limite}");
        if (respuesta.StatusCode == HttpStatusCode.NoContent)
        {
            return new List<Empresa>();
        }

        respuesta.EnsureSuccessStatusCode();
        var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Empresa>>();
        return sobre?.Datos ?? new List<Empresa>();
    }

    public async Task<Empresa?> ObtenerPorCodigoAsync(string codigo)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/empresa/{Uri.EscapeDataString(codigo)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Empresa>();
    }

    public async Task<ResultadoOperacion> CrearAsync(Empresa empresa)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/empresa", new
            {
                codigo = empresa.Codigo,
                nombre = empresa.Nombre
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Empresa creada exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Empresa empresa)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PutAsJsonAsync($"api/empresa/{Uri.EscapeDataString(empresa.Codigo)}", new
            {
                nombre = empresa.Nombre
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

            var respuesta = await _http.PatchAsync($"api/empresa/{Uri.EscapeDataString(codigo)}", contenido);

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
            var respuesta = await _http.DeleteAsync($"api/empresa/{Uri.EscapeDataString(codigo)}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Empresa eliminada exitosamente." };
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

