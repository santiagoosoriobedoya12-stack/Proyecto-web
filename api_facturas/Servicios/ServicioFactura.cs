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
/// Lógica de negocio para Factura.
/// </summary>
public class ServicioFactura : IServicioFactura
{
    private readonly IRepositorioFactura _repositorio;

    // Constructor con inyección de dependencias: recibe la dependencia requerida y protege contra referencias nulas
    public ServicioFactura(IRepositorioFactura repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<List<Factura>> ListarAsync()
    {
        return await _repositorio.ListarAsync();
    }

    // Ejecuta la operación 'ConsultarAsync' dentro de la capa correspondiente
    public async Task<Factura> ConsultarAsync(int numero)
    {
        if (numero <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El número de la factura debe ser un entero positivo.");
        }

        return await _repositorio.ConsultarAsync(numero);
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public async Task<Factura> CrearAsync(FacturaCrear peticion)
    {
        // Verifica que el cuerpo de la petición recibido no sea nulo
        if (peticion == null)
        {
            throw new ArgumentNullException(nameof(peticion));
        }

        if (peticion.Productos == null || peticion.Productos.Count == 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("La factura requiere mínimo 1 producto.");
        }

        return await _repositorio.CrearAsync(peticion);
    }

    // Anula lógicamente la factura en SQL Server devolviendo el stock al inventario
    public async Task<string> AnularAsync(int numero)
    {
        if (numero <= 0)
        {
            // Lanza error de validación de argumento (se traduce a HTTP 400 Bad Request)
            throw new ArgumentException("El número de la factura debe ser un entero positivo.");
        }

        return await _repositorio.AnularAsync(numero);
    }
}
