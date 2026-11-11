namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo propio del frontend para Empresa.
/// </summary>
public class Empresa
{
    // Propiedad 'Codigo' (string) mapeada desde SQL Server o JSON
    public string Codigo { get; set; } = string.Empty;
    // Propiedad 'Nombre' (string) mapeada desde SQL Server o JSON
    public string Nombre { get; set; } = string.Empty;
}

