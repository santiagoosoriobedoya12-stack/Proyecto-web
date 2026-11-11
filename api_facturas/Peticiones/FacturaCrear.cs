// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
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
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo fkidcliente es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del cliente debe ser un entero positivo.")]
    [JsonPropertyName("fkidcliente")]
    // Propiedad 'Fkidcliente' (int?) mapeada desde SQL Server o JSON
    public int? Fkidcliente { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo fkidvendedor es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del vendedor debe ser un entero positivo.")]
    [JsonPropertyName("fkidvendedor")]
    // Propiedad 'Fkidvendedor' (int?) mapeada desde SQL Server o JSON
    public int? Fkidvendedor { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El listado de productos es obligatorio.")]
    [MinLength(1, ErrorMessage = "La factura requiere mínimo 1 producto.")]
    [JsonPropertyName("productos")]
    public List<ItemFacturaCrear>? Productos { get; set; }
}
