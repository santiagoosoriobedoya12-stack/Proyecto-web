// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/ruta/{id}.
/// </summary>
public class RutaActualizar
{
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo ruta no puede exceder 100 caracteres.")]
    [JsonPropertyName("ruta")]
    // Propiedad 'Ruta' (string?) mapeada desde SQL Server o JSON
    public string? Ruta { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo descripcion no puede exceder 200 caracteres.")]
    [JsonPropertyName("descripcion")]
    // Propiedad 'Descripcion' (string?) mapeada desde SQL Server o JSON
    public string? Descripcion { get; set; }

    // Verifica que al menos un campo opcional haya sido enviado en el PATCH
    public bool TieneCamposParaActualizar()
    {
        return Ruta != null || Descripcion != null;
    }
}

