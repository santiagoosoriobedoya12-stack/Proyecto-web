namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de respuesta entregado por el endpoint POST /api/sesion.
/// </summary>
public class RespuestaSesion
{
    public int Estado { get; set; } = 200;
    public string Mensaje { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expira { get; set; }
}

/// <summary>
/// Modelo de respuesta entregado por el endpoint GET /api/permisos/mios.
/// </summary>
public class RespuestaPermisosMios
{
    public string Email { get; set; } = string.Empty;
    public int Total { get; set; }
    public List<string> Datos { get; set; } = new();
}

/// <summary>
/// Modelo de petición para enviar las credenciales a POST /api/sesion.
/// </summary>
public class PeticionSesion
{
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

