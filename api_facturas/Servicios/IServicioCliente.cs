using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato de la capa de servicio para el recurso Cliente.
/// Orquesta las reglas de negocio y desacopla los controladores HTTP del acceso a datos.
/// </summary>
public interface IServicioCliente
{
    /// <summary>
    /// Consulta la lista de clientes con validación del límite.
    /// </summary>
    Task<List<Cliente>> ListarAsync(int limite);

    /// <summary>
    /// Obtiene un cliente por su id; lanza NoEncontradoExcepcion si no existe.
    /// </summary>
    Task<Cliente> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Crea un cliente y retorna el id asignado por la base de datos.
    /// </summary>
    Task<int> CrearAsync(ClienteCrear peticion);

    /// <summary>
    /// Reemplaza todos los datos de un cliente (PUT).
    /// </summary>
    Task<int> ReemplazarAsync(int id, ClienteReemplazo peticion);

    /// <summary>
    /// Actualiza únicamente los campos provistos (PATCH).
    /// </summary>
    Task<int> ActualizarParcialAsync(int id, ClienteActualizar peticion);

    /// <summary>
    /// Elimina un cliente por su id; lanza NoEncontradoExcepcion si no existe.
    /// </summary>
    Task<int> EliminarAsync(int id);
}

