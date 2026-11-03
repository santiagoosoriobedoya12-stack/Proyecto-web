using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Lógica de negocio para Vendedor.
/// </summary>
public class ServicioVendedor : IServicioVendedor
{
    private readonly IRepositorioVendedor _repositorio;

    public ServicioVendedor(IRepositorioVendedor repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Vendedor>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Vendedor> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        var vendedor = await _repositorio.ObtenerPorIdAsync(id);
        if (vendedor == null)
        {
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        return vendedor;
    }

    public async Task<int> CrearAsync(VendedorCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var entidad = new Vendedor
        {
            Carnet = peticion.Carnet!.Value,
            Direccion = peticion.Direccion!.Trim(),
            Fkcodpersona = peticion.Fkcodpersona!.Trim()
        };

        return await _repositorio.CrearAsync(entidad);
    }

    public async Task<int> ReemplazarAsync(int id, VendedorReemplazo peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        var datos = new Dictionary<string, object>
        {
            ["carnet"] = peticion.Carnet!.Value,
            ["direccion"] = peticion.Direccion!.Trim(),
            ["fkcodpersona"] = peticion.Fkcodpersona!.Trim()
        };

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> ActualizarParcialAsync(int id, VendedorActualizar peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        if (peticion == null || !peticion.TieneCamposModificados())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        var datos = new Dictionary<string, object>();

        if (peticion.Carnet.HasValue)
        {
            datos["carnet"] = peticion.Carnet.Value;
        }

        if (peticion.Direccion != null)
        {
            datos["direccion"] = peticion.Direccion.Trim();
        }

        if (peticion.Fkcodpersona != null)
        {
            datos["fkcodpersona"] = peticion.Fkcodpersona.Trim();
        }

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        return await _repositorio.EliminarAsync(id);
    }
}

