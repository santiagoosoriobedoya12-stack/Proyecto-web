namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Usuario.
/// </summary>
public class Usuario
{
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    // Propiedad 'Contrasena' (string) mapeada desde SQL Server o JSON
    public string Contrasena { get; set; } = string.Empty;
}

