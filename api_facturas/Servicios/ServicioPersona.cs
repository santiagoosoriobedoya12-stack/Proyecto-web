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
/// Implementación de la lógica de negocio para Persona.
/// </summary>
public class ServicioPersona : IServicioPersona
{
    private readonly IRepositorioPersona _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioPersona(IRepositorioPersona repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Persona>> ListarAsync(int limite)
    {
        // Regla de negocio: impide solicitar listados con límite cero o negativo
        if (limite <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    // Busca un registro específico por su código único
    public async Task<Persona> ObtenerPorCodigoAsync(string codigo)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        var persona = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (persona == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        return persona;
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task CrearAsync(PersonaCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
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

    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<int> ReemplazarAsync(string codigo, PersonaReemplazo peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
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

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarParcialAsync(string codigo, PersonaActualizar peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
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

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(string codigo)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(codigo))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El código de la persona no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(codigo.Trim());
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe una persona con codigo = {codigo.Trim()}");
        }

        return filas;
    }
}

