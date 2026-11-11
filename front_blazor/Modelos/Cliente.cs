using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para Cliente en el cliente Blazor Server.
/// Desacoplado del backend (no referencia ApiFacturas ni paquetes SQL).
/// </summary>
public class Cliente
{
    [JsonPropertyName("id")]
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }

    [JsonPropertyName("credito")]
    // Propiedad 'Credito' (decimal) mapeada desde SQL Server o JSON
    public decimal Credito { get; set; }

    [JsonPropertyName("fkcodpersona")]
    // Propiedad 'Fkcodpersona' (string) mapeada desde SQL Server o JSON
    public string Fkcodpersona { get; set; } = string.Empty;

    [JsonPropertyName("fkcodempresa")]
    // Propiedad 'Fkcodempresa' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodempresa { get; set; }
}

