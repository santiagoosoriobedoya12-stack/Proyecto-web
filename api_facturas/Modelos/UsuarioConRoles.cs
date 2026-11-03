using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para el recurso compuesto 'usuario-con-roles'.
/// Retorna el usuario junto con la colección de roles asignados.
/// NOTA DE SEGURIDAD: Nunca expone la contraseña en respuestas de lectura.
/// </summary>
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
