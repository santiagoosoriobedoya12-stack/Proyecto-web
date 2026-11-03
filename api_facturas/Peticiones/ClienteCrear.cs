using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para la creación de un nuevo cliente (POST /api/cliente).
/// 
/// Reglas de validación:
/// 1. El 'id' NO se incluye: la base de datos lo genera automáticamente con IDENTITY(1,1).
/// 2. 'credito': Obligatorio y mayor o igual a 0.
/// 3. 'fkcodpersona': Obligatorio (máximo 10 caracteres). Debe coincidir con un código de 'persona'.
/// 4. 'fkcodempresa': Opcional (máximo 10 caracteres). Si se envía, debe coincidir con un código de 'empresa'.
/// </summary>
public class ClienteCrear
{
    [Required(ErrorMessage = "El campo credito es obligatorio.")]
    [Range(0, 9999999999999999.99, ErrorMessage = "El crédito no puede ser negativo.")]
    public decimal? Credito { get; set; }

    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo fkcodpersona debe tener entre 1 y 10 caracteres.")]
    public string? Fkcodpersona { get; set; }

    [StringLength(10, ErrorMessage = "El campo fkcodempresa no puede exceder 10 caracteres.")]
    public string? Fkcodempresa { get; set; }
}

