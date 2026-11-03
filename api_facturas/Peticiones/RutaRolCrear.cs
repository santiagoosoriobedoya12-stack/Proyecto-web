using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiFacturas.Peticiones;

public class RutaRolCrear
{
    [Required(ErrorMessage = "El id de la ruta es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id de la ruta debe ser un entero positivo.")]
    [JsonPropertyName("fkidruta")]
    public int? Fkidruta { get; set; }

    [Required(ErrorMessage = "El id del rol es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id del rol debe ser un entero positivo.")]
    [JsonPropertyName("fkidrol")]
    public int? Fkidrol { get; set; }
}
