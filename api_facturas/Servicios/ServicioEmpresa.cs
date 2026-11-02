using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Implementación de reglas de negocio para Empresa.
/// </summary>
public class ServicioEmpresa : IServicioEmpresa
{
    private readonly IRepositorioEmpresa _repositorio;

    public ServicioEmpresa(IRepositorioEmpresa repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Empresa>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro 'limite' debe ser un número entero mayor a 0.");
        }

        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Empresa> ObtenerPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la empresa no puede estar vacío.");
        }

        var empresa = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (empresa == null)
        {
            throw new NoEncontradoExcepcion($"No existe una empresa con codigo = {codigo.Trim()}");
        }

        return empresa;
    }

    public async Task CrearAsync(EmpresaCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var empresa = new Empresa
        {
            Codigo = peticion.Codigo!.Trim(),
            Nombre = peticion.Nombre!.Trim()
        };

        await _repositorio.CrearAsync(empresa);
    }

    public async Task<int> ReemplazarAsync(string codigo, EmpresaReemplazo peticion)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la empresa no puede estar vacío.");
        }

        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una empresa con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!.Trim()
        };

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    public async Task<int> ActualizarParcialAsync(string codigo, EmpresaActualizar peticion)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la empresa no puede estar vacío.");
        }

        if (peticion == null || !peticion.TieneCamposParaActualizar())
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo.Trim());
        if (existe == null)
        {
            throw new NoEncontradoExcepcion($"No existe una empresa con codigo = {codigo.Trim()}");
        }

        var datos = new Dictionary<string, object>();
        if (peticion.Nombre != null)
        {
            datos["nombre"] = peticion.Nombre.Trim();
        }

        return await _repositorio.ActualizarAsync(codigo.Trim(), datos);
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la empresa no puede estar vacío.");
        }

        var filas = await _repositorio.EliminarAsync(codigo.Trim());
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe una empresa con codigo = {codigo.Trim()}");
        }

        return filas;
    }
}
