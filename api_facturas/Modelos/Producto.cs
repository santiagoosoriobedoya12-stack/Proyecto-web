namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'producto' de la base de datos bdfacturas.
/// En C#, las propiedades con { get; set; } actúan como los getters y setters del lenguaje.
/// Dapper utiliza esta clase para mapear automáticamente cada columna de la consulta SQL
/// a su correspondiente propiedad por nombre exacto.
/// </summary>
public class Producto
{
    /// <summary>
    /// Clave primaria (PK) en la tabla producto (VARCHAR(10)).
    /// 'required' garantiza en C# que el objeto no se instancie sin su clave.
    /// </summary>
    public required string Codigo { get; set; }

    /// <summary>
    /// Nombre descriptivo del producto (VARCHAR(100)).
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Cantidad disponible en inventario (INT). Debe ser mayor o igual a 0.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Precio unitario del producto (DECIMAL(18,2)).
    /// En C#, 'decimal' es el tipo numérico exacto para valores monetarios.
    /// </summary>
    public decimal Valorunitario { get; set; }
}

