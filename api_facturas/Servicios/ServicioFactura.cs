using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

/// <summary>
/// Lógica de negocio para Factura.
/// </summary>
public class ServicioFactura : IServicioFactura
{
    private readonly IRepositorioFactura _repositorio;

    public ServicioFactura(IRepositorioFactura repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Factura>> ListarAsync()
    {
        return await _repositorio.ListarAsync();
    }

    public async Task<Factura> ConsultarAsync(int numero)
    {
        if (numero <= 0)
        {
            throw new ArgumentException("El número de la factura debe ser un entero positivo.");
        }

        return await _repositorio.ConsultarAsync(numero);
    }

    public async Task<Factura> CrearAsync(FacturaCrear peticion)
    {
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        if (peticion.Productos == null || peticion.Productos.Count == 0)
        {
            throw new ArgumentException("La factura requiere mínimo 1 producto.");
        }

        return await _repositorio.CrearAsync(peticion);
    }

    public async Task<string> AnularAsync(int numero)
    {
        if (numero <= 0)
        {
            throw new ArgumentException("El número de la factura debe ser un entero positivo.");
        }

        return await _repositorio.AnularAsync(numero);
    }
}
