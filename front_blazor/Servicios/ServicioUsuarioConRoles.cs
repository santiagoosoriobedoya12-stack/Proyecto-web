using System.Net;
using System.Net.Http.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP cliente para el recurso compuesto 'api/usuario-con-roles' (RF5).
/// Administra el usuario y sus roles asignados como una única entidad lógica.
/// </summary>
public class ServicioUsuarioConRoles
{
    private readonly HttpClient _http;

    public ServicioUsuarioConRoles(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>
    /// Consulta la lista de usuarios junto con los roles que tienen asociados.
    /// Si la API no está disponible o responde 204, retorna una lista vacía para no romper la UI.
    /// </summary>
    public async Task<List<UsuarioConRoles>> ListarAsync()
    {
        try
        {
            var respuesta = await _http.GetAsync("api/usuario-con-roles");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
                return new List<UsuarioConRoles>();

            respuesta.EnsureSuccessStatusCode();

            // Deserializa la envoltura { recurso: "usuario-con-roles", total: N, datos: [...] }
            var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<UsuarioConRoles>>();
            return sobre?.Datos ?? new List<UsuarioConRoles>();
        }
        catch
        {
            return new List<UsuarioConRoles>();
        }
    }

    /// <summary>
    /// Consulta un usuario y sus roles por correo electrónico.
    /// </summary>
    public async Task<UsuarioConRoles?> ConsultarAsync(string email)
    {
        var respuesta = await _http.GetAsync($"api/usuario-con-roles/{email}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<UsuarioConRoles>();
    }

    /// <summary>
    /// Crea un usuario con sus roles en una única operación atómica (POST /api/usuario-con-roles).
    /// </summary>
    public async Task<ResultadoOperacion> CrearAsync(string email, string contrasena, List<int> roles)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/usuario-con-roles", new
            {
                email,
                contrasena,
                roles
            });

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario con roles creado exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    /// <summary>
    /// Actualiza el usuario y sus roles en una sola llamada (PUT /api/usuario-con-roles/{email}).
    /// 
    /// REGLA DE NEGOCIO (Criterios 9 y 17):
    /// - Si 'contrasena' viene nula o vacía, la base de datos conserva la clave actual.
    /// - La lista de roles reemplaza los roles previos en su totalidad.
    /// </summary>
    public async Task<ResultadoOperacion> ActualizarAsync(string email, string? contrasena, List<int> roles)
    {
        try
        {
            var respuesta = await _http.PutAsJsonAsync($"api/usuario-con-roles/{email}", new
            {
                contrasena = string.IsNullOrWhiteSpace(contrasena) ? null : contrasena,
                roles
            });

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario y roles actualizados exitosamente." };

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"Error de comunicación: {ex.Message}" };
        }
    }

    /// <summary>
    /// Elimina el usuario y todas sus asociaciones de roles en una sola operación.
    /// </summary>
    public async Task<ResultadoOperacion> EliminarAsync(string email)
    {
        try
        {
            var respuesta = await _http.DeleteAsync($"api/usuario-con-roles/{email}");
            if (respuesta.IsSuccessStatusCode)
                return new ResultadoOperacion { Exitoso = true, Mensaje = "Usuario eliminado exitosamente." };

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

