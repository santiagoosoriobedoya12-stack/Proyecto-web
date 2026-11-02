using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/persona/{codigo}.
/// Reemplazo completo (sin código, que viaja en la URL).
/// </summary>
public class PersonaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico es inválido.")]
    [StringLength(100, ErrorMessage = "El campo email no puede exceder 100 caracteres.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El campo telefono es obligatorio.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El campo telefono no puede estar vacío.")]
    public string? Telefono { get; set; }
}
