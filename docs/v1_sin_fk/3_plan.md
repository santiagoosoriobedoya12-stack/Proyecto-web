# Plan t├®cnico ÔÇö Versi├│n 1: las seis tablas sin FK (C#/ASP.NET Core + Blazor)

> **Versi├│n 1** ┬À C├ôMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqu├® de cada decisi├│n: [4_research.md](4_research.md) ┬À contratos
> exactos: [6_contracts.md](6_contracts.md) ┬À orden: [8_tasks.md](8_tasks.md).

---

## 1. Stack

| Pieza | Elecci├│n | Por qu├® |
|---|---|---|
| Lenguaje / framework | **C# sobre ASP.NET Core (.NET 10)** | El stack del curso; controladores con atributos, DI integrada, async nativo |
| Acceso a datos | **Dapper** sobre `Microsoft.Data.SqlClient`, con SQL parametrizado a mano | SQL visible ÔÇö Dapper mapea filaÔåÆobjeto pero NO genera SQL (Art. 2, D1) |
| Validaci├│n | **Una petici├│n por verbo** con anotaciones (`[Required]`, `[Range]`ÔÇª) | El framework valida el body contra la petici├│n y responde 422 ÔÇö la petici├│n ES la frontera |
| Motor (v1) | **SQL Server 2022** (contenedor oficial) | El motor natural del ecosistema .NET; los otros llegan en v3/v4 |
| Contenedor de la API | `mcr.microsoft.com/dotnet/sdk:10.0` + `dotnet watch` | Guardar un `.cs` recompila y reinicia solo (ciclo de desarrollo del curso) |

## 2. Estructura de carpetas

```
(ra├¡z del proyecto)
Ôö£ÔöÇÔöÇ docker-compose.yml                # UN comando: postgres + api (crece por versiones)
Ôö£ÔöÇÔöÇ db/
Ôöé   ÔööÔöÇÔöÇ bdfacturas.sql       # la BD completa, PROVISTA (se copia, no se genera)
ÔööÔöÇÔöÇ api_facturas/
    Ôö£ÔöÇÔöÇ ApiFacturas.csproj            # el proyecto .NET (paquetes: Microsoft.Data.SqlClient y Swashbuckle)
    Ôö£ÔöÇÔöÇ Program.cs                    # punto de entrada: ENSAMBLADOR (DI) + 422 + rutas
    Ôö£ÔöÇÔöÇ appsettings.json              # cadena de conexi├│n (default localhost,11463)
    Ôö£ÔöÇÔöÇ Dockerfile                    # sdk:10.0 + dotnet watch (puerto 8032)
    Ôö£ÔöÇÔöÇ Modelos/
    Ôöé   ÔööÔöÇÔöÇ Producto.cs               # el MODELO = la ENTIDAD: 4 propiedades tipadas
    Ôö£ÔöÇÔöÇ Peticiones/
    Ôöé   Ôö£ÔöÇÔöÇ ProductoCrear.cs          # petici├│n del POST (todo obligatorio)
    Ôöé   Ôö£ÔöÇÔöÇ ProductoReemplazo.cs      # petici├│n del PUT (todo obligatorio, sin c├│digo)
    Ôöé   ÔööÔöÇÔöÇ ProductoActualizar.cs     # petici├│n del PATCH (todo opcional)
    Ôö£ÔöÇÔöÇ Controllers/
    Ôöé   ÔööÔöÇÔöÇ ProductoController.cs     # HTTP: atributos de verbo, try/catch ÔåÆ c├│digos
    Ôö£ÔöÇÔöÇ Servicios/
    Ôöé   Ôö£ÔöÇÔöÇ IServicioProducto.cs      # interface del servicio
    Ôöé   ÔööÔöÇÔöÇ ServicioProducto.cs       # reglas de negocio; recibe IRepositorioProducto
    Ôö£ÔöÇÔöÇ Repositorios/
    Ôöé   Ôö£ÔöÇÔöÇ IRepositorioProducto.cs   # interface: 5 m├®todos de datos (async)
    Ôöé   ÔööÔöÇÔöÇ RepositorioProductoSqlServer.cs   # Dapper + SQL a mano parametrizado
    Ôö£ÔöÇÔöÇ Excepciones/
    Ôöé   ÔööÔöÇÔöÇ NoEncontradoExcepcion.cs  # la excepci├│n de negocio que el controller vuelve 404
    ÔööÔöÇÔöÇ pruebas/
        Ôö£ÔöÇÔöÇ PruebaCapas.csproj        # proyecto de consola aparte (criterio 6)
        ÔööÔöÇÔöÇ Programa.cs               # el servicio con un repositorio falso, sin BD
```

### 2bis. El front, que es un proyecto APARTE

