using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

// Modelo / DTO 'RutaRol': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class RutaRol
{
    [JsonPropertyName("fkidruta")]
    // Propiedad 'Fkidruta' (int) mapeada desde SQL Server o JSON
    public int Fkidruta { get; set; }

    [JsonPropertyName("ruta")]
    // Propiedad 'Ruta' (string?) mapeada desde SQL Server o JSON
    public string? Ruta { get; set; }

    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int) mapeada desde SQL Server o JSON
    public int Fkidrol { get; set; }

    [JsonPropertyName("rol")]
    // Propiedad 'Rol' (string?) mapeada desde SQL Server o JSON
    public string? Rol { get; set; }
}
