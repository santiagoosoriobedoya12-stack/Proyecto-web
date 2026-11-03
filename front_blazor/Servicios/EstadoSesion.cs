namespace FrontFacturas.Servicios;

/// <summary>
/// Servicio de estado de sesión para Blazor Server (Versión 3).
/// Se registra como SCOPED (uno por circuito de usuario conectado).
/// El token vive exclusivamente en la memoria del servidor y nunca se expone a JavaScript en el navegador.
/// </summary>
public class EstadoSesion
{
    public string? Token { get; private set; }
    public string? Email { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public List<string> RutasPermitidas { get; private set; } = new();

    public bool EstaAutenticado => !string.IsNullOrWhiteSpace(Token);

    public event Action? AlCambiarEstado;

    public void IniciarSesion(string token, string email, List<string> roles, List<string> rutas)
    {
        Token = token;
        Email = email;
        Roles = roles ?? new();
        RutasPermitidas = rutas ?? new();
        AlCambiarEstado?.Invoke();
    }

    public void CerrarSesion()
    {
        Token = null;
        Email = null;
        Roles.Clear();
        RutasPermitidas.Clear();
        AlCambiarEstado?.Invoke();
    }

    public bool TienePermiso(string ruta)
    {
        if (!EstaAutenticado) return false;
        if (Roles.Contains("Administrador")) return true;
        return RutasPermitidas.Contains(ruta);
    }
}

