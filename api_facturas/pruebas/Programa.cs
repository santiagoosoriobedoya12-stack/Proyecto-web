// Excepciones tipadas de dominio (NoEncontradoExcepcion -> 404, ConflictoExcepcion -> 409)
using ApiFacturas.Excepciones;
// Entidades del dominio que representan las tablas de la base de datos
using ApiFacturas.Modelos;
// DTOs de entrada segregados por verbo HTTP (Crear, Reemplazo, Actualizar)
using ApiFacturas.Peticiones;
// Contratos e implementaciones de acceso a datos con Dapper
using ApiFacturas.Repositorios;
// Contratos e implementaciones de lógica y reglas de negocio
using ApiFacturas.Servicios;

namespace ApiFacturas.Pruebas;

/// <summary>
/// Repositorio Falso en Memoria para Producto.
/// Permite probar ServicioProducto sin base de datos real.
/// </summary>
public class RepositorioProductoFalso : IRepositorioProducto
{
    private readonly Dictionary<string, Producto> _tabla = new(StringComparer.OrdinalIgnoreCase);

    public Task<List<Producto>> ObtenerTodosAsync(int limite)
    {
        var lista = _tabla.Values.OrderBy(p => p.Codigo).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    // Busca un registro específico por su código único
    public Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        _tabla.TryGetValue(codigo, out var producto);
        return Task.FromResult(producto);
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public Task CrearAsync(Producto producto)
    {
        if (_tabla.ContainsKey(producto.Codigo))
        {
            // Lanza error cuando la operación entra en conflicto con el estado actual (HTTP 409)
            throw new InvalidOperationException($"Clave duplicada: {producto.Codigo}");
        }

        _tabla[producto.Codigo] = new Producto
        {
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Stock = producto.Stock,
            Valorunitario = producto.Valorunitario
        };
        return Task.CompletedTask;
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (!_tabla.TryGetValue(codigo, out var producto))
        {
            return Task.FromResult(0);
        }

        foreach (var par in datos)
        {
            switch (par.Key.ToLower())
            {
                case "nombre":
                    producto.Nombre = (string)par.Value;
                    break;
                case "stock":
                    producto.Stock = Convert.ToInt32(par.Value);
                    break;
                case "valorunitario":
                    producto.Valorunitario = Convert.ToDecimal(par.Value);
                    break;
            }
        }

        return Task.FromResult(1);
    }

    // Elimina el registro especificado validando su existencia previa
    public Task<int> EliminarAsync(string codigo)
    {
        return Task.FromResult(_tabla.Remove(codigo) ? 1 : 0);
    }
}

/// <summary>
/// Repositorio Falso en Memoria para Cliente (Versión 2).
/// Simula la generación de IDENTITY(1,1) y las claves foráneas en memoria.
/// </summary>
public class RepositorioClienteFalso : IRepositorioCliente
{
    private readonly Dictionary<int, Cliente> _tabla = new();
    private int _contadorId = 1;

    public Task<List<Cliente>> ObtenerTodosAsync(int limite)
    {
        var lista = _tabla.Values.OrderBy(c => c.Id).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    // Busca un registro específico por su identificador numérico
    public Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        _tabla.TryGetValue(id, out var cliente);
        return Task.FromResult(cliente);
    }

    // Valida los datos de entrada y registra una nueva entidad en el sistema
    public Task<int> CrearAsync(Cliente cliente)
    {
        var id = _contadorId++;
        var nuevo = new Cliente
        {
            Id = id,
            Credito = cliente.Credito,
            Fkcodpersona = cliente.Fkcodpersona,
            Fkcodempresa = cliente.Fkcodempresa
        };
        _tabla[id] = nuevo;
        return Task.FromResult(id);
    }

    // Actualiza parcialmente solo los campos modificados enviados (operación PATCH)
    public Task<int> ActualizarAsync(int id, Dictionary<string, object?> datos)
    {
        if (!_tabla.TryGetValue(id, out var cliente))
            return Task.FromResult(0);

        foreach (var par in datos)
        {
            switch (par.Key.ToLower())
            {
                case "credito":
                    cliente.Credito = Convert.ToDecimal(par.Value);
                    break;
                case "fkcodpersona":
                    cliente.Fkcodpersona = (string)par.Value!;
                    break;
                case "fkcodempresa":
                    cliente.Fkcodempresa = (string?)par.Value;
                    break;
            }
        }

        return Task.FromResult(1);
    }

