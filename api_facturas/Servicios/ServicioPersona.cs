using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de la lógica de negocio para Persona.
/// </summary>
public class ServicioPersona : IServicioPersona
{
    private readonly IRepositorioPersona _repositorio;

    public ServicioPersona(IRepositorioPersona repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Persona>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Persona> ObtenerPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        var persona = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (persona == null)
        {
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        return persona;
    }

    public async Task CrearAsync(PersonaCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var persona = new Persona
        {
            Codigo = peticion.Codigo!.Trim(),
            Nombre = peticion.Nombre!.Trim(),
            Email = peticion.Email!.Trim(),
            Telefono = peticion.Telefono!.Trim()
        };

        await _repositorio.CrearAsync(persona);
    }

    public async Task<int> ReemplazarAsync(string codigo, PersonaReemplazo peticion)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!.Trim(),
            ["email"] = peticion.Email!.Trim(),
            ["telefono"] = peticion.Telefono!.Trim()
        };

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    public async Task<int> ActualizarParcialAsync(string codigo, PersonaActualizar peticion)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Nombre != null)
            datos["nombre"] = peticion.Nombre.Trim();
        if (peticion.Email != null)
            datos["email"] = peticion.Email.Trim();
        if (peticion.Telefono != null)
            datos["telefono"] = peticion.Telefono.Trim();

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(codigo.Trim());
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        return filas;
    }
}
