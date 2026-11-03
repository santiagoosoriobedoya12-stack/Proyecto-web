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
    [Required(ErrorMessage = "El campo credito es obligatorio en el reemplazo completo.")]
    [Range(0, 9999999999999999.99, ErrorMessage = "El crédito no puede ser negativo.")]
    public decimal? Credito { get; set; }

    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio en el reemplazo completo.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo fkcodpersona debe tener entre 1 y 10 caracteres.")]
    public string? Fkcodpersona { get; set; }

    [StringLength(10, ErrorMessage = "El campo fkcodempresa no puede exceder 10 caracteres.")]
    public string? Fkcodempresa { get; set; }
}

