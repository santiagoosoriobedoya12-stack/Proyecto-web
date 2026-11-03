namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'persona' de la base de datos bdfacturas.
/// Una de las 6 tablas sin clave foránea de la Versión 1.
/// </summary>
public class Persona
{
    public required string Codigo { get; set; }
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Telefono { get; set; }
}

