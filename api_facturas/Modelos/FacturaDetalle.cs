using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para un renglón o detalle de factura (productosporfactura).
/// 
/// Regla fundamental de la Versión 2:
/// - El 'subtotal' NO lo calcula el backend en C# ni el frontend.
/// - Es calculado automáticamente por el disparador (trigger) trg_prodfact_insert en SQL Server:
///   subtotal = cantidad * valorunitario.
/// </summary>
public class FacturaDetalle
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
