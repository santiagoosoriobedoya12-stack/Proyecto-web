using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para POST /api/ruta.
/// El 'id' lo genera la base de datos con IDENTITY.
/// </summary>
public class RutaCrear
{
    [Required(ErrorMessage = "El campo ruta es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo ruta no puede exceder 100 caracteres.")]
    [JsonPropertyName("ruta")]
    public string? Ruta { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo descripcion no puede exceder 200 caracteres.")]
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}

