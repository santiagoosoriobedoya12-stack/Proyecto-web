// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

// Servicio 'ServicioRolUsuario': encapsula validaciones de dominio o llamadas HTTP tipadas hacia la API
public class ServicioRolUsuario : IServicioRolUsuario
{
    private readonly IRepositorioRolUsuario _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
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
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
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

    // Ejecuta la operación 'AgregarAsync' dentro de la capa correspondiente
    public async Task AgregarAsync(RolUsuarioCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        var email = peticion.Fkemail!.Trim();
        var idRol = peticion.Fkidrol!.Value;

        var yaExiste = await _repositorio.ExisteAsync(email, idRol);
        if (yaExiste)
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion($"El usuario '{email}' ya tiene asignado el rol #{idRol}.");
        }

        await _repositorio.AgregarAsync(email, idRol);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task EliminarAsync(string email, int idRol)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        if (idRol <= 0)
            throw new ArgumentException("El id del rol debe ser un entero positivo.");

        var filas = await _repositorio.EliminarAsync(email.Trim(), idRol);
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe la relación entre el usuario '{email}' y el rol #{idRol}.");
        }
    }
}
