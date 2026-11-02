using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato de la capa de Lógica de Negocio (Servicio) para Producto.
/// El controlador interactúa con esta interfaz.
/// El servicio contiene las validaciones de negocio y no conoce de códigos HTTP ni de conexiones a BD.
/// </summary>
public interface IServicioProducto
{
    Task<List<Producto>> ListarAsync(int limite);
    Task<Producto> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(ProductoCrear peticion);
    Task<int> ReemplazarAsync(string codigo, ProductoReemplazo peticion);
    Task<int> ActualizarParcialAsync(string codigo, ProductoActualizar peticion);
    Task<int> EliminarAsync(string codigo);
}

