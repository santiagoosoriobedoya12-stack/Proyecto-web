using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

public class RolUsuario
{
    [JsonPropertyName("fkemail")]
    public string Fkemail { get; set; } = string.Empty;

    [JsonPropertyName("fkidrol")]
    public int Fkidrol { get; set; }

    [JsonPropertyName("nombre_rol")]
    public string? NombreRol { get; set; }
}
