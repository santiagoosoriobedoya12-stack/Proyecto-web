using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para el reemplazo completo de un vendedor (PUT /api/vendedor/{id}).
/// </summary>
public class VendedorReemplazo
{
    [Required(ErrorMessage = "El campo carnet es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El carnet debe ser un número entero positivo mayor a cero.")]
    public int? Carnet { get; set; }

    [Required(ErrorMessage = "El campo direccion es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "La dirección debe tener entre 1 y 100 caracteres.")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código de persona debe tener entre 1 y 10 caracteres.")]
    public string? Fkcodpersona { get; set; }
}

