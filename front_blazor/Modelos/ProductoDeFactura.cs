using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para un renglón del detalle de factura en Blazor (3.2 en 3_plan.md).
/// </summary>
public class ProductoDeFactura
{
    [JsonPropertyName("codigo_producto")]
    // Propiedad 'CodigoProducto' (string) mapeada desde SQL Server o JSON
    public string CodigoProducto { get; set; } = string.Empty;

    [JsonPropertyName("nombre_producto")]
    // Propiedad 'NombreProducto' (string?) mapeada desde SQL Server o JSON
    public string? NombreProducto { get; set; }

    [JsonPropertyName("cantidad")]
    // Propiedad 'Cantidad' (int) mapeada desde SQL Server o JSON
    public int Cantidad { get; set; }

    [JsonPropertyName("valorunitario")]
    // Propiedad 'ValorUnitario' (decimal) mapeada desde SQL Server o JSON
    public decimal ValorUnitario { get; set; }

    [JsonPropertyName("subtotal")]
    // Propiedad 'Subtotal' (decimal) mapeada desde SQL Server o JSON
    public decimal Subtotal { get; set; }
}

