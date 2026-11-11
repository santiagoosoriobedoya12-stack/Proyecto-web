namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Persona.
/// </summary>
public class Persona
{
    // Propiedad 'Codigo' (string) mapeada desde SQL Server o JSON
    public string Codigo { get; set; } = string.Empty;
    // Propiedad 'Nombre' (string) mapeada desde SQL Server o JSON
    public string Nombre { get; set; } = string.Empty;
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    // Propiedad 'Telefono' (string) mapeada desde SQL Server o JSON
    public string Telefono { get; set; } = string.Empty;
}

