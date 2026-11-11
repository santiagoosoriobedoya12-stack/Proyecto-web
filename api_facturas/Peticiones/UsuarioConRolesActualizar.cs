// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para la actualización atómica de usuario con roles (PUT /api/usuario-con-roles/{email}).
/// 
/// REGLAS CRÍTICAS (Criterios 9 y 17):
/// 1. Si 'contrasena' viene nula o vacía "", la base de datos conserva la clave anterior sin modificarla.
/// 2. La lista de 'roles' REEMPLAZA íntegramente los roles anteriores del usuario.
/// </summary>
public class UsuarioConRolesActualizar
{
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, ErrorMessage = "La contraseña no puede superar 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    // Propiedad 'Contrasena' (string?) mapeada desde SQL Server o JSON
    public string? Contrasena { get; set; }

    [JsonPropertyName("roles")]
    public List<int> Roles { get; set; } = new();
}
