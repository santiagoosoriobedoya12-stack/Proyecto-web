using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// DTO para la actualización parcial de un vendedor (PATCH /api/vendedor/{id}).
/// </summary>
public class VendedorActualizar
{
    [Range(1, int.MaxValue, ErrorMessage = "El carnet debe ser un número entero positivo mayor a cero.")]
    public int? Carnet { get; set; }

    [StringLength(100, MinimumLength = 1, ErrorMessage = "La dirección no puede estar vacía ni superar 100 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(10, MinimumLength = 1, ErrorMessage = "El código de persona debe tener entre 1 y 10 caracteres.")]
    public string? Fkcodpersona { get; set; }

    public bool TieneCamposModificados() =>
        Carnet.HasValue || Direccion != null || Fkcodpersona != null;
}

