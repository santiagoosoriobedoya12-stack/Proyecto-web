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
    // Propiedad 'Fkidcliente' (int) mapeada desde SQL Server o JSON
    public int Fkidcliente { get; set; }

    [JsonPropertyName("fkidvendedor")]
    // Propiedad 'Fkidvendedor' (int) mapeada desde SQL Server o JSON
    public int Fkidvendedor { get; set; }

    [JsonPropertyName("productos")]
    public List<ItemFacturaCrearDto> Productos { get; set; } = new();
}

// Modelo / DTO 'ItemFacturaCrearDto': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class ItemFacturaCrearDto
{
    [JsonPropertyName("codigo")]
    // Propiedad 'Codigo' (string) mapeada desde SQL Server o JSON
    public string Codigo { get; set; } = string.Empty;

    [JsonPropertyName("cantidad")]
    // Propiedad 'Cantidad' (int) mapeada desde SQL Server o JSON
    public int Cantidad { get; set; }
}
