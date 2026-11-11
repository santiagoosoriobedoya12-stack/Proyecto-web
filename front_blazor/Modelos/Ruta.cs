using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Ruta.
/// </summary>
public class Ruta
{
    [JsonPropertyName("id")]
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }

    [JsonPropertyName("ruta")]
    // Propiedad 'NombreRuta' (string) mapeada desde SQL Server o JSON
    public string NombreRuta { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    // Propiedad 'Descripcion' (string) mapeada desde SQL Server o JSON
    public string Descripcion { get; set; } = string.Empty;
}

