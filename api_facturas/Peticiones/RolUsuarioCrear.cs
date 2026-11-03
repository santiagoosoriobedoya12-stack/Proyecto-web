using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para agregar una pareja (usuario, rol) en la tabla puente rol_usuario.
/// </summary>
public class RolUsuarioCrear
{
    [Required(ErrorMessage = "El email del usuario es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [JsonPropertyName("fkemail")]
    public string? Fkemail { get; set; }

    [Required(ErrorMessage = "El id del rol es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id del rol debe ser un entero positivo.")]
    [JsonPropertyName("fkidrol")]
    public int? Fkidrol { get; set; }
}
