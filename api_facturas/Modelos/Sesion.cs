namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de respuesta entregado tras una autenticación exitosa (Versión 3).
/// Contiene el token JWT, los roles del usuario y la fecha de expiración.
/// </summary>
public class Sesion
{
    public int Estado { get; set; } = 200;
    public string Mensaje { get; set; } = "Sesion iniciada.";
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expira { get; set; }
}

