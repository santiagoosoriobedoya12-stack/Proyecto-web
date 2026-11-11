// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PUT /api/rol/{id}.
/// </summary>
public class RolReemplazo
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni superar 50 caracteres.")]
    // Propiedad 'Nombre' (string?) mapeada desde SQL Server o JSON
    public string? Nombre { get; set; }
}

