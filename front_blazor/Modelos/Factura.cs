using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para la factura en el cliente Blazor.
/// </summary>
public class Factura
{
    [JsonPropertyName("numero")]
    public int Numero { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "activa";

    [JsonPropertyName("fkidcliente")]
    public int Fkidcliente { get; set; }

    [JsonPropertyName("nombre_cliente")]
    public string? NombreCliente { get; set; }

    [JsonPropertyName("fkidvendedor")]
    public int Fkidvendedor { get; set; }

    [JsonPropertyName("nombre_vendedor")]
    public string? NombreVendedor { get; set; }

    [JsonPropertyName("productos")]
    public List<FacturaDetalle> Productos { get; set; } = new();
}
