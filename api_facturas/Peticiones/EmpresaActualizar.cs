using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/empresa/{codigo}.
/// Actualización parcial: el nombre es opcional.
/// </summary>
public class EmpresaActualizar
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    public bool TieneCamposParaActualizar()
    {
        return Nombre != null;
    }
}

