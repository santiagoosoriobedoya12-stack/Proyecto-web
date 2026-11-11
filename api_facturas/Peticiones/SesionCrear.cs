// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición de inicio de sesión (Versión 3).
/// Recibe las credenciales estrictamente en el cuerpo HTTP, nunca en la URL.
/// </summary>
public class SesionCrear
{
    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    // Verifica que la cadena tenga estructura válida de correo electrónico
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    // Propiedad 'Email' (string) mapeada desde SQL Server o JSON
    public string Email { get; set; } = string.Empty;

    // Campo obligatorio: si falta o llega vacío, ASP.NET Core rechaza la petición con HTTP 422
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    // Propiedad 'Contrasena' (string) mapeada desde SQL Server o JSON
    public string Contrasena { get; set; } = string.Empty;
}

