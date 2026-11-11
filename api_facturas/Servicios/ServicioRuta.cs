// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación del servicio de negocio para Ruta.
/// </summary>
public class ServicioRuta : IServicioRuta
{
    private readonly IRepositorioRuta _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioRuta(IRepositorioRuta repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Ruta>> ListarAsync(int limite)
    {
        // Regla de negocio: impide solicitar listados con límite cero o negativo
        if (limite <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    // Busca un registro específico por su identificador numérico
    public async Task<Ruta> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        var ruta = await _repositorio.ObtenerPorIdAsync(id);
        if (ruta == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        return ruta;
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<int> CrearAsync(RutaCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
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

    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<int> ReemplazarAsync(int id, RutaReemplazo peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        var datos = new Dictionary<string, object>
        {
            ["ruta"] = peticion.Ruta!.Trim(),
            ["descripcion"] = peticion.Descripcion!.Trim()
        };

        return await _repositorio.ActualizarAsync(id, datos);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarParcialAsync(int id, RutaActualizar peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Ruta != null)
            datos["ruta"] = peticion.Ruta.Trim();
        if (peticion.Descripcion != null)
            datos["descripcion"] = peticion.Descripcion.Trim();

        return await _repositorio.ActualizarAsync(id, datos);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador de la ruta debe ser un entero positivo.");
        }

        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una ruta con id = {id}");
        }

        return filas;
    }
}

