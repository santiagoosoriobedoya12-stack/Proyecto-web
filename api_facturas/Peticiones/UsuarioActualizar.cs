using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/usuario/{email}.
/// </summary>
public class UsuarioActualizar
{
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 200 caracteres.")]
    [JsonPropertyName("contrasena")]
    public string? Contrasena { get; set; }

    public bool TieneCamposParaActualizar()
    {
        return Contrasena != null;
    }
}

