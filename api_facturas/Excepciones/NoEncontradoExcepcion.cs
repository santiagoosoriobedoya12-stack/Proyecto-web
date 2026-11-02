namespace ApiFacturas.Excepciones;

/// <summary>
/// Excepción de dominio para representar la ausencia de un recurso buscado.
/// El controlador capturará esta excepción específica y la transformará en una respuesta HTTP 404 Not Found.
/// Separar esta excepción evita que la lógica de negocio (Servicio) dependa de clases o códigos HTTP.
/// </summary>
public class NoEncontradoExcepcion : Exception
{
    public NoEncontradoExcepcion(string mensaje) : base(mensaje)
    {
    }
}
