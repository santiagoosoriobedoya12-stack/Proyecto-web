// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
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
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El código del producto es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 10 caracteres.")]
    [JsonPropertyName("codigo")]
    // Propiedad 'Codigo' (string) mapeada desde SQL Server o JSON
    public string Codigo { get; set; } = string.Empty;

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "La cantidad del producto es obligatoria.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número entero mayor a cero.")]
    [JsonPropertyName("cantidad")]
    // Propiedad 'Cantidad' (int) mapeada desde SQL Server o JSON
    public int Cantidad { get; set; }
}
