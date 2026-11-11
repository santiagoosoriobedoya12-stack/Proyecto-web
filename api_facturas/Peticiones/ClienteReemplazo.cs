// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para el reemplazo completo de un cliente existente (PUT /api/cliente/{id}).
/// 
/// Características del verbo PUT:
/// - Es idempotente y exige todos los campos obligatorios del registro.
/// - Si 'fkcodempresa' no se proporciona o es nulo, se actualiza a NULL en la base de datos
///   (permitiendo cambiar un cliente empresarial a cliente persona natural).
/// </summary>
public class ClienteReemplazo
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo credito es obligatorio en el reemplazo completo.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(0, 9999999999999999.99, ErrorMessage = "El crédito no puede ser negativo.")]
    // Propiedad 'Credito' (decimal?) mapeada desde SQL Server o JSON
    public decimal? Credito { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio en el reemplazo completo.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo fkcodpersona debe tener entre 1 y 10 caracteres.")]
    // Propiedad 'Fkcodpersona' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodpersona { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, ErrorMessage = "El campo fkcodempresa no puede exceder 10 caracteres.")]
    // Propiedad 'Fkcodempresa' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodempresa { get; set; }
}

