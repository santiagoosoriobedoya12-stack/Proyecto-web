namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'rol' de la base de datos bdfacturas.
/// Posee clave autoincremental IDENTITY (INT).
/// </summary>
public class Rol
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
}

