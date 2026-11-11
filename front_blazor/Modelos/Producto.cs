namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo 'Producto' PROPIO del Frontend.
/// REGLA ARQUITECTÓNICA:
/// Aunque tiene los mismos campos que el modelo de la API, es una clase totalmente independiente.
/// NO se comparte por referencia de proyecto para evitar el acoplamiento rígido entre procesos.
/// El frontend modela los datos según el contrato JSON que recibe por HTTP.
/// </summary>
public class Producto
{
    // Propiedad 'Codigo' (string) mapeada desde SQL Server o JSON
    public string Codigo { get; set; } = string.Empty;
    // Propiedad 'Nombre' (string) mapeada desde SQL Server o JSON
    public string Nombre { get; set; } = string.Empty;
    // Propiedad 'Stock' (int) mapeada desde SQL Server o JSON
    public int Stock { get; set; }
    // Propiedad 'Valorunitario' (decimal) mapeada desde SQL Server o JSON
    public decimal Valorunitario { get; set; }
}

