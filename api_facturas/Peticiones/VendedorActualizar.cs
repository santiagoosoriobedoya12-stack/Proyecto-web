// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para la actualización parcial de un vendedor (PATCH /api/vendedor/{id}).
/// </summary>
public class VendedorActualizar
{
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(1, int.MaxValue, ErrorMessage = "El carnet debe ser un número entero positivo mayor a cero.")]
    // Propiedad 'Carnet' (int?) mapeada desde SQL Server o JSON
    public int? Carnet { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "La dirección no puede estar vacía ni superar 100 caracteres.")]
    // Propiedad 'Direccion' (string?) mapeada desde SQL Server o JSON
    public string? Direccion { get; set; }

    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código de persona debe tener entre 1 y 10 caracteres.")]
    // Propiedad 'Fkcodpersona' (string?) mapeada desde SQL Server o JSON
    public string? Fkcodpersona { get; set; }

    // Ejecuta la operación 'TieneCamposModificados' dentro de la capa correspondiente
    public bool TieneCamposModificados() =>
        Carnet.HasValue || Direccion != null || Fkcodpersona != null;
}

