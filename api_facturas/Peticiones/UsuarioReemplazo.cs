// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/usuario/{email}.
/// Reemplaza la contraseña del usuario.
/// </summary>
public class UsuarioReemplazo
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo contrasena es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    // Propiedad 'Contrasena' (string?) mapeada desde SQL Server o JSON
    public string? Contrasena { get; set; }
}

