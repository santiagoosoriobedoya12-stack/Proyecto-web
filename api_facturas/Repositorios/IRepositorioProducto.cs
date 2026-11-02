using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de la capa de datos (Repositorio) para la entidad Producto.
/// Siguiendo el Principio de Inversión de Dependencias (DIP):
/// El servicio dependerá exclusivamente de esta INTERFAZ y nunca de una clase concreta
/// como SQL Server o PostgreSQL.
/// Todos los métodos son asíncronos (Task) para no bloquear hilos del servidor durante I/O.
/// </summary>
public interface IRepositorioProducto
{
    /// <summary>
    /// Consulta todos los productos con un límite máximo especificado.
    /// </summary>
    /// <param name="limite">Cantidad máxima de filas a retornar.</param>
    /// <returns>Lista de objetos Producto.</returns>
    Task<List<Producto>> ObtenerTodosAsync(int limite);

    /// <summary>
    /// Busca un producto por su clave primaria (código).
    /// </summary>
    /// <param name="codigo">Código del producto a buscar.</param>
    /// <returns>El objeto Producto encontrado, o null si no existe.</returns>
    Task<Producto?> ObtenerPorCodigoAsync(string codigo);

    /// <summary>
    /// Inserta un nuevo producto en la base de datos.
    /// </summary>
    /// <param name="producto">Instancia del modelo Producto a persistir.</param>
    Task CrearAsync(Producto producto);

    /// <summary>
    /// Actualiza campos específicos de un producto existente (PUT y PATCH).
    /// Se utiliza un Diccionario columna->valor para permitir la actualización dinámica
    /// de los campos que hayan sido enviados por el cliente.
    /// </summary>
    /// <param name="codigo">Código del producto a actualizar.</param>
    /// <param name="datos">Diccionario con los nombres de columna y sus nuevos valores.</param>
    /// <returns>El número de filas afectadas en la base de datos.</returns>
    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);

    /// <summary>
    /// Elimina físicamente un producto por su código.
    /// </summary>
    /// <param name="codigo">Código del producto a eliminar.</param>
    /// <returns>El número de filas eliminadas.</returns>
    Task<int> EliminarAsync(string codigo);
}
