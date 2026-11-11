// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para agregar una pareja (usuario, rol) en la tabla puente rol_usuario.
/// </summary>
public class RolUsuarioCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El email del usuario es obligatorio.")]
    // Verifica que la cadena tenga estructura válida de correo electrónico
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [JsonPropertyName("fkemail")]
    // Propiedad 'Fkemail' (string?) mapeada desde SQL Server o JSON
    public string? Fkemail { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El id del rol es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El id del rol debe ser un entero positivo.")]
    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int?) mapeada desde SQL Server o JSON
    public int? Fkidrol { get; set; }
}
