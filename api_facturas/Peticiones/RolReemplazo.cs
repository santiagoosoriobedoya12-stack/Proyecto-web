using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/rol/{id}.
/// </summary>
public class RolReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni superar 50 caracteres.")]
    public string? Nombre { get; set; }
}

