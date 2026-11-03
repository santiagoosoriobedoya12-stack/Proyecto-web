using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de datos para la emisión de una nueva factura desde la interfaz gráfica.
/// 
/// Criterio 14:
/// - NO envía 'total' ni 'subtotal'.
/// - Solo envía las claves foráneas del cliente, vendedor y el arreglo de productos.
/// </summary>
public class FacturaCrearDto
{
    [JsonPropertyName("fkidcliente")]
    public int Fkidcliente { get; set; }

    [JsonPropertyName("fkidvendedor")]
    public int Fkidvendedor { get; set; }

    [JsonPropertyName("productos")]
    public List<ItemFacturaCrearDto> Productos { get; set; } = new();
}

public class ItemFacturaCrearDto
{
    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }
}
