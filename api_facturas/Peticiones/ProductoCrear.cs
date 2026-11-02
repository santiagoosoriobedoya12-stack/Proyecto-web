using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición de entrada para el verbo POST (Creación de Producto).
/// Esta clase actúa como la primera muralla o frontera de entrada.
/// Declara mediante anotaciones (Data Annotations) las reglas requeridas.
/// ASP.NET Core valida el cuerpo JSON contra estas reglas antes de ejecutar el controlador.
/// Si alguna falla, responde automáticamente con HTTP 422 y la lista de errores.
/// </summary>
public class ProductoCrear
{
    /// <summary>
    /// Código del producto: Obligatorio, longitud de 1 a 10 caracteres.
    /// En el POST se incluye obligatoriamente en el cuerpo porque es la clave primaria manual.
    /// </summary>
    [Required(ErrorMessage = "El campo codigo es obligatorio.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El campo codigo debe tener entre 1 y 10 caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>
    /// Nombre del producto: Obligatorio, texto no vacío de hasta 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Cantidad en stock: Obligatoria, entero mayor o igual a 0.
    /// NOTA DIDÁCTICA: Se declara 'int?' (anulable) para que el tipo actúe como regla.
    /// Si el cliente envía texto o decimales (ej. 7.5), ASP.NET no puede deserializarlo
    /// y genera inmediatamente un error de validación 422 sin tocar la base de datos.
    /// </summary>
    [Required(ErrorMessage = "El campo stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo stock debe ser un entero mayor o igual a 0.")]
    public int? Stock { get; set; }

    /// <summary>
    /// Valor unitario del producto: Obligatorio, número decimal mayor o igual a 0.
    /// </summary>
    [Required(ErrorMessage = "El campo valorunitario es obligatorio.")]
    [Range(0.0, (double)decimal.MaxValue, ErrorMessage = "El campo valorunitario debe ser un número mayor o igual a 0.")]
    public decimal? Valorunitario { get; set; }
}
