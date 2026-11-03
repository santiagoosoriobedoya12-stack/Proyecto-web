using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Ruta.
/// </summary>
public class Ruta
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ruta")]
    public string NombreRuta { get; set; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;
}

