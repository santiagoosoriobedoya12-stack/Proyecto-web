// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato del Repositorio para 'usuario-con-roles'.
/// El repositorio se limita a ejecutar los procedimientos almacenados de base de datos.
/// No toma decisiones ni transforma formatos de negocio (regla de capas, 3_plan.md 3.4).
/// </summary>
public interface IRepositorioUsuarioConRoles
{
    Task<List<UsuarioConRoles>> ListarAsync();
    Task<UsuarioConRoles> ConsultarAsync(string email);
    Task<UsuarioConRoles> CrearAsync(string email, string contrasena, string rolesJson);
    Task<UsuarioConRoles> ActualizarAsync(string email, string? contrasena, string rolesJson);
    Task<string> EliminarAsync(string email);
}
