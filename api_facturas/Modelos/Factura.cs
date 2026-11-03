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
