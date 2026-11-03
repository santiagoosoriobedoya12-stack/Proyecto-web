using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para POST /api/rol.
/// El 'id' NO se envía en la creación porque la base de datos lo genera con IDENTITY(1,1).
/// </summary>
public class RolCrear
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni superar 50 caracteres.")]
    public string? Nombre { get; set; }
}

