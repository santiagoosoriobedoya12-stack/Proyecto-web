using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/persona/{codigo}.
/// Actualización parcial: campos opcionales.
/// </summary>
public class PersonaActualizar
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío.")]
    public string? Nombre { get; set; }

    [EmailAddress(ErrorMessage = "El formato de correo electrónico es inválido.")]
    [StringLength(100, ErrorMessage = "El campo email no puede exceder 100 caracteres.")]
    public string? Email { get; set; }

    [StringLength(20, MinimumLength = 1, ErrorMessage = "El campo telefono no puede estar vacío.")]
    public string? Telefono { get; set; }

    public bool TieneCamposParaActualizar()
    {
        return Nombre != null || Email != null || Telefono != null;
    }
}

