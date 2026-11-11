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
/// Implementación de la lógica de negocio para Usuario.
/// </summary>
public class ServicioUsuario : IServicioUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Usuario>> ListarAsync(int limite)
    {
        // Regla de negocio: impide solicitar listados con límite cero o negativo
        if (limite <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    // Busca un registro específico por su correo electrónico
    public async Task<Usuario> ObtenerPorEmailAsync(string email)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        var usuario = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (usuario == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        return usuario;
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task CrearAsync(UsuarioCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        await _repositorio.CrearAsync(peticion.Email!.Trim(), peticion.Contrasena!);
    }

    // Reemplaza completamente los datos de un registro existente (operación PUT idempotente)
    public async Task<int> ReemplazarAsync(string email, UsuarioReemplazo peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        var datos = new Dictionary<string, object>
        {
            ["contrasena"] = peticion.Contrasena!
        };

        return await _repositorio.ActualizarAsync(email.Trim(), datos);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<int> ActualizarParcialAsync(string email, UsuarioActualizar peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorEmailAsync(email.Trim());
        if (existe == null)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Contrasena != null)
        {
            datos["contrasena"] = peticion.Contrasena;
        }

        return await _repositorio.ActualizarAsync(email.Trim(), datos);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task<int> EliminarAsync(string email)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El email del usuario no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(email.Trim());
        if (filas == 0)
        {
            // Lanza excepción de dominio cuando el registro no existe (se traduce a HTTP 404)
            throw new NoEncontradoExcepcion($"No existe un usuario con email = {email.Trim()}");
        }

        return filas;
    }
}

