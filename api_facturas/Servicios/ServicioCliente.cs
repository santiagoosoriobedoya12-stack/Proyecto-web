// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de la lógica de negocio para la entidad Cliente.
/// Valida reglas de negocio antes de persistir y traduce estados vacíos o ausentes a excepciones de dominio.
/// </summary>
public class ServicioCliente : IServicioCliente
{
    private readonly IRepositorioCliente _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioCliente(IRepositorioCliente repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    /// <summary>
    /// Consulta los clientes asegurando que el límite sea un valor entero positivo (> 0).
    /// </summary>
    public async Task<List<Cliente>> ListarAsync(int limite)
    {
        // Regla de negocio: impide solicitar listados con límite cero o negativo
        if (limite <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    /// <summary>
    /// Obtiene un cliente por ID. Si no existe, lanza NoEncontradoExcepcion para HTTP 404.
    /// </summary>
    public async Task<Cliente> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        var cliente = await _repositorio.ObtenerPorIdAsync(id);
        if (cliente == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un cliente con id = {id}");
        }

        return cliente;
    }

    /// <summary>
    /// Crea un nuevo cliente mapeando los campos del DTO a la entidad.
    /// Retorna el ID autogenerado.
    /// </summary>
    public async Task<int> CrearAsync(ClienteCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var entidad = new Cliente
        {
            Credito = peticion.Credito!.Value,
            Fkcodpersona = peticion.Fkcodpersona!.Trim(),
            Fkcodempresa = string.IsNullOrWhiteSpace(peticion.Fkcodempresa) ? null : peticion.Fkcodempresa.Trim()
        };

        return await _repositorio.CrearAsync(entidad);
    }

    /// <summary>
    /// Reemplaza completamente los campos de un cliente existente (PUT).
    /// Si 'fkcodempresa' es nulo o vacío, se actualiza explícitamente a null (persona natural).
    /// </summary>
    public async Task<int> ReemplazarAsync(int id, ClienteReemplazo peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un cliente con id = {id}");
        }

        var datos = new Dictionary<string, object?>
        {
            ["credito"] = peticion.Credito!.Value,
            ["fkcodpersona"] = peticion.Fkcodpersona!.Trim(),
            ["fkcodempresa"] = string.IsNullOrWhiteSpace(peticion.Fkcodempresa) ? null : peticion.Fkcodempresa.Trim()
        };

        return await _repositorio.ActualizarAsync(id, datos);
    }

    /// <summary>
    /// Actualiza únicamente los campos que vienen con valor en la petición PATCH.
    /// </summary>
    public async Task<int> ActualizarParcialAsync(int id, ClienteActualizar peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposModificados())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un cliente con id = {id}");
        }

        var datos = new Dictionary<string, object?>();

        if (peticion.Credito.HasValue)
        {
            datos["credito"] = peticion.Credito.Value;
        }

        if (peticion.Fkcodpersona != null)
        {
            datos["fkcodpersona"] = peticion.Fkcodpersona.Trim();
        }

        if (peticion.Fkcodempresa != null)
        {
            datos["fkcodempresa"] = string.IsNullOrWhiteSpace(peticion.Fkcodempresa) ? null : peticion.Fkcodempresa.Trim();
        }

        return await _repositorio.ActualizarAsync(id, datos);
    }

    /// <summary>
    /// Elimina un cliente por su ID.
    /// </summary>
    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un cliente con id = {id}");
        }

        return await _repositorio.EliminarAsync(id);
    }
}

