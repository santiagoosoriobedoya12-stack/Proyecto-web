// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para la creación de un nuevo cliente (POST /api/cliente).
/// 
/// Reglas de validación:
/// 1. El 'id' NO se incluye: la base de datos lo genera automáticamente con IDENTITY(1,1).
/// 2. 'credito': Obligatorio y mayor o igual a 0.
/// 3. 'fkcodpersona': Obligatorio (máximo 10 caracteres). Debe coincidir con un código de 'persona'.
/// 4. 'fkcodempresa': Opcional (máximo 10 caracteres). Si se envía, debe coincidir con un código de 'empresa'.
/// </summary>
public class ClienteCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo credito es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(0, 9999999999999999.99, ErrorMessage = "El crédito no puede ser negativo.")]
    // Propiedad 'Credito' (decimal?) mapeada desde SQL Server o JSON
    public decimal? Credito { get; set; }

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El campo fkcodpersona es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo fkcodpersona debe tener entre 1 y 10 caracteres.")]
    // Propiedad 'Fkcodpersona' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodpersona { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, ErrorMessage = "El campo fkcodempresa no puede exceder 10 caracteres.")]
    // Propiedad 'Fkcodempresa' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodempresa { get; set; }
}

