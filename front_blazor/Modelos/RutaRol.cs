using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

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
