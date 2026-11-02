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
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Valorunitario { get; set; }
}

