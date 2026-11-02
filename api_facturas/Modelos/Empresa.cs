namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'empresa' de la base de datos bdfacturas.
/// En la Versión 1, es una de las 6 tablas independientes (sin clave foránea).
/// </summary>
public class Empresa
{
    /// <summary>
    /// Código identificador de la empresa (VARCHAR(10)). Clave primaria.
    /// </summary>
    public required string Codigo { get; set; }

    /// <summary>
    /// Razón social o nombre de la empresa (VARCHAR(100)).
    /// </summary>
    public required string Nombre { get; set; }
}
