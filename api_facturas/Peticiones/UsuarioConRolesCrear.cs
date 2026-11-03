using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

public class RolItemDto
{
    [JsonPropertyName("fkidrol")]
    public int Fkidrol { get; set; }
}

/// <summary>
/// DTO para la creación atómica de usuario con sus roles iniciales (POST /api/usuario-con-roles).
/// </summary>
public class UsuarioConRolesCrear
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El email debe tener entre 3 y 100 caracteres.")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
    [JsonPropertyName("contrasena")]
    public string? Contrasena { get; set; }

    [Required(ErrorMessage = "El campo roles es obligatorio.")]
    [MinLength(1, ErrorMessage = "El usuario requiere al menos un rol asignado (mínimo 1).")]
    [JsonPropertyName("roles")]
    public List<int>? Roles { get; set; }
}
