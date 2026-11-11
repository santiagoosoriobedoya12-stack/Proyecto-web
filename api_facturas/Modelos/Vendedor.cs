namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Entidad: Representa la tabla 'vendedor' en bdfacturas.
/// 
/// Características:
/// - 'id': Clave primaria autoincremental IDENTITY (INT).
/// - 'carnet': Número de carnet asignado al vendedor (INT).
/// - 'direccion': Dirección de correspondencia (NVARCHAR 100).
/// - 'fkcodpersona': Clave foránea obligatoria hacia persona(codigo).
///   Un vendedor siempre es una persona registrada en el sistema.
/// </summary>
public class Vendedor
{
    // Propiedad 'Id' (int) mapeada desde SQL Server o JSON
    public int Id { get; set; }
    // Propiedad 'Carnet' (int) mapeada desde SQL Server o JSON
    public int Carnet { get; set; }
    public required string Direccion { get; set; }
    public required string Fkcodpersona { get; set; }
}

