using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para la Factura (Encabezado maestro con su detalle anidado).
/// 
/// Características arquitectónicas:
/// 1. Clave primaria 'numero': Autogenerada con IDENTITY(1,1).
/// 2. Claves foráneas: 'fkidcliente' y 'fkidvendedor'.
/// 3. 'total': Calculado exclusivamente por el disparador (trg_prodfact_insert).
/// 4. 'estado': Inicia en 'activa' y puede pasar a 'anulada' mediante sp_anular_factura.
/// 5. 'productos': Lista de renglones anidados resueltos directamente por el procedimiento almacenado.
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
