// Atributos de validación automática de modelos ([Required], [StringLength], [Range], [EmailAddress])
using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

/// <summary>
/// Petición para PATCH /api/empresa/{codigo}.
/// Actualización parcial: el nombre es opcional.
/// </summary>
public class EmpresaActualizar
{
    // Limita longitud mínima y máxima de texto según el tamaño de la columna en SQL Server
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo nombre no puede estar vacío ni exceder 100 caracteres.")]
    // Propiedad 'Nombre' (string?) mapeada desde SQL Server o JSON
    public string? Nombre { get; set; }

    // Verifica que al menos un campo opcional haya sido enviado en el PATCH
    public bool TieneCamposParaActualizar()
    {
        return Nombre != null;
    }
}

