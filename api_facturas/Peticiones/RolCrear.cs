// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para POST /api/rol.
/// El 'id' NO se envía en la creación porque la base de datos lo genera con IDENTITY(1,1).
/// </summary>
public class RolCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni superar 50 caracteres.")]
    // Propiedad 'Nombre' (string?) mapeada desde SQL Server o JSON
    public string? Nombre { get; set; }
}

