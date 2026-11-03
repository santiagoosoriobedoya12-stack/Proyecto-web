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
    public int Id { get; set; }
    public int Carnet { get; set; }
    public required string Direccion { get; set; }
    public required string Fkcodpersona { get; set; }
}