```
front_blazor/
Ôö£ÔöÇÔöÇ FrontFacturas.csproj          sin UN SOLO paquete de datos
Ôö£ÔöÇÔöÇ Program.cs                    registra ServicioProducto con su HttpClient
Ôö£ÔöÇÔöÇ Dockerfile                    SDK + dotnet watch, igual que la API
Ôö£ÔöÇÔöÇ appsettings.json              UrlApi (el compose la sobreescribe)
Ôö£ÔöÇÔöÇ Modelos/
Ôöé   ÔööÔöÇÔöÇ Producto.cs               la clase DEL FRONT, no la de la API
Ôö£ÔöÇÔöÇ Servicios/
Ôöé   ÔööÔöÇÔöÇ ServicioProducto.cs       el ├ÜNICO sitio que sabe de HTTP
Ôö£ÔöÇÔöÇ Components/
Ôöé   Ôö£ÔöÇÔöÇ App.razor ┬À Routes.razor ┬À _Imports.razor
Ôöé   Ôö£ÔöÇÔöÇ Layout/   MainLayout ┬À NavMenu
Ôöé   ÔööÔöÇÔöÇ Pages/    Home ┬À Productos
ÔööÔöÇÔöÇ wwwroot/
    Ôö£ÔöÇÔöÇ lib/bootstrap/            SERVIDO DESDE AQUI, nunca por CDN
    ÔööÔöÇÔöÇ app.css                   la capa del proyecto, ENCIMA de Bootstrap
```

**Que el front y la API est├®n los dos en C# no cambia nada**, y hay que
cuidarlo: la tentaci├│n de compartir una clase existe aqu├¡ y no existir├¡a con
dos lenguajes distintos.

| Regla | Por qu├® |
|---|---|
| **El front tiene SU propia clase `Producto`** | Se parece a la de la API porque el **contrato** es el mismo, no porque sea la misma. Una referencia de proyecto atar├¡a los dos procesos |
| **`FrontFacturas.csproj` no tiene Microsoft.Data.SqlClient** | No es un olvido: es la comprobaci├│n de que este proceso **no puede** llegar a SQL Server ni queriendo |
| **Un servicio POR RECURSO** | Hoy `ServicioProducto`. Con doce recursos, doce servicios ÔÇö no un `ApiService` con la tabla como par├ímetro |
| **Bootstrap SI, por CDN NO** | Se sirve desde `wwwroot/lib/bootstrap/`, y `wwwroot/app.css` va **encima** con las clases del dominio (`.tarjeta`, `.campos`, `.acciones`). A igual especificidad gana el ultimo que carga: por eso el orden no es decorativo |

## 3. Arquitectura en capas (flujo de una petici├│n)

```
HTTP ÔåÆ ASP.NET routing        (los atributos [HttpGet]/[HttpPost]ÔÇª deciden el m├®todo)
     ÔåÆ validaci├│n de la PETICI├ôN (anotaciones de la petici├│n del verbo ÔåÆ 422 autom├ítico)
     ÔåÆ ProductoController     (try/catch: traduce excepciones a c├│digos HTTP)
     ÔåÆ IServicioProducto      (interfaz ÔÇö reglas de negocio)
     ÔåÆ IRepositorioProducto   (interfaz ÔÇö el servicio no sabe qu├® motor hay detr├ís)
     ÔåÆ RepositorioProductoSqlServer (Dapper + par├ímetros @)
     ÔåÆ SQL Server
```

## 4. Decisiones de dise├▒o clave

### 4.1 Interfaces de C# desde v1
```csharp
public interface IRepositorioProducto
{
    Task<List<Producto>> ObtenerTodosAsync(int limite);
    Task<Producto?> ObtenerPorCodigoAsync(string codigo);
    Task CrearAsync(Producto producto);
    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);
    Task<int> EliminarAsync(string codigo);
}
```

### 4.2 La validaci├│n vive en las PETICIONES (una por verbo)
- `ProductoCrear`      ÔåÆ POST: todos obligatorios (con c├│digo)
- `ProductoReemplazo`  ÔåÆ PUT: todos obligatorios (el c├│digo va en la URL)
- `ProductoActualizar` ÔåÆ PATCH: todos opcionales (se valida lo que llegue)

### 4.3 El ensamblador: la secci├│n de DI de Program.cs
```csharp
builder.Services.AddScoped<IRepositorioProducto>(
    _ => new RepositorioProductoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();
```

### 4.4 SQL del repositorio (Dapper, siempre parametrizado)
```sql
SELECT codigo, nombre, stock, valorunitario FROM producto ORDER BY codigo LIMIT @limite
SELECT ÔÇª WHERE codigo = @codigo
INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES (@codigo, @nombre, @stock, @valorunitario)
UPDATE producto SET ÔÇª WHERE codigo = @codigo_clave
DELETE FROM producto WHERE codigo = @codigo
```

### 4.5 Traducci├│n de excepciones a HTTP (en el controller)
| Situaci├│n | HTTP |
|---|---|
| (Body con errores de forma) | 422 |
| `ArgumentException` | 400 |
| `NoEncontradoExcepcion` | 404 |
| `Microsoft.Data.SqlClientException` y cualquier otra | 500 (mensaje del motor en `detalle`) |

### 4.6 SQL Server se siembra SOLO
SQL Server ejecuta autom├íticamente los scripts montados en `/docker-entrypoint-initdb.d/` la primera vez.

## 5. Docker: un solo comando desde v1
`docker compose up -d --build` deja corriendo la base, la API y la interfaz gr├ífica.
