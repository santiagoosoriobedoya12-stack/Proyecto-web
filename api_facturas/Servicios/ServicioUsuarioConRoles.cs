// Serializador oficial System.Text.Json para leer y construir cuerpos JSON
using System.Text.Json;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Lógica de negocio para 'usuario-con-roles'.
/// 
/// TRABAJO PROPIO DEL SERVICIO (3_plan.md Sección 3.4):
/// Traduce la lista de enteros del formulario [1, 3] al JSON que el procedimiento espera:
/// [{"fkidrol":1},{"fkidrol":3}].
/// La clave obligatoria es 'fkidrol' para que OPENJSON la encuentre correctamente.
/// Esta transformación reside en el servicio porque la forma del JSON es una regla del dominio.
/// </summary>
public class ServicioUsuarioConRoles : IServicioUsuarioConRoles
{
    private readonly IRepositorioUsuarioConRoles _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioUsuarioConRoles(IRepositorioUsuarioConRoles repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<UsuarioConRoles>> ListarAsync()
    {
        return await _repositorio.ListarAsync();
    }

    // Ejecuta la operación 'ConsultarAsync' dentro de la capa correspondiente
    public async Task<UsuarioConRoles> ConsultarAsync(string email)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        return await _repositorio.ConsultarAsync(email.Trim());
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<UsuarioConRoles> CrearAsync(UsuarioConRolesCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        // Regla 3.4: Traducir la lista de enteros [1, 3] al JSON [{"fkidrol":1},{"fkidrol":3}]
        var rolesJson = JsonSerializer.Serialize(peticion.Roles.Select(id => new { fkidrol = id }));

        return await _repositorio.CrearAsync(peticion.Email!.Trim(), peticion.Contrasena!, rolesJson);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public async Task<UsuarioConRoles> ActualizarAsync(string email, UsuarioConRolesActualizar peticion)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        // Regla 3.4: Traducir lista de roles a JSON con la propiedad 'fkidrol'
        var rolesJson = JsonSerializer.Serialize(peticion.Roles.Select(id => new { fkidrol = id }));

        return await _repositorio.ActualizarAsync(email.Trim(), peticion.Contrasena, rolesJson);
    }

    // Elimina el registro especificado validando su existencia previa
    public async Task EliminarAsync(string email)
    {
        // Valida que el identificador o cadena requerida no llegue nula ni vacía
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        await _repositorio.EliminarAsync(email.Trim());
    }
}
