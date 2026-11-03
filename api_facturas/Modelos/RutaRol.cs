using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para la tabla puente 'rutarol'.
/// Representa qué rol tiene acceso a qué ruta o pantalla del sistema.
/// </summary>
public class RutaRol
{
    [JsonPropertyName("fkidruta")]
    public int Fkidruta { get; set; }

    [JsonPropertyName("ruta")]
    public string? Ruta { get; set; }

    [JsonPropertyName("fkidrol")]
    public int Fkidrol { get; set; }

    [JsonPropertyName("rol")]
    public string? Rol { get; set; }
}
