using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de la lógica de negocio para la entidad Cliente.
/// Valida reglas de negocio antes de persistir y traduce estados vacíos o ausentes a excepciones de dominio.
/// </summary>
public class ServicioCliente : IServicioCliente
{
    private readonly IRepositorioCliente _repositorio;

    public ServicioCliente(IRepositorioCliente repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    /// <summary>
    /// Consulta los clientes asegurando que el límite sea un valor entero positivo (> 0).
    /// </summary>
    public async Task<List<Cliente>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
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
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        var cliente = await _repositorio.ObtenerPorIdAsync(id);
        if (cliente == null)
        {
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
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
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
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        if (peticion == null || !peticion.TieneCamposModificados())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
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
            throw new ArgumentException("El identificador del cliente debe ser un entero positivo.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un cliente con id = {id}");
        }

        return await _repositorio.EliminarAsync(id);
    }
}

