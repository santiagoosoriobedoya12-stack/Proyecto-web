using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para la factura en el cliente Blazor.
/// </summary>
public class Factura
{
    [JsonPropertyName("numero")]
    // Propiedad 'Numero' (int) mapeada desde SQL Server o JSON
    public int Numero { get; set; }

    [JsonPropertyName("fecha")]
    // Propiedad 'Fecha' (DateTime) mapeada desde SQL Server o JSON
    public DateTime Fecha { get; set; }

    [JsonPropertyName("total")]
    // Propiedad 'Total' (decimal) mapeada desde SQL Server o JSON
    public decimal Total { get; set; }

    [JsonPropertyName("estado")]
    // Propiedad 'Estado' (string) mapeada desde SQL Server o JSON
    public string Estado { get; set; } = "activa";

    [JsonPropertyName("fkidcliente")]
    // Propiedad 'Fkidcliente' (int) mapeada desde SQL Server o JSON
    public int Fkidcliente { get; set; }

    [JsonPropertyName("nombre_cliente")]
    // Propiedad 'NombreCliente' (string?) mapeada desde SQL Server o JSON
    public string? NombreCliente { get; set; }

    [JsonPropertyName("fkidvendedor")]
    // Propiedad 'Fkidvendedor' (int) mapeada desde SQL Server o JSON
    public int Fkidvendedor { get; set; }

    [JsonPropertyName("nombre_vendedor")]
    // Propiedad 'NombreVendedor' (string?) mapeada desde SQL Server o JSON
    public string? NombreVendedor { get; set; }

    [JsonPropertyName("productos")]
    public List<FacturaDetalle> Productos { get; set; } = new();
}
