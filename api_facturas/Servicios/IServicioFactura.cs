// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
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
