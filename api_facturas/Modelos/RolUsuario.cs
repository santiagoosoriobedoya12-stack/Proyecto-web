using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para la tabla puente 'rol_usuario'.
/// 
/// CARACTERÍSTICAS DE UNA TABLA PUENTE (RF4):
/// 1. Clave primaria compuesta (fkemail, fkidrol).
/// 2. No contiene campos propios (sin PUT ni PATCH).
/// 3. Existe o no existe la relación.
/// </summary>
public class RolUsuario
{
    [JsonPropertyName("fkemail")]
    // Propiedad 'Fkemail' (string) mapeada desde SQL Server o JSON
    public string Fkemail { get; set; } = string.Empty;

    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int) mapeada desde SQL Server o JSON
    public int Fkidrol { get; set; }

    [JsonPropertyName("nombre_rol")]
    // Propiedad 'NombreRol' (string?) mapeada desde SQL Server o JSON
    public string? NombreRol { get; set; }
}
