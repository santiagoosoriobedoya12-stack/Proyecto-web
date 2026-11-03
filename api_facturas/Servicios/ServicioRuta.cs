using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación del servicio de negocio para Ruta.
/// </summary>
public class ServicioRuta : IServicioRuta
{
    private readonly IRepositorioRuta _repositorio;

    public ServicioRuta(IRepositorioRuta repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Ruta>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Ruta> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        var ruta = await _repositorio.ObtenerPorIdAsync(id);
        if (ruta == null)
        {
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        return ruta;
    }

    public async Task<int> CrearAsync(RutaCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var ruta = new Ruta
        {
            NombreRuta = peticion.Ruta!.Trim(),
            Descripcion = peticion.Descripcion!.Trim()
        };

        return await _repositorio.CrearAsync(ruta);
    }

    public async Task<int> ReemplazarAsync(int id, RutaReemplazo peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        var datos = new Dictionary<string, object>
        {
            ["ruta"] = peticion.Ruta!.Trim(),
            ["descripcion"] = peticion.Descripcion!.Trim()
        };

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> ActualizarParcialAsync(int id, RutaActualizar peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Ruta != null)
            datos["ruta"] = peticion.Ruta.Trim();
        if (peticion.Descripcion != null)
            datos["descripcion"] = peticion.Descripcion.Trim();

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        return filas;
    }
}

