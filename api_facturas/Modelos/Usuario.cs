namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo Entidad: Representa la tabla 'usuario' de la base de datos bdfacturas.
/// REGLA DE SEGURIDAD (5_data_model.md):
/// La API NUNCA devuelve la contraseña en las respuestas.
/// El modelo de salida contiene únicamente el Email.
/// </summary>
public class Usuario
{
    public required string Email { get; set; }
}

