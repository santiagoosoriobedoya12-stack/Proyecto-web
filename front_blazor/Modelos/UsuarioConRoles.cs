using System.Text.Json.Serialization;

namespace FrontFacturas.Modelos;

// Modelo / DTO 'UsuarioConRoles': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class UsuarioConRoles
{
    [JsonPropertyName("email")]
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<RolAsignado> Roles { get; set; } = new();
}

// Modelo / DTO 'RolAsignado': estructura fuertemente tipada que representa datos de tabla o petición JSON
public class RolAsignado
{
    [JsonPropertyName("idrol")]
    // Propiedad 'IdRol' (int) mapeada desde SQL Server o JSON
    public int IdRol { get; set; }

    [JsonPropertyName("nombre")]
    // Propiedad 'Nombre' (string) mapeada desde SQL Server o JSON
    public string Nombre { get; set; } = string.Empty;
}
