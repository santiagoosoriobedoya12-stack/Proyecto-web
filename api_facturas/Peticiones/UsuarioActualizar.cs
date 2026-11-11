// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/usuario/{email}.
/// </summary>
public class UsuarioActualizar
{
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    // Propiedad 'Contrasena' (string?) mapeada desde SQL Server o JSON
    public string? Contrasena { get; set; }

    // Verifica que al menos un campo opcional haya sido enviado en el PATCH
    public bool TieneCamposParaActualizar()
    {
        return Contrasena != null;
    }
}

