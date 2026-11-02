using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio de comunicación HTTP para el recurso 'producto'.
/// REGLAS DE ARQUITECTURA:
/// 1. Es el ÚNICO sitio del frontend que conoce los endpoints y verbos HTTP de la API.
/// 2. Un servicio POR RECURSO (no un servicio genérico con la tabla como parámetro).
/// 3. Traduce respuestas HTTP y códigos de error a mensajes entendibles para los componentes visuales.
/// </summary>
public class ServicioProducto
{
    private readonly HttpClient _http;

    public ServicioProducto(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>
    /// Consulta el listado de productos llamando a GET /api/producto?limite={limite}.
    /// Si la API responde 204 (sin contenido), retorna una lista vacía.
    /// Si la API está apagada, la excepción HttpRequestException sube al componente para activar el aviso del Criterio 10.
    /// </summary>
    public async Task<List<Producto>> ListarAsync(int limite = 1000)
    {
        var respuesta = await _http.GetAsync($"api/producto?limite={limite}");

        // Criterio C1: Si la tabla está vacía, la API responde 204 No Content
        if (respuesta.StatusCode == HttpStatusCode.NoContent)
        {
            return new List<Producto>();
        }

        respuesta.EnsureSuccessStatusCode();

        var sobre = await respuesta.Content.ReadFromJsonAsync<RespuestaListado<Producto>>();
        return sobre?.Datos ?? new List<Producto>();
    }

    /// <summary>
    /// Obtiene un producto por su clave llamando a GET /api/producto/{codigo}.
    /// </summary>
    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        var respuesta = await _http.GetAsync($"api/producto/{Uri.EscapeDataString(codigo)}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Producto>();
    }

    /// <summary>
    /// Crea un producto llamando a POST /api/producto.
    /// Criterio 8: Si la API rechaza la creación (422 datos inválidos o 500 código duplicado),
    /// captura los errores para mostrarlos en la pantalla sin limpiar los campos del usuario.
    /// </summary>
    public async Task<ResultadoOperacion> CrearAsync(Producto producto)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/producto", new
            {
                codigo = producto.Codigo,
                nombre = producto.Nombre,
                stock = producto.Stock,
                valorunitario = producto.Valorunitario
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = "Producto creado exitosamente."
                };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Criterio 9 - Botón 1: «Guardar la ficha completa» (el PUT).
    /// Envía todos los campos del producto a PUT /api/producto/{codigo}.
    /// Si falta un campo o está incompleto, la API responderá con 422.
    /// </summary>
    public async Task<ResultadoOperacion> GuardarFichaCompletaAsync(Producto producto)
    {
        try
        {
            var cuerpo = new
            {
                nombre = producto.Nombre,
                stock = producto.Stock,
                valorunitario = producto.Valorunitario
            };

            var respuesta = await _http.PutAsJsonAsync($"api/producto/{Uri.EscapeDataString(producto.Codigo)}", cuerpo);

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = "Ficha completa guardada exitosamente."
                };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Criterio 9 - Botón 2: «Guardar solo lo que cambié» (el PATCH).
    /// Envía únicamente los campos modificados a PATCH /api/producto/{codigo}.
    /// </summary>
    public async Task<ResultadoOperacion> GuardarSoloLoCambiadoAsync(string codigo, Dictionary<string, object> camposModificados)
    {
        try
        {
            var json = JsonSerializer.Serialize(camposModificados);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            var respuesta = await _http.PatchAsync($"api/producto/{Uri.EscapeDataString(codigo)}", contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = "Cambios guardados exitosamente."
                };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina un producto llamando a DELETE /api/producto/{codigo}.
    /// </summary>
    public async Task<ResultadoOperacion> EliminarAsync(string codigo)
    {
        try
        {
            var respuesta = await _http.DeleteAsync($"api/producto/{Uri.EscapeDataString(codigo)}");

            if (respuesta.IsSuccessStatusCode)
            {
                return new ResultadoOperacion
                {
                    Exitoso = true,
                    Mensaje = "Producto eliminado exitosamente."
                };
            }

            return await ExtraerErrorAsync(respuesta);
        }
        catch (HttpRequestException ex)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                Mensaje = $"No se pudo comunicar con el servidor: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Parsea los cuerpos de error devueltos por la API (400, 404, 422, 500)
    /// extrayendo la lista de errores y mensajes descriptivos.
    /// </summary>
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
        catch
        {
            // Si el cuerpo no era JSON estándar, retornamos el status code
        }

        return new ResultadoOperacion
        {
            Exitoso = false,
            Mensaje = $"Error del servidor: HTTP {(int)respuesta.StatusCode} ({respuesta.ReasonPhrase})"
        };
    }
}

