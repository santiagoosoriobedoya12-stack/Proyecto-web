using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/ruta/{id}.
/// </summary>
public class RutaActualizar
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo ruta no puede exceder 100 caracteres.")]
    [JsonPropertyName("ruta")]
    public string? Ruta { get; set; }

    [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo descripcion no puede exceder 200 caracteres.")]
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }

    public bool TieneCamposParaActualizar()
    {
        return Ruta != null || Descripcion != null;
    }
}

