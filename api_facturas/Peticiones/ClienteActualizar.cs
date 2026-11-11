// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO de Petición para la actualización parcial de un cliente (PATCH /api/cliente/{id}).
/// 
/// Características del verbo PATCH:
/// - Todos los campos son opcionales individualmente.
/// - El cliente envía únicamente las columnas que desea modificar.
/// - Debe contener al menos un campo con valor; de lo contrario se rechaza con HTTP 400.
/// </summary>
public class ClienteActualizar
{
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(0, 9999999999999999.99, ErrorMessage = "El crédito no puede ser negativo.")]
    // Propiedad 'Credito' (decimal?) mapeada desde SQL Server o JSON
    public decimal? Credito { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo fkcodpersona debe tener entre 1 y 10 caracteres.")]
    // Propiedad 'Fkcodpersona' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodpersona { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, ErrorMessage = "El campo fkcodempresa no puede exceder 10 caracteres.")]
    // Propiedad 'Fkcodempresa' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodempresa { get; set; }

    /// <summary>
    /// Comprueba si la petición contiene al menos un campo a modificar.
    /// Si todos son nulos, la petición es semánticamente vacía y no debe generar un UPDATE.
    /// </summary>
    public bool TieneCamposModificados() =>
        Credito.HasValue || Fkcodpersona != null || Fkcodempresa != null;
}

