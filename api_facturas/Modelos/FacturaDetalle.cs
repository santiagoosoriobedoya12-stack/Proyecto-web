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
