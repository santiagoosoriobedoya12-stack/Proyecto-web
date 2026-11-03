using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para Vendedor en Blazor Server.
/// </summary>
public class Vendedor
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("carnet")]
    public int Carnet { get; set; }

    [JsonPropertyName("direccion")]
    public string Direccion { get; set; } = string.Empty;

    [JsonPropertyName("fkcodpersona")]
    public string Fkcodpersona { get; set; } = string.Empty;
}

