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
    // Propiedad 'Tabla' (string?) mapeada desde SQL Server o JSON
    public string? Tabla { get; set; }

    [JsonPropertyName("limite")]
    // Propiedad 'Limite' (int) mapeada desde SQL Server o JSON
    public int Limite { get; set; }

    [JsonPropertyName("total")]
    // Propiedad 'Total' (int) mapeada desde SQL Server o JSON
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
    // Propiedad 'Estado' (int) mapeada desde SQL Server o JSON
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    // Propiedad 'Mensaje' (string?) mapeada desde SQL Server o JSON
    public string? Mensaje { get; set; }

    [JsonPropertyName("detalle")]
    // Propiedad 'Detalle' (string?) mapeada desde SQL Server o JSON
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
    // Propiedad 'Exitoso' (bool) mapeada desde SQL Server o JSON
    public bool Exitoso { get; set; }
    // Propiedad 'Mensaje' (string) mapeada desde SQL Server o JSON
    public string Mensaje { get; set; } = string.Empty;
    public List<string>? ErroresValidacion { get; set; }
}

