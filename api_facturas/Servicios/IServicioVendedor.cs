using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Vendedor.
/// </summary>
public interface IServicioVendedor
{
    Task<List<Vendedor>> ListarAsync(int limite);
    Task<Vendedor> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(VendedorCrear peticion);
    Task<int> ReemplazarAsync(int id, VendedorReemplazo peticion);
    Task<int> ActualizarParcialAsync(int id, VendedorActualizar peticion);
    Task<int> EliminarAsync(int id);
}

