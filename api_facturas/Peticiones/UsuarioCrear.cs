using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para POST /api/usuario.
/// Requiere email y contraseña.
/// </summary>
public class UsuarioCrear
{
    [Required(ErrorMessage = "El campo email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico es inválido.")]
    [StringLength(100, ErrorMessage = "El campo email no puede exceder 100 caracteres.")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El campo contrasena es obligatorio.")]
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    public string? Contrasena { get; set; }
}

