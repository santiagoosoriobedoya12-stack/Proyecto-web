using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/usuario/{email}.
/// Reemplaza la contraseña del usuario.
/// </summary>
public class UsuarioReemplazo
{
    [Required(ErrorMessage = "El campo contrasena es obligatorio.")]
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    public string? Contrasena { get; set; }
}

