// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición de entrada para el verbo PUT (Reemplazo COMPLETO de Producto).
/// La semántica de PUT exige que el cliente envíe TODOS los datos del recurso.
/// Omitir un campo es un error 422 (no significa 'dejarlo como estaba').
/// NOTA: El 'codigo' NO se envía en el cuerpo porque ya viaja en la ruta de la URL (/api/producto/{codigo}).
/// </summary>
public class ProductoReemplazo
{
    /// <summary>
    /// Nombre del producto: Obligatorio en el reemplazo completo.
    /// </summary>
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    // Propiedad 'Nombre' (string?) mapeada desde SQL Server o JSON
    public string? Nombre { get; set; }

    /// <summary>
    /// Stock del producto: Obligatorio en el reemplazo completo.
    /// </summary>
    [Required(ErrorMessage = "El campo stock es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(0, int.MaxValue, ErrorMessage = "El campo stock debe ser un entero mayor o igual a 0.")]
    // Propiedad 'Stock' (int?) mapeada desde SQL Server o JSON
    public int? Stock { get; set; }

    /// <summary>
    /// Valor unitario del producto: Obligatorio en el reemplazo completo.
    /// </summary>
    [Required(ErrorMessage = "El campo valorunitario es obligatorio.")]
    // Restringe el rango numérico permitido para prevenir valores negativos o fuera de dominio
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "El campo valorunitario debe ser un número mayor o igual a 0.")]
    // Propiedad 'Valorunitario' (decimal?) mapeada desde SQL Server o JSON
    public decimal? Valorunitario { get; set; }
}

