using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para POST /api/persona.
/// </summary>
public class PersonaCrear
{
    [Required(ErrorMessage = "El campo codigo es obligatorio.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo codigo debe tener entre 1 y 10 caracteres.")]
    public string? Codigo { get; set; }

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

