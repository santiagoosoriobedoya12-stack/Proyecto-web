// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

// Modelo / DTO 'RutaRolCrear': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class RutaRolCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El id de la ruta es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El id de la ruta debe ser un entero positivo.")]
    [JsonPropertyName("fkidruta")]
    // Propiedad 'Fkidruta' (int?) mapeada desde SQL Server o JSON
    public int? Fkidruta { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El id del rol es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El id del rol debe ser un entero positivo.")]
    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int?) mapeada desde SQL Server o JSON
    public int? Fkidrol { get; set; }
}
