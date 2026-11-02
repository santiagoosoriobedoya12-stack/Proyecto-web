using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición de entrada para el verbo PATCH (Actualización PARCIAL de Producto).
/// En contraste con PUT, en PATCH TODOS los campos son opcionales:
/// Solo se modificarán en la base de datos las propiedades que el cliente envíe.
/// Cada propiedad que llegue se valida individualmente según sus restricciones.
/// NOTA: Si el cliente envía un JSON completamente vacío {}, el servicio devolverá HTTP 400.
/// </summary>
public class ProductoActualizar
{
    /// <summary>
    /// Nombre opcional: Si se envía, no puede ser vacío ni superar 100 caracteres.
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Stock opcional: Si se envía, debe ser un entero mayor o igual a 0.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El campo stock debe ser un entero mayor o igual a 0.")]
    public int? Stock { get; set; }

    /// <summary>
    /// Valor unitario opcional: Si se envía, debe ser un número decimal mayor o igual a 0.
    /// </summary>
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "El campo valorunitario debe ser un número mayor o igual a 0.")]
    public decimal? Valorunitario { get; set; }

    /// <summary>
    /// Método de utilidad para verificar si se envió al menos un campo para actualizar.
    /// Si ningún campo viene en la petición, la regla de negocio exige HTTP 400.
    /// </summary>
    public bool TieneCamposParaActualizar()
    {
        return Nombre != null || Stock.HasValue || Valorunitario.HasValue;
    }
}
