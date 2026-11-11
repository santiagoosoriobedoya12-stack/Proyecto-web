// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/ruta/{id}.
/// </summary>
public class RutaReemplazo
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo ruta es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo ruta no puede exceder 100 caracteres.")]
    [JsonPropertyName("ruta")]
    // Propiedad 'Ruta' (string?) mapeada desde SQL Server o JSON
    public string? Ruta { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo descripcion no puede exceder 200 caracteres.")]
    [JsonPropertyName("descripcion")]
    // Propiedad 'Descripcion' (string?) mapeada desde SQL Server o JSON
    public string? Descripcion { get; set; }
}

