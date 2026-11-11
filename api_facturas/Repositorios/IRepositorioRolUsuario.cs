// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato de repositorio para la tabla puente 'rol_usuario'.
/// Soporta consultas por ambos lados de la relación y borrado con ambas claves.
/// </summary>
public interface IRepositorioRolUsuario
{
    Task<List<RolUsuario>> ObtenerTodosAsync();
    Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string email);
    Task<List<RolUsuario>> ObtenerPorRolAsync(int idRol);
    Task<bool> ExisteAsync(string email, int idRol);
    Task<int> AgregarAsync(string email, int idRol);
    Task<int> EliminarAsync(string email, int idRol);
}
