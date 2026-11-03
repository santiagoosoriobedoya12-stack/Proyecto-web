using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

public class UsuarioConRoles
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<RolAsignado> Roles { get; set; } = new();
}

public class RolAsignado
{
    [JsonPropertyName("idrol")]
    public int IdRol { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
}
