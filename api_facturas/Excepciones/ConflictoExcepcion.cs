namespace ApiFacturas.Excepciones;

/// <summary>
/// Excepción de dominio para representar conflictos de estado en el sistema (HTTP 409 Conflict).
/// Se utiliza en:
/// 1. Violación de clave foránea (intentar asociar una persona o empresa que no existe).
/// 2. Violación de clave primaria en tablas puente (intentar asignar un rol o ruta duplicada).
/// 3. Conflicto de negocio (intentar anular una factura que ya se encuentra anulada).
///
/// La separación en esta clase permite que la capa de servicio y repositorio exprese
/// la regla de negocio sin acoplarse directamente a los tipos de Microsoft.AspNetCore.Mvc.
/// </summary>
public class ConflictoExcepcion : Exception
{
    public ConflictoExcepcion(string mensaje) : base(mensaje)
    {
    }

    public ConflictoExcepcion(string mensaje, Exception innerException) : base(mensaje, innerException)
    {
    }
}

