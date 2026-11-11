namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de respuesta entregado tras una autenticación exitosa (Versión 3).
/// Contiene el token JWT, los roles del usuario y la fecha de expiración.
/// </summary>
public class Sesion
{
    // Propiedad 'Estado' (int) mapeada desde SQL Server o JSON
    public int Estado { get; set; } = 200;
    // Propiedad 'Mensaje' (string) mapeada desde SQL Server o JSON
    public string Mensaje { get; set; } = "Sesion iniciada.";
    // Propiedad 'Token' (string) mapeada desde SQL Server o JSON
    public string Token { get; set; } = string.Empty;
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    // Propiedad 'Expira' (DateTime) mapeada desde SQL Server o JSON
    public DateTime Expira { get; set; }
}

