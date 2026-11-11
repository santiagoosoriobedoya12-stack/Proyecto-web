using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para Vendedor en Blazor Server.
/// </summary>
public class Vendedor
{
    [JsonPropertyName("id")]
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }

    [JsonPropertyName("carnet")]
    // Propiedad 'Carnet' (int) mapeada desde SQL Server o JSON
    public int Carnet { get; set; }

    [JsonPropertyName("direccion")]
    // Propiedad 'Direccion' (string) mapeada desde SQL Server o JSON
    public string Direccion { get; set; } = string.Empty;

    [JsonPropertyName("fkcodpersona")]
    // Propiedad 'Fkcodpersona' (string) mapeada desde SQL Server o JSON
    public string Fkcodpersona { get; set; } = string.Empty;
}

