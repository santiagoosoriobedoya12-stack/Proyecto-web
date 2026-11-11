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
/// Implementación de la capa de Lógica de Negocio para Producto.
/// Reglas de arquitectura:
/// 1. Recibe 'IRepositorioProducto' por inyección de dependencias (el servicio no sabe si detrás hay SQL Server, Postgres o memoria).
/// 2. Valida reglas semánticas y de negocio: límites válidos, campos obligatorios de negocio, recursos no encontrados.
/// 3. Lanza excepciones de dominio (ArgumentException -> HTTP 400, NoEncontradoExcepcion -> HTTP 404).
/// </summary>
public class ServicioProducto : IServicioProducto
{
    private readonly IRepositorioProducto _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioProducto(IRepositorioProducto repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    /// <summary>
    /// Lista productos aplicando validación del límite.
    /// Si límite <= 0, es una regla de negocio rota -> lanza ArgumentException (HTTP 400).
    /// </summary>
    public async Task<List<Producto>> ListarAsync(int limite)
    {
        // Regla de negocio: impide solicitar listados con límite cero o negativo
        if (limite <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    /// <summary>
    /// Obtiene un producto por su clave.
    /// Si no existe en la base de datos, lanza NoEncontradoExcepcion (HTTP 404).
    /// </summary>
    public async Task<Producto> ObtenerPorCodigoAsync(string codigo)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        var producto = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (producto == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        return producto;
    }

    /// <summary>
    /// Crea un producto convirtiendo la petición (DTO) a la entidad de dominio Producto.
    /// </summary>
    public async Task CrearAsync(ProductoCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var producto = new Producto
        {
            Codigo = peticion.Codigo!.Trim(),
            Nombre = peticion.Nombre!.Trim(),
            Stock = peticion.Stock!.Value,
            Valorunitario = peticion.Valorunitario!.Value
        };

        await _repositorio.CrearAsync(producto);
    }

    /// <summary>
    /// Reemplazo COMPLETO (PUT).
    /// Si el código no existe en la base de datos, lanza NoEncontradoExcepcion (HTTP 404).
    /// </summary>
    public async Task<int> ReemplazarAsync(string codigo, ProductoReemplazo peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        // Verificar existencia previa
        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!.Trim(),
            ["stock"] = peticion.Stock!.Value,
            ["valorunitario"] = peticion.Valorunitario!.Value
        };

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    /// <summary>
    /// Actualización PARCIAL (PATCH).
    /// Si la petición viene sin campos, lanza ArgumentException (HTTP 400).
    /// Si el producto no existe, lanza NoEncontradoExcepcion (HTTP 404).
    /// Solo actualiza los campos efectivamente enviados.
    /// </summary>
    public async Task<int> ActualizarParcialAsync(string codigo, ProductoActualizar peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        // Verificar existencia previa
        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>();

        if (peticion.Nombre != null)
            datos["nombre"] = peticion.Nombre.Trim();

        if (peticion.Stock.HasValue)
            datos["stock"] = peticion.Stock.Value;

        if (peticion.Valorunitario.HasValue)
            datos["valorunitario"] = peticion.Valorunitario.Value;

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    /// <summary>
    /// Elimina un producto. Si no existía, lanza NoEncontradoExcepcion (HTTP 404).
    /// Un segundo DELETE consecutivo lanzará 404 porque las filas afectadas serán 0.
    /// </summary>
    public async Task<int> EliminarAsync(string codigo)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(codigo.Trim());
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        return filas;
    }
}

