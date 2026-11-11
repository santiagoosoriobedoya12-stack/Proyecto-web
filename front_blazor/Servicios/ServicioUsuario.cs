// Códigos de estado HTTP estándar (NoContent, Unauthorized, Forbidden, Conflict)
using System.Net;
// Métodos de extensión para enviar y deserializar JSON directamente con HttpClient
using System.Net.Http.Json;
using System.Text;
// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Clases DTO usadas por los componentes de Blazor para mostrar y editar datos
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP para el recurso 'usuario'.
/// </summary>
public class ServicioUsuario
{
    // Cliente HTTP inyectado para comunicarse con la API REST
    private readonly HttpClient _http;
    // Estado de sesión Scoped que conserva el token JWT en memoria del circuito Blazor
    private readonly EstadoSesion _estadoSesion;


    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
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
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
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

    // Busca un registro específico por su correo electrónico
    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        var respuesta = await _http.GetAsync($"api/usuario/{Uri.EscapeDataString(email)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Usuario>();
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<ResultadoOperacion> CrearAsync(Usuario usuario)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    // Envía petición PUT a la API para reemplazar toda la ficha del registro
    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Usuario usuario)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    // Envía petición PATCH a la API únicamente con los campos editados
    public async Task<ResultadoOperacion> GuardarSoloLoCambiadoAsync(string email, Dictionary<string, object> camposModificados)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
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
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<ResultadoOperacion> EliminarAsync(string email)
    {
        // Versión 3: Inserta el token JWT Bearer en la cabecera Authorization antes de enviar la petición HTTP
        Autorizar();
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            var respuesta = await _http.DeleteAsync($"api/usuario/{Uri.EscapeDataString(email)}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario eliminado exitosamente." };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        // Captura falla de red o API apagada en el frontend para evitar caída y mostrar alerta
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
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

