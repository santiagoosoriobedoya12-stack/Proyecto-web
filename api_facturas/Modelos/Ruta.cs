namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'ruta' de la base de datos bdfacturas.
/// En el modelo de seguridad RBAC, representa interfaces y acciones protegibles
/// (ej. 'interfaz.productos', 'permiso.crear').
/// </summary>
public class Ruta
{
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }
    public required string NombreRuta { get; set; }
    public required string Descripcion { get; set; }
}

