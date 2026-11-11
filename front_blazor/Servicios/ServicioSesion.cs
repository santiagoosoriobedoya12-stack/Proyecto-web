using System.Net.Http.Headers;
// Métodos de extensión para enviar y deserializar JSON directamente con HttpClient
using System.Net.Http.Json;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Clases DTO usadas por los componentes de Blazor para mostrar y editar datos
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Cliente HTTP tipado para el inicio y cierre de sesión (Versión 3).
/// Es el único servicio que puede operar de forma anónima hacia POST /api/sesion.
/// </summary>
public class ServicioSesion
{
    // Cliente HTTP inyectado para comunicarse con la API REST
    private readonly HttpClient _http;
    // Estado de sesión Scoped que conserva el token JWT en memoria del circuito Blazor
    private readonly EstadoSesion _estadoSesion;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
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
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var peticion = new PeticionSesion { Email = email, Contrasena = contrasena };
            var respuesta = await _http.PostAsJsonAsync("api/sesion", peticion);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContenido = await respuesta.Content.ReadAsStringAsync();
                // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Red de seguridad final: captura excepciones imprevistas y responde HTTP 500 Internal Server Error
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

