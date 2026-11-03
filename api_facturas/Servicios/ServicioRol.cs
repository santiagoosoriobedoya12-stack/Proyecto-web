using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de la lógica de negocio para Rol.
/// </summary>
public class ServicioRol : IServicioRol
{
    private readonly IRepositorioRol _repositorio;

    public ServicioRol(IRepositorioRol repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Rol>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Rol> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del rol debe ser un entero positivo.");
        }

        var rol = await _repositorio.ObtenerPorIdAsync(id);
        if (rol == null)
        {
            throw new NoEncontradoExcepcion($"No existe un rol con id = {id}");
        }

        return rol;
    }

    public async Task<int> CrearAsync(RolCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var rol = new Rol
        {
            Nombre = peticion.Nombre!.Trim()
        };

        return await _repositorio.CrearAsync(rol);
    }

    public async Task<int> ReemplazarAsync(int id, RolReemplazo peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del rol debe ser un entero positivo.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un rol con id = {id}");
        }

        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!.Trim()
        };

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> ActualizarParcialAsync(int id, RolActualizar peticion)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del rol debe ser un entero positivo.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un rol con id = {id}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Nombre != null)
        {
            datos["nombre"] = peticion.Nombre.Trim();
        }

        return await _repositorio.ActualizarAsync(id, datos);
    }

    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El identificador del rol debe ser un entero positivo.");
        }

        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un rol con id = {id}");
        }

        return filas;
    }
}

