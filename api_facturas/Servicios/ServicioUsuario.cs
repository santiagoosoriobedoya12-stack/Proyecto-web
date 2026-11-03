using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de la lógica de negocio para Usuario.
/// </summary>
public class ServicioUsuario : IServicioUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    public ServicioUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Usuario>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Usuario> ObtenerPorEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        var usuario = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (usuario == null)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        return usuario;
    }

    public async Task CrearAsync(UsuarioCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        await _repositorio.CrearAsync(peticion.Email!.Trim(), peticion.Contrasena!);
    }

    public async Task<int> ReemplazarAsync(string email, UsuarioReemplazo peticion)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        var datos = new Dictionary<string, object>
        {
            ["contrasena"] = peticion.Contrasena!
        };

        return await _repositorio.ActualizarAsync(email.Trim(), datos);
    }

    public async Task<int> ActualizarParcialAsync(string email, UsuarioActualizar peticion)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Contrasena != null)
        {
            datos["contrasena"] = peticion.Contrasena;
        }

        return await _repositorio.ActualizarAsync(email.Trim(), datos);
    }

    public async Task<int> EliminarAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(email.Trim());
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        return filas;
    }
}

