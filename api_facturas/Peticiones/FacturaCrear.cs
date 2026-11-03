using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para la creación de una factura (POST /api/factura).
/// 
/// Características:
/// 1. Clientes y Vendedores identificados por sus IDs numéricos (FK).
/// 2. Colección de productos: mínimo 1 elemento (si llega vacía [] se responde 422).
/// 3. Criterio 14: NO contiene 'total' ni 'subtotales'. La base de datos es la única
///    responsable de calcularlos mediante disparadores transaccionales.
/// </summary>
public class FacturaCrear
{
    [Required(ErrorMessage = "El campo fkidcliente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del cliente debe ser un entero positivo.")]
    [JsonPropertyName("fkidcliente")]
    public int? Fkidcliente { get; set; }

    [Required(ErrorMessage = "El campo fkidvendedor es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del vendedor debe ser un entero positivo.")]
    [JsonPropertyName("fkidvendedor")]
    public int? Fkidvendedor { get; set; }

    [Required(ErrorMessage = "El listado de productos es obligatorio.")]
    [MinLength(1, ErrorMessage = "La factura requiere mínimo 1 producto.")]
    [JsonPropertyName("productos")]
    public List<ItemFacturaCrear>? Productos { get; set; }
}
