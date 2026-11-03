namespace ApiFacturas.Repositorios;

/// <summary>
/// Contrato del repositorio de acceso y permisos RBAC para la Versión 3.
/// Orquesta la consulta del Stored Procedure verificar_acceso_ruta y los roles/rutas de los usuarios.
/// </summary>
public interface IRepositorioAcceso
{
    /// <summary>
    /// Consulta el procedimiento almacenado verificar_acceso_ruta en SQL Server.
    /// Recibe el email y el id de la ruta y retorna si el usuario tiene permiso (true/false).
    /// </summary>
    Task<bool> VerificarAccesoAsync(string email, int idRuta);

    /// <summary>
    /// Busca el identificador numérico de una ruta a partir de su nombre/patrón (ej: 'interfaz.usuarios').
    /// Retorna null si la ruta no existe en la base de datos (fallando cerrado).
    /// </summary>
    Task<int?> ObtenerIdRutaPorNombreAsync(string nombreRuta);

    /// <summary>
    /// Retorna los nombres de los roles asignados a un usuario a través de la tabla rol_usuario.
    /// </summary>
    Task<List<string>> ObtenerRolesPorEmailAsync(string email);

    /// <summary>
    /// Retorna la lista de rutas autorizadas para el usuario cruzando usuario -> rol_usuario -> rutarol -> ruta.
    /// Utilizado por GET /api/permisos/mios para armar el menú en la interfaz gráfica.
    /// </summary>
    Task<List<string>> ObtenerRutasPermitidasPorEmailAsync(string email);
}

