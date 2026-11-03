using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para Cliente en el cliente Blazor Server.
/// Desacoplado del backend (no referencia ApiFacturas ni paquetes SQL).
/// </summary>
public class Cliente
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("credito")]
    public decimal Credito { get; set; }

    [JsonPropertyName("fkcodpersona")]
    public string Fkcodpersona { get; set; } = string.Empty;

    [JsonPropertyName("fkcodempresa")]
    public string? Fkcodempresa { get; set; }
}

