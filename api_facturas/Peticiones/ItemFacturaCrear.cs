using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para un renglón de producto en la petición de emisión de factura.
/// 
/// Criterio de Aceptación 14:
/// - El JSON de emisión de factura NO lleva 'total' ni 'subtotal'.
/// - Únicamente se envía el 'codigo' del producto y la 'cantidad' deseada.
/// </summary>
public class ItemFacturaCrear
{
    [Required(ErrorMessage = "El código del producto es obligatorio.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 10 caracteres.")]
    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cantidad del producto es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número entero mayor a cero.")]
    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }
}
