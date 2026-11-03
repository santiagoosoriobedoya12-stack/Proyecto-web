using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Factura.
/// </summary>
public interface IServicioFactura
{
    Task<List<Factura>> ListarAsync();
    Task<Factura> ConsultarAsync(int numero);
    Task<Factura> CrearAsync(FacturaCrear peticion);
    Task<string> AnularAsync(int numero);
}
