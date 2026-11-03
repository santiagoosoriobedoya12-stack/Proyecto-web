namespace ApiFacturas.Modelos;

/// <summary>
/// Modelo de Dominio / Entidad: Representa una fila de la tabla 'cliente' en bdfacturas.
/// 
/// Características arquitectónicas:
/// 1. Clave primaria 'id': Es de tipo INT autoincremental (IDENTITY). No se envía en la creación.
/// 2. Clave foránea 'fkcodpersona': Relación obligatoria 1 a N con la tabla 'persona'.
///    Un cliente en este dominio siempre debe estar respaldado por los datos básicos de una persona.
/// 3. Clave foránea 'fkcodempresa': Relación opcional/nullable (NVARCHAR(10)?) con 'empresa'.
///    Permite modelar clientes como personas naturales (fkcodempresa = null) o clientes corporativos.
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único autogenerado por SQL Server (IDENTITY).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Límite de crédito otorgado al cliente (DECIMAL 18,2).
    /// </summary>
    public decimal Credito { get; set; }

    /// <summary>
    /// Clave foránea que referencia a persona(codigo). Es obligatoria.
    /// </summary>
    public required string Fkcodpersona { get; set; }

    /// <summary>
    /// Clave foránea que referencia a empresa(codigo). Es opcional (puede ser null).
    /// </summary>
    public string? Fkcodempresa { get; set; }
}

