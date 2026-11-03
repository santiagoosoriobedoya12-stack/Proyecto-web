using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para un renglón del detalle de factura (3.2 en 3_plan.md).
/// </summary>
public class ProductoDeFactura
{
    [JsonPropertyName("codigo_producto")]
    public string CodigoProducto { get; set; } = string.Empty;

    [JsonPropertyName("nombre_producto")]
    public string? NombreProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("valorunitario")]
    public decimal ValorUnitario { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }
}

