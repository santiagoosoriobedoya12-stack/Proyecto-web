using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo para deserializar la envoltura de respuesta de listado de la API:
/// { "tabla": "producto", "limite": 1000, "total": 8, "datos": [...] }
/// </summary>
/// <typeparam name="T">Tipo de dato de los elementos de la lista.</typeparam>
public class RespuestaListado<T>
{
    [JsonPropertyName("tabla")]
    public string? Tabla { get; set; }

    [JsonPropertyName("limite")]
    public int Limite { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("datos")]
    public List<T> Datos { get; set; } = new();
}

/// <summary>
/// Modelo para capturar y deserializar los mensajes de error devueltos por la API (400, 404, 422, 500).
/// </summary>
public class ErrorApi
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }

    [JsonPropertyName("detalle")]
    public string? Detalle { get; set; }

    [JsonPropertyName("errores")]
    public List<string>? Errores { get; set; }
}

/// <summary>
/// Resultado genérico de una operación hacia la API para que la interfaz gráfica
/// muestre notificaciones al usuario sin manejar códigos HTTP crudos.
/// </summary>
public class ResultadoOperacion
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<string>? ErroresValidacion { get; set; }
}

