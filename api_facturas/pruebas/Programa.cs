using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Repositorios;
using ApiFacturas.Servicios;

namespace ApiFacturas.Pruebas;

/// <summary>
/// Repositorio Falso en Memoria (Mock / InMemory Repository).
/// Implementa IRepositorioProducto guardando los datos en un diccionario local.
/// Esta clase es la demostración científica del desacoplamiento de capas:
/// Permite probar toda la lógica de negocio del ServicioProducto SIN necesitar SQL Server ni Docker.
/// </summary>
public class RepositorioProductoFalso : IRepositorioProducto
{
    private readonly Dictionary<string, Producto> _tabla = new(StringComparer.OrdinalIgnoreCase);

    public Task<List<Producto>> ObtenerTodosAsync(int limite)
    {
        var lista = _tabla.Values.OrderBy(p => p.Codigo).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    public Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        _tabla.TryGetValue(codigo, out var producto);
        return Task.FromResult(producto);
    }

    public Task CrearAsync(Producto producto)
    {
        if (_tabla.ContainsKey(producto.Codigo))
        {
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

    public Task<int> EliminarAsync(string codigo)
    {
        return Task.FromResult(_tabla.Remove(codigo) ? 1 : 0);
    }
}

/// <summary>
/// Programa ejecutable para probar el Criterio 6 de la especificación:
/// Verifica que el servicio funciona al 100% con un repositorio falso en memoria.
/// </summary>
public class Programa
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("Iniciando Prueba de Capas (Criterio de Aceptación 6)");
        Console.WriteLine("Ejecutando ServicioProducto con Repositorio Falso en Memoria...");
        Console.WriteLine("==================================================");

        var repoFalso = new RepositorioProductoFalso();
        var servicio = new ServicioProducto(repoFalso);

        // 1. Crear producto
        await servicio.CrearAsync(new ProductoCrear
        {
            Codigo = "PR001",
            Nombre = "Laptop de Prueba",
            Stock = 10,
            Valorunitario = 2000000m
        });
        Console.WriteLine("✔ 1. Crear producto funcionó correctamente.");

        // 2. Listar
        var lista = await servicio.ListarAsync(10);
        if (lista.Count != 1 || lista[0].Codigo != "PR001")
            throw new Exception("Fallo en listar productos.");
        Console.WriteLine("✔ 2. Listar productos funcionó correctamente.");

        // 3. Obtener por código
        var prod = await servicio.ObtenerPorCodigoAsync("PR001");
        if (prod.Nombre != "Laptop de Prueba")
            throw new Exception("Fallo en obtener producto por código.");
        Console.WriteLine("✔ 3. Obtener por código funcionó correctamente.");

        // 4. Reemplazo completo (PUT)
        await servicio.ReemplazarAsync("PR001", new ProductoReemplazo
        {
            Nombre = "Laptop Modificada",
            Stock = 15,
            Valorunitario = 2200000m
        });
        var prodModificado = await servicio.ObtenerPorCodigoAsync("PR001");
        if (prodModificado.Nombre != "Laptop Modificada" || prodModificado.Stock != 15)
            throw new Exception("Fallo en reemplazo completo.");
        Console.WriteLine("✔ 4. Reemplazar completo (PUT) funcionó correctamente.");

        // 5. Actualización parcial (PATCH)
        await servicio.ActualizarParcialAsync("PR001", new ProductoActualizar
        {
            Stock = 99
        });
        var prodParcial = await servicio.ObtenerPorCodigoAsync("PR001");
        if (prodParcial.Stock != 99 || prodParcial.Nombre != "Laptop Modificada")
            throw new Exception("Fallo en actualización parcial.");
        Console.WriteLine("✔ 5. Actualizar parcial (PATCH) funcionó correctamente.");

        // 6. Validar excepciones de negocio
        try
        {
            await servicio.ListarAsync(0);
            throw new Exception("Debió fallar con límite <= 0.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✔ 6a. Límite <= 0 rechazado correctamente con ArgumentException.");
        }

        try
        {
            await servicio.ObtenerPorCodigoAsync("INEXISTENTE");
            throw new Exception("Debió fallar con código inexistente.");
        }
        catch (NoEncontradoExcepcion)
        {
            Console.WriteLine("✔ 6b. Recurso inexistente rechazado correctamente con NoEncontradoExcepcion.");
        }

        // 7. Eliminar
        await servicio.EliminarAsync("PR001");
        try
        {
            await servicio.EliminarAsync("PR001"); // Segundo borrado debe fallar
            throw new Exception("El segundo borrado debió fallar con 404.");
        }
        catch (NoEncontradoExcepcion)
        {
            Console.WriteLine("✔ 7. Eliminar y segundo eliminar rechazado con NoEncontradoExcepcion.");
        }

        Console.WriteLine("==================================================");
        Console.WriteLine("CRITERIO 6 OK: El servicio funciona con el repositorio falso, sin SQL Server.");
        Console.WriteLine("==================================================");
    }
}

