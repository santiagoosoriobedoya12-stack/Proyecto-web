// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;

namespace ApiFacturas.Servicios;

/// <summary>
/// Contrato del servicio de negocio para Empresa.
/// </summary>
public interface IServicioEmpresa
{
    Task<List<Empresa>> ListarAsync(int limite);
    Task<Empresa> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(EmpresaCrear peticion);
    Task<int> ReemplazarAsync(string codigo, EmpresaReemplazo peticion);
    Task<int> ActualizarParcialAsync(string codigo, EmpresaActualizar peticion);
    Task<int> EliminarAsync(string codigo);
}

