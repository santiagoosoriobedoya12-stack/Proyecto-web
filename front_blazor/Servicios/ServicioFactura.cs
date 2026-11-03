using System.Net;
using System.Net.Http.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio HTTP cliente para 'factura' (/api/factura).
/// Consume los endpoints que ejecutan procedimientos almacenados en el backend.
/// </summary>
public class ServicioFactura
{
    private readonly HttpClient _http;
    private readonly EstadoSesion _estadoSesion;


    public ServicioFactura(HttpClient http, EstadoSesion estadoSesion)
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
    /// Consulta el listado completo de facturas.
    /// Resiliente ante desconexión o 204 No Content.
    /// </summary>
    public async Task<List<Factura>> ListarAsync()
    {
        Autorizar();
        try
        {
            var respuesta = await _http.GetAsync("api/factura");
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<Factura>();
            }

            respuesta.EnsureSuccessStatusCode();
            var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Factura>>();
            return sobre?.Datos ?? new List<Factura>();
        }
        catch (HttpRequestException)
        {
            // Resiliencia C18: si la API se apaga, la UI se mantiene en pie mostrando 0 filas
            return new List<Factura>();
        }
    }

    /// <summary>
    /// Consulta una factura en detalle por su número.
    /// </summary>
    public async Task<Factura?> ConsultarAsync(int numero)
    {
        Autorizar();
        var respuesta = await _http.GetAsync($"api/factura/{numero}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Factura>();
    }

    /// <summary>
    /// Emite una nueva factura (POST /api/factura).
    /// Envía en una sola transacción el encabezado y sus renglones.
    /// Criterio 14: NO envía total ni subtotal.
    /// </summary>
    public async Task<ResultadoOperacion> EmitirAsync(FacturaCrearDto dto)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/factura", dto);

            if (respuesta.IsSuccessStatusCode)
            {
                var facturaCreada = await respuesta.Content.ReadFromJsonAsync<Factura>();
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = $"Factura #{facturaCreada?.Numero} emitida exitosamente."
                };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion { Exitoso = false, Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}" };
        }
    }

    /// <summary>
    /// Anula una factura (POST /api/factura/{numero}/anular).
    /// Operación lógica que devuelve el stock de los productos.
    /// </summary>
    public async Task<ResultadoOperacion> AnularAsync(int numero)
    {
        Autorizar();
        try
        {
            var respuesta = await _http.PostAsync($"api/factura/{numero}/anular", null);

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = $"Factura #{numero} anulada exitosamente. El stock ha sido restablecido."
                };
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
