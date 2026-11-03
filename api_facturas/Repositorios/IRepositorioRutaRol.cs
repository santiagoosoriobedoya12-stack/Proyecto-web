using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioRutaRol
{
    Task<List<RutaRol>> ObtenerTodosAsync();
    Task<List<RutaRol>> ObtenerPorRutaAsync(int idRuta);
    Task<List<RutaRol>> ObtenerPorRolAsync(int idRol);
    Task<bool> ExisteAsync(int idRuta, int idRol);
    Task<int> AgregarAsync(int idRuta, int idRol);
    Task<int> EliminarAsync(int idRuta, int idRol);
}
