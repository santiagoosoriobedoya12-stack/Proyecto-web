using System.Text.Json.Serialization;

namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad para la tabla puente 'rutarol'.
/// Representa qué rol tiene acceso a qué ruta o pantalla del sistema.
/// </summary>
public class RutaRol
{
    [JsonPropertyName("fkidruta")]
    // Propiedad 'Fkidruta' (int) mapeada desde SQL Server o JSON
    public int Fkidruta { get; set; }

    [JsonPropertyName("ruta")]
    // Propiedad 'Ruta' (string?) mapeada desde SQL Server o JSON
    public string? Ruta { get; set; }

    [JsonPropertyName("fkidrol")]
    // Propiedad 'Fkidrol' (int) mapeada desde SQL Server o JSON
    public int Fkidrol { get; set; }

    [JsonPropertyName("rol")]
    // Propiedad 'Rol' (string?) mapeada desde SQL Server o JSON
    public string? Rol { get; set; }
}
