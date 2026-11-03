using System.Text.Json;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
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

    public ServicioUsuarioConRoles(IRepositorioUsuarioConRoles repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<UsuarioConRoles>> ListarAsync()
    {
        return await _repositorio.ListarAsync();
    }

    public async Task<UsuarioConRoles> ConsultarAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        return await _repositorio.ConsultarAsync(email.Trim());
    }

    public async Task<UsuarioConRoles> CrearAsync(UsuarioConRolesCrear peticion)
    {
        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        // Regla 3.4: Traducir la lista de enteros [1, 3] al JSON [{"fkidrol":1},{"fkidrol":3}]
        var rolesJson = JsonSerializer.Serialize(peticion.Roles.Select(id => new { fkidrol = id }));

        return await _repositorio.CrearAsync(peticion.Email!.Trim(), peticion.Contrasena!, rolesJson);
    }

    public async Task<UsuarioConRoles> ActualizarAsync(string email, UsuarioConRolesActualizar peticion)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        if (peticion == null)
            throw new ArgumentNullException(nameof(peticion));

        // Regla 3.4: Traducir lista de roles a JSON con la propiedad 'fkidrol'
        var rolesJson = JsonSerializer.Serialize(peticion.Roles.Select(id => new { fkidrol = id }));

        return await _repositorio.ActualizarAsync(email.Trim(), peticion.Contrasena, rolesJson);
    }

    public async Task EliminarAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es obligatorio.");

        await _repositorio.EliminarAsync(email.Trim());
    }
}
