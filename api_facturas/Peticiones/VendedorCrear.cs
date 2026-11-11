// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para crear un vendedor (POST /api/vendedor).
/// 'id' no se incluye porque lo autogenera la base de datos (IDENTITY).
/// </summary>
public class VendedorCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo carnet es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El carnet debe ser un número entero positivo mayor a cero.")]
    // Propiedad 'Carnet' (int?) mapeada desde SQL Server o JSON
    public int? Carnet { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo direccion es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "La dirección debe tener entre 1 y 100 caracteres.")]
    // Propiedad 'Direccion' (string?) mapeada desde SQL Server o JSON
    public string? Direccion { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código de persona debe tener entre 1 y 10 caracteres.")]
    // Propiedad 'Fkcodpersona' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodpersona { get; set; }
}

