namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Rol.
/// </summary>
public class Rol
{
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }
    // Propiedad 'Nombre' (string) mapeada desde SQL Server o JSON
    public string Nombre { get; set; } = string.Empty;
}

