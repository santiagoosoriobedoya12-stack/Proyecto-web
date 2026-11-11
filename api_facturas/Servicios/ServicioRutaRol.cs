// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

// Servicio 'ServicioRutaRol': encapsula validaciones de dominio o llamadas HTTP tipadas hacia la API
public class ServicioRutaRol : IServicioRutaRol
{
    private readonly IRepositorioRutaRol _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
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

    // Ejecuta la operación 'AgregarAsync' dentro de la capa correspondiente
    public async Task AgregarAsync(RutaRolCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        var idRuta = peticion.Fkidruta!.Value;
        var idRol = peticion.Fkidrol!.Value;

        var yaExiste = await _repositorio.ExisteAsync(idRuta, idRol);
        if (yaExiste)
        {
            // Lanza excepción de conflicto relacional o duplicidad (se traduce a HTTP 409)
            throw new ConflictoExcepcion($"El permiso de la ruta #{idRuta} para el rol #{idRol} ya existe.");
        }

        await _repositorio.AgregarAsync(idRuta, idRol);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task EliminarAsync(int idRuta, int idRol)
    {
        if (idRuta <= 0 || idRol <= 0)
            throw new ArgumentException("Los identificadores de ruta y rol deben ser enteros positivos.");

        var filas = await _repositorio.EliminarAsync(idRuta, idRol);
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe la relación entre la ruta #{idRuta} y el rol #{idRol}.");
        }
    }
}
