// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato del Repositorio para Facturas.
/// 
/// REGLA ARQUITECTÓNICA DE LA VERSIÓN 2:
/// - La API NO escribe sentencias SQL de tablas (SELECT/INSERT/UPDATE) para Factura.
/// - Todas las operaciones se realizan invocando procedimientos almacenados oficiales:
///   1. sp_listar_facturas_y_productosporfactura
///   2. sp_consultar_factura_y_productosporfactura
///   3. sp_insertar_factura_y_productosporfactura
///   4. sp_anular_factura
/// </summary>
public interface IRepositorioFactura
{
    Task<List<Factura>> ListarAsync();
    Task<Factura> ConsultarAsync(int numero);
    Task<Factura> CrearAsync(FacturaCrear peticion);
    Task<string> AnularAsync(int numero);
}
