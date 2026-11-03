using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioRutaRol : IServicioRutaRol
{
    private readonly IRepositorioRutaRol _repositorio;

    public ServicioRutaRol(IRepositorioRutaRol repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<RutaRol>> ObtenerTodosAsync()
    {
        return await _repositorio.ObtenerTodosAsync();
    }

    public async Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta)
    {
        if (idRuta <= 0)
            throw new ArgumentException("El id de la ruta debe ser un entero positivo.");

        return await _repositorio.ObtenerPorRutaAsync(idRuta);
    }

    public async Task<List<RutaRol>> ObtenerPorRolAsync(int idRol)
    {
        if (idRol <= 0)
            throw new ArgumentException("El id del rol debe ser un entero positivo.");

        return await _repositorio.ObtenerPorRolAsync(idRol);
    }

    public async Task AgregarAsync(RutaRolCrear peticion)
    {
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        var idRuta = peticion.Fkidruta!.Value;
        var idRol = peticion.Fkidrol!.Value;

        var yaExiste = await _repositorio.ExisteAsync(idRuta, idRol);
        if (yaExiste)
        {
            throw new ConflictoExcepcion($"El permiso de la ruta #{idRuta} para el rol #{idRol} ya existe.");
        }

        await _repositorio.AgregarAsync(idRuta, idRol);
    }

    public async Task EliminarAsync(int idRuta, int idRol)
    {
        if (idRuta <= 0 || idRol <= 0)
            throw new ArgumentException("Los identificadores de ruta y rol deben ser enteros positivos.");

        var filas = await _repositorio.EliminarAsync(idRuta, idRol);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la relación entre la ruta #{idRuta} y el rol #{idRol}.");
        }
    }
}
