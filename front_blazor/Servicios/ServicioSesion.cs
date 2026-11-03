using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Cliente HTTP tipado para el inicio y cierre de sesión (Versión 3).
/// Es el único servicio que puede operar de forma anónima hacia POST /api/sesion.
/// </summary>
public class ServicioSesion
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;

    public ServicioSesion(HttpClient http, EstadoSesion estadoSesion)
    {
        _http = http;
        _estadoSesion = estadoSesion;
    }

    /// <summary>
    /// Intenta autenticar al usuario contra POST /api/sesion y obtiene sus permisos para armar el menú.
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> IniciarSesionAsync(string email, string contrasena)
    {
        try
        {
            var peticion = new PeticionSesion { Email = email, Contrasena = contrasena };
            var respuesta = await _http.PostAsJsonAsync("api/sesion", peticion);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContenido = await respuesta.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(errorContenido);
                    if (doc.RootElement.TryGetProperty("mensaje", out var mensajeProp))
                    {
                        return (false, mensajeProp.GetString() ?? "Error de autenticación.");
                    }
                }
                catch { }

                return (false, "El correo o la contrasena no son correctos.");
            }

            var sesion = await respuesta.Content.ReadFromJsonAsync<RespuestaSesion>();
            if (sesion == null || string.IsNullOrWhiteSpace(sesion.Token))
            {
                return (false, "Respuesta de sesión inválida de la API.");
            }

            // Consultar permisos autorizados con el token recibido
            var rutasPermitidas = new List<string>();
            var peticionPermisos = new HttpRequestMessage(HttpMethod.Get, "api/permisos/mios");
            peticionPermisos.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);

            var respuestaPermisos = await _http.SendAsync(peticionPermisos);
            if (respuestaPermisos.IsSuccessStatusCode)
            {
                var permisosObj = await respuestaPermisos.Content.ReadFromJsonAsync<RespuestaPermisosMios>();
                if (permisosObj?.Datos != null)
                {
                    rutasPermitidas = permisosObj.Datos;
                }
            }

            // Guardar sesión en el circuito
            _estadoSesion.IniciarSesion(sesion.Token, sesion.Email, sesion.Roles, rutasPermitidas);
            return (true, "Sesión iniciada correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error de comunicación con la API: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra la sesión activa en este circuito.
    /// </summary>
    public void CerrarSesion()
    {
        _estadoSesion.CerrarSesion();
    }
}

