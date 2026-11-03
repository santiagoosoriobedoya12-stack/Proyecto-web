using System.Net;
using System.Net.Http.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

public class ServicioRolUsuario
{
    private readonly HttpClient _http;

    public ServicioRolUsuario(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async Task<List<RolUsuario>> ListarAsync()
    {
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
