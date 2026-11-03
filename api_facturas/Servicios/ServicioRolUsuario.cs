using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioRolUsuario : IServicioRolUsuario
{
    private readonly IRepositorioRolUsuario _repositorio;

    public ServicioRolUsuario(IRepositorioRolUsuario repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<RolUsuario>> ObtenerTodosAsync()
    {
        return await _repositorio.ObtenerTodosAsync();
    }

    public async Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        return await _repositorio.ObtenerPorUsuarioAsync(email.Trim());
    }

    public async Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol)
    {
        if (idRol <= 0)
            throw new ArgumentException("El id del rol debe ser un entero positivo.");

        return await _repositorio.ObtenerPorRolAsync(idRol);
    }

    public async Task AgregarAsync(RolUsuarioCrear peticion)
    {
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        var email = peticion.Fkemail!.Trim();
        var idRol = peticion.Fkidrol!.Value;

        var yaExiste = await _repositorio.ExisteAsync(email, idRol);
        if (yaExiste)
        {
            throw new ConflictoExcepcion($"El usuario '{email}' ya tiene asignado el rol #{idRol}.");
        }

        await _repositorio.AgregarAsync(email, idRol);
    }

    public async Task EliminarAsync(string email, int idRol)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        if (idRol <= 0)
            throw new ArgumentException("El id del rol debe ser un entero positivo.");

        var filas = await _repositorio.EliminarAsync(email.Trim(), idRol);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la relación entre el usuario '{email}' y el rol #{idRol}.");
        }
    }
}
