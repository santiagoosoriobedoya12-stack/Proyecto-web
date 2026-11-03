using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/rol/{id}.
/// </summary>
public class RolActualizar
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni superar 50 caracteres.")]
    public string? Nombre { get; set; }

    public bool TieneCamposParaActualizar()
    {
        return Nombre != null;
    }
}

