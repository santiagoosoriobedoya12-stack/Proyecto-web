// Códigos de estado HTTP estándar (NoContent, Unauthorized, Forbidden, Conflict)
using System.Net;
// Métodos de extensión para enviar y deserializar JSON directamente con HttpClient
using System.Net.Http.Json;
// Clases DTO usadas por los componentes de Blazor para mostrar y editar datos
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

// Servicio 'ServicioRolUsuario': encapsula validaciones de dominio o llamadas HTTP tipadas hacia la API
public class ServicioRolUsuario
{
    // Cliente HTTP inyectado para comunicarse con la API REST
    private readonly HttpClient _http;
    // Estado de sesión Scoped que conserva el token JWT en memoria del circuito Blazor
    private readonly EstadoSesion _estadoSesion;


    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
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
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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

    // Ejecuta la operación 'AsignarAsync' dentro de la capa correspondiente
    public async Task<ResultadoOperacion> AsignarAsync(string email, int idRol)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    // Ejecuta la operación 'QuitarAsync' dentro de la capa correspondiente
    public async Task<ResultadoOperacion> QuitarAsync(string email, int idRol)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.DeleteAsync($"api/rol-usuario/{email}/{idRol}");
            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Asignación eliminada exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    private static async Task<ResultadoOperacion> ExtraerErrorAsync(HttpResponseMessage respuesta)
    {
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
