// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

// Modelo / DTO 'RolItemDto': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class RolItemDto
{
    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int) mapeada desde SQL Server o JSON
    public int Fkidrol { get; set; }
}

/// <summary>
/// DTO para la creación atómica de usuario con sus roles iniciales (POST /api/usuario-con-roles).
/// </summary>
public class UsuarioConRolesCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El email es obligatorio.")]
    // Verifica que la cadena tenga estructura válida de correo electrónico
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El email debe tener entre 3 y 100 caracteres.")]
    [JsonPropertyName("email")]
    // Propiedad 'Email' (string?) mapeada desde SQL Server o JSON
    public string? Email { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
    [JsonPropertyName("contrasena")]
    // Propiedad 'Contrasena' (string?) mapeada desde SQL Server o JSON
    public string? Contrasena { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo roles es obligatorio.")]
    [MinLength(1, ErrorMessage = "El usuario requiere al menos un rol asignado (mínimo 1).")]
    [JsonPropertyName("roles")]
    public List<int>? Roles { get; set; }
}