    // Elimina el registro especificado validando su existencia previa
    public Task<int> EliminarAsync(int id)
    {
        return Task.FromResult(_tabla.Remove(id) ? 1 : 0);
    }
}

/// <summary>
/// Programa ejecutable para probar el desacoplamiento de capas (Criterio 10 de la Versión 2).
/// Verifica que los servicios de negocio funcionan al 100% con repositorios falsos sin SQL Server.
/// </summary>
public class Programa
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("PRUEBA DE CAPAS — VERSIÓN 2 (Criterio de Aceptación 10)");
        Console.WriteLine("Verificando Servicios con Repositorios Falsos en Memoria");
        Console.WriteLine("==================================================");

        // --- 1. Prueba ServicioProducto ---
        Console.WriteLine("\n[1] Probando ServicioProducto...");
        var repoProd = new RepositorioProductoFalso();
        var servicioProd = new ServicioProducto(repoProd);

        await servicioProd.CrearAsync(new ProductoCrear
        {
            Codigo = "PR001",
            Nombre = "Laptop de Prueba",
            Stock = 10,
            Valorunitario = 2000000m
        });
        Console.WriteLine("✔ Crear producto exitoso.");

        var prod = await servicioProd.ObtenerPorCodigoAsync("PR001");
        if (prod.Nombre != "Laptop de Prueba") throw new Exception("Fallo en producto.");
        Console.WriteLine("✔ Consulta de producto exitosa.");

        // --- 2. Prueba ServicioCliente (Versión 2 con FK) ---
        Console.WriteLine("\n[2] Probando ServicioCliente (Versión 2)...");
        var repoCli = new RepositorioClienteFalso();
        var servicioCli = new ServicioCliente(repoCli);

        // Crear cliente persona natural (fkcodempresa = null)
        var idNatural = await servicioCli.CrearAsync(new ClienteCrear
        {
            Credito = 5000000m,
            Fkcodpersona = "CC1001",
            Fkcodempresa = null
        });
        Console.WriteLine($"✔ Cliente persona natural creado con ID #{idNatural}.");

        var cliente1 = await servicioCli.ObtenerPorIdAsync(idNatural);
        if (cliente1.Fkcodempresa != null || cliente1.Credito != 5000000m)
            throw new Exception("Fallo al validar cliente persona natural.");
        Console.WriteLine("✔ Verificación de campos y nulabilidad correcta.");

        // Reemplazo completo (PUT)
        await servicioCli.ReemplazarAsync(idNatural, new ClienteReemplazo
        {
            Credito = 7500000m,
            Fkcodpersona = "CC1001",
            Fkcodempresa = "EMP01"
        });
        var modificado = await servicioCli.ObtenerPorIdAsync(idNatural);
        if (modificado.Credito != 7500000m || modificado.Fkcodempresa != "EMP01")
            throw new Exception("Fallo en PUT de cliente.");
        Console.WriteLine("✔ Reemplazo completo (PUT) funcionó correctamente.");

        // Actualización parcial (PATCH)
        await servicioCli.ActualizarParcialAsync(idNatural, new ClienteActualizar
        {
            Credito = 9000000m
        });
        var parcial = await servicioCli.ObtenerPorIdAsync(idNatural);
        if (parcial.Credito != 9000000m || parcial.Fkcodempresa != "EMP01")
            throw new Exception("Fallo en PATCH de cliente.");
        Console.WriteLine("✔ Actualización parcial (PATCH) funcionó correctamente.");

        // Eliminación
        await servicioCli.EliminarAsync(idNatural);
        // Bloque try: ejecuta la operación capturando excepciones de negocio, SQL o red para responder adecuadamente
        try
        {
            await servicioCli.ObtenerPorIdAsync(idNatural);
            throw new Exception("Debió fallar con 404.");
        }
        // Captura recurso no encontrado y responde código HTTP 404 Not Found
        catch (NoEncontradoExcepcion)
        {
            Console.WriteLine("✔ Eliminación y verificación 404 correcta.");
        }

        Console.WriteLine("\n==================================================");
        Console.WriteLine("CRITERIO 10 OK: Toda la lógica de negocio opera desacoplada de la base de datos.");
        Console.WriteLine("==================================================");
    }
}
