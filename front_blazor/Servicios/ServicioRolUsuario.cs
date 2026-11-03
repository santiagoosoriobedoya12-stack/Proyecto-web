using System.Net;
using System.Net.Http.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

public class ServicioRolUsuario
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


    public ServicioRolUsuario(HttpClient http, EstadoSesion estadoSesion)
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


    public async Task<List<RolUsuario>> ListarAsync()
    {
        Autorizar();
        try
        {
            var respuesta = await _http.GetAsync("api/rol-usuario");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
                return new List<RolUsuario>();

            respuesta.EnsureSuccessStatusCode();
            var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<RolUsuario>>();
            return sobre?.Datos ?? new List<RolUsuario>();
        }
        catch
        {
            return new List<RolUsuario>();
        }
    }

    public async Task<ResultadoOperacion> AsignarAsync(string email, int idRol)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/rol-usuario", new
            {
                fkemail = email,
                fkidrol = idRol
            });

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Rol asignado exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    public async Task<ResultadoOperacion> QuitarAsync(string email, int idRol)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.DeleteAsync($"api/rol-usuario/{email}/{idRol}");
            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Asignación eliminada exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    private static async Task<ResultadoOperacion> ExtraerErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ErrorApi>();
            if (error != null)
            {
                var msg = error.Mensaje ?? "Error en la petición";
                if (!string.IsNullOrWhiteSpace(error.Detalle)) msg += $" ({error.Detalle})";
                return new ResultadoOperacion { Exitoso = false, Mensaje = msg, ErroresValidacion = error.Errores };
            }
        }
        catch { }

        return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error HTTP {(int)respuesta.StatusCode}" };
    }
}
