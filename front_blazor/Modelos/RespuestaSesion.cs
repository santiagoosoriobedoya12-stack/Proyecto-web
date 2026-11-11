namespace FrontFacturas.Modelos;

/// <summary>
/// Modelo de respuesta entregado por el endpoint POST /api/sesion.
/// </summary>
public class RespuestaSesion
{
    // Propiedad 'Estado' (int) mapeada desde SQL Server o JSON
    public int Estado { get; set; } = 200;
    // Propiedad 'Mensaje' (string) mapeada desde SQL Server o JSON
    public string Mensaje { get; set; } = string.Empty;
    // Propiedad 'Token' (string) mapeada desde SQL Server o JSON
    public string Token { get; set; } = string.Empty;
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    // Propiedad 'Expira' (DateTime) mapeada desde SQL Server o JSON
    public DateTime Expira { get; set; }
}

/// <summary>
/// Modelo de respuesta entregado por el endpoint GET /api/permisos/mios.
/// </summary>
public class RespuestaPermisosMios
{
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    // Propiedad 'Total' (int) mapeada desde SQL Server o JSON
    public int Total { get; set; }
    public List<string> Datos { get; set; } = new();
}

/// <summary>
/// Modelo de petición para enviar las credenciales a POST /api/sesion.
/// </summary>
public class PeticionSesion
{
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;
    // Propiedad 'Contrasena' (string) mapeada desde SQL Server o JSON
    public string Contrasena { get; set; } = string.Empty;
}

