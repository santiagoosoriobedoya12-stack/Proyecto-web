using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de acceso a datos para la entidad Cliente.
/// Desacopla la lógica de negocio del motor de base de datos relacional (SQL Server).
/// </summary>
public interface IRepositorioCliente
{
    /// <summary>
    /// Consulta una lista de clientes ordenados por ID con un límite máximo.
    /// </summary>
    Task<List<Cliente>> ObtenerTodosAsync(int limite);

    /// <summary>
    /// Obtiene un cliente por su clave primaria autoincremental (id).
    /// </summary>
    Task<Cliente?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Inserta un nuevo cliente y retorna el ID autogenerado por IDENTITY(1,1).
    /// </summary>
    Task<int> CrearAsync(Cliente cliente);

    /// <summary>
    /// Actualiza de forma dinámica y parametrizada las columnas indicadas en el diccionario.
    /// </summary>
    Task<int> ActualizarAsync(int id, Dictionary<string, object?> datos);

    /// <summary>
    /// Elimina físicamente un cliente por su ID.
    /// </summary>
    Task<int> EliminarAsync(int id);
}

