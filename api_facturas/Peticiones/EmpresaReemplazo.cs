using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/empresa/{codigo}.
/// Reemplazo completo: el nombre es obligatorio.
/// </summary>
public class EmpresaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    public string? Nombre { get; set; }
}
