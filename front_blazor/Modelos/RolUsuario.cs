using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

// Modelo / DTO 'RolUsuario': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class RolUsuario
{
    [JsonPropertyName("fkemail")]
    // Propiedad 'Fkemail' (string) mapeada desde SQL Server o JSON
    public string Fkemail { get; set; } = string.Empty;

    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int) mapeada desde SQL Server o JSON
    public int Fkidrol { get; set; }

    [JsonPropertyName("nombre_rol")]
    // Propiedad 'NombreRol' (string?) mapeada desde SQL Server o JSON
    public string? NombreRol { get; set; }
}
