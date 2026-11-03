namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de configuración de JSON Web Tokens (JWT) para la Versión 3.
/// Almacena los parámetros criptográficos requeridos para firmar y validar tokens.
/// </summary>
public class ConfiguracionJwt
{
    /// <summary>
    /// Clave secreta simétrica. Debe contener al menos 32 caracteres (256 bits para HS256).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Emisor del token (Issuer).
    /// </summary>
    public string Issuer { get; set; } = "ApiFacturas";

    /// <summary>
    /// Audiencia del token (Audience).
    /// </summary>
    public string Audience { get; set; } = "FrontFacturas";

    /// <summary>
    /// Tiempo de vigencia del token en horas (por defecto 1 hora según el plan v3).
    /// </summary>
    public int ExpiracionHoras { get; set; } = 1;
}

