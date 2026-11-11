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
/// Lógica de negocio para Vendedor.
/// </summary>
public class ServicioVendedor : IServicioVendedor
{
    private readonly IRepositorioVendedor _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioVendedor(IRepositorioVendedor repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Vendedor>> ListarAsync(int limite)
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
    public async Task<Vendedor> ObtenerPorIdAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        var vendedor = await _repositorio.ObtenerPorIdAsync(id);
        if (vendedor == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        return vendedor;
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<int> CrearAsync(VendedorCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
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

    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<int> ReemplazarAsync(int id, VendedorReemplazo peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
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

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarParcialAsync(int id, VendedorActualizar peticion)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposModificados())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
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

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(int id)
    {
        if (id <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El identificador del vendedor debe ser un entero positivo.");
        }

        var existe = await _repositorio.ObtenerPorIdAsync(id);
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un vendedor con id = {id}");
        }

        return await _repositorio.EliminarAsync(id);
    }
}

