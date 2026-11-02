using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
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
        if (limite <= 0)
        {
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
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        var producto = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (producto == null)
        {
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        return producto;
    }

    /// <summary>
    /// Crea un producto convirtiendo la petición (DTO) a la entidad de dominio Producto.
    /// </summary>
    public async Task CrearAsync(ProductoCrear peticion)
    {
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
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        // Verificar existencia previa
        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
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
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        // Verificar existencia previa
        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
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
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del producto no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(codigo.Trim());
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un producto con codigo = {codigo.Trim()}");
        }

        return filas;
    }
}

