// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/persona/{codigo}.
/// Reemplazo completo (sin código, que viaja en la URL).
/// </summary>
public class PersonaReemplazo
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío.")]
    // Propiedad 'Nombre' (string?) mapeada desde SQL Server o JSON
    public string? Nombre { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo email es obligatorio.")]
    // Verifica que la cadena tenga estructura válida de correo electrónico
    [EmailAddress(ErrorMessage = "El formato de correo electrónico es inválido.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, ErrorMessage = "El campo email no puede exceder 100 caracteres.")]
    // Propiedad 'Email' (string?) mapeada desde SQL Server o JSON
    public string? Email { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo telefono es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El campo telefono no puede estar vacío.")]
    // Propiedad 'Telefono' (string?) mapeada desde SQL Server o JSON
    public string? Telefono { get; set; }
}

