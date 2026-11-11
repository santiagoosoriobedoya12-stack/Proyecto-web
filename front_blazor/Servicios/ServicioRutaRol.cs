// Códigos de estado HTTP estándar (NoContent, Unauthorized, Forbidden, Conflict)
using System.Net;
// Métodos de extensión para enviar y deserializar JSON directamente con HttpClient
using System.Net.Http.Json;
// Clases DTO usadas por los componentes de Blazor para mostrar y editar datos
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP cliente para la tabla puente 'rutarol' (/api/rutarol).
/// Permite consultar permisos asignados, asociar una ruta a un rol y remover el acceso.
/// </summary>
public class ServicioRutaRol
{
    // Cliente HTTP inyectado para comunicarse con la API REST
    private readonly HttpClient _http;
    // Estado de sesión Scoped que conserva el token JWT en memoria del circuito Blazor
    private readonly EstadoSesion _estadoSesion;


    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioRutaRol(HttpClient http, EstadoSesion estadoSesion)
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
    /// Consulta todas las asociaciones existentes entre rutas y roles.
    /// </summary>
    public async Task<List<RutaRol>> ListarAsync()
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.GetAsync("api/rutarol");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
                return new List<RutaRol>();

            respuesta.EnsureSuccessStatusCode();
            var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<RutaRol>>();
            return sobre?.Datos ?? new List<RutaRol>();
        }
        catch
        {
            return new List<RutaRol>();
        }
    }

    /// <summary>
    /// Asigna una ruta a un rol enviando un POST a /api/rutarol.
    /// Si la relación ya existe, la API retorna 409 Conflict.
    /// </summary>
    public async Task<ResultadoOperacion> AsignarAsync(int idRuta, int idRol)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/rutarol", new
            {
                fkidruta = idRuta,
                fkidrol = idRol
            });

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Permiso de ruta asignado exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    /// <summary>
    /// Elimina la relación exigiendo ambas claves primarias.
    /// </summary>
    public async Task<ResultadoOperacion> QuitarAsync(int idRuta, int idRol)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.DeleteAsync($"api/rutarol/{idRuta}/{idRol}");
            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Permiso eliminado exitosamente." };

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

