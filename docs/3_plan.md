# Plan técnico — Versión 1: las seis tablas sin FK (C#/ASP.NET Core + Blazor)

> **Versión 1** · CÓMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqué de cada decisión: [4_research.md](4_research.md) · contratos
> exactos: [6_contracts.md](6_contracts.md) · orden: [8_tasks.md](8_tasks.md).

---

## 1. Stack

| Pieza | Elección | Por qué |
|---|---|---|
| Lenguaje / framework | **C# sobre ASP.NET Core (.NET 10)** | El stack del curso; controladores con atributos, DI integrada, async nativo |
| Acceso a datos | **Dapper** sobre `Microsoft.Data.SqlClient`, con SQL parametrizado a mano | SQL visible — Dapper mapea fila→objeto pero NO genera SQL (Art. 2, D1) |
| Validación | **Una petición por verbo** con anotaciones (`[Required]`, `[Range]`…) | El framework valida el body contra la petición y responde 422 — la petición ES la frontera |
| Motor (v1) | **SQL Server 2022** (contenedor oficial) | El motor natural del ecosistema .NET; los otros llegan en v3/v4 |
| Contenedor de la API | `mcr.microsoft.com/dotnet/sdk:10.0` + `dotnet watch` | Guardar un `.cs` recompila y reinicia solo (ciclo de desarrollo del curso) |

## 2. Estructura de carpetas

```
(raíz del proyecto)
├── docker-compose.yml                # UN comando: postgres + api (crece por versiones)
├── db/
│   └── bdfacturas.sql       # la BD completa, PROVISTA (se copia, no se genera)
└── api_facturas/
    ├── ApiFacturas.csproj            # el proyecto .NET (paquetes: Microsoft.Data.SqlClient y Swashbuckle)
    ├── Program.cs                    # punto de entrada: ENSAMBLADOR (DI) + 422 + rutas
    ├── appsettings.json              # cadena de conexión (default localhost,11463)
    ├── Dockerfile                    # sdk:10.0 + dotnet watch (puerto 8032)
    ├── Modelos/
    │   └── Producto.cs               # el MODELO = la ENTIDAD: 4 propiedades tipadas
    ├── Peticiones/
    │   ├── ProductoCrear.cs          # petición del POST (todo obligatorio)
    │   ├── ProductoReemplazo.cs      # petición del PUT (todo obligatorio, sin código)
    │   └── ProductoActualizar.cs     # petición del PATCH (todo opcional)
    ├── Controllers/
    │   └── ProductoController.cs     # HTTP: atributos de verbo, try/catch → códigos
    ├── Servicios/
    │   ├── IServicioProducto.cs      # interface del servicio
    │   └── ServicioProducto.cs       # reglas de negocio; recibe IRepositorioProducto
    ├── Repositorios/
    │   ├── IRepositorioProducto.cs   # interface: 5 métodos de datos (async)
    │   └── RepositorioProductoSqlServer.cs   # Dapper + SQL a mano parametrizado
    ├── Excepciones/
    │   └── NoEncontradoExcepcion.cs  # la excepción de negocio que el controller vuelve 404
    └── pruebas/
        ├── PruebaCapas.csproj        # proyecto de consola aparte (criterio 6)
        └── Programa.cs               # el servicio con un repositorio falso, sin BD
```

### 2bis. El front, que es un proyecto APARTE

```
front_blazor/
├── FrontFacturas.csproj          sin UN SOLO paquete de datos
├── Program.cs                    registra ServicioProducto con su HttpClient
├── Dockerfile                    SDK + dotnet watch, igual que la API
├── appsettings.json              UrlApi (el compose la sobreescribe)
├── Modelos/
│   └── Producto.cs               la clase DEL FRONT, no la de la API
├── Servicios/
│   └── ServicioProducto.cs       el ÚNICO sitio que sabe de HTTP
├── Components/
│   ├── App.razor · Routes.razor · _Imports.razor
│   ├── Layout/   MainLayout · NavMenu
│   └── Pages/    Home · Productos
└── wwwroot/
    ├── lib/bootstrap/            SERVIDO DESDE AQUI, nunca por CDN
    └── app.css                   la capa del proyecto, ENCIMA de Bootstrap
```

**Que el front y la API estén los dos en C# no cambia nada**, y hay que
cuidarlo: la tentación de compartir una clase existe aquí y no existiría con
dos lenguajes distintos.

| Regla | Por qué |
|---|---|
| **El front tiene SU propia clase `Producto`** | Se parece a la de la API porque el **contrato** es el mismo, no porque sea la misma. Una referencia de proyecto ataría los dos procesos |
| **`FrontFacturas.csproj` no tiene Microsoft.Data.SqlClient** | No es un olvido: es la comprobación de que este proceso **no puede** llegar a SQL Server ni queriendo |
| **Un servicio POR RECURSO** | Hoy `ServicioProducto`. Con doce recursos, doce servicios — no un `ApiService` con la tabla como parámetro |
| **Bootstrap SI, por CDN NO** | Se sirve desde `wwwroot/lib/bootstrap/`, y `wwwroot/app.css` va **encima** con las clases del dominio (`.tarjeta`, `.campos`, `.acciones`). A igual especificidad gana el ultimo que carga: por eso el orden no es decorativo |

## 3. Arquitectura en capas (flujo de una petición)

```
HTTP → ASP.NET routing        (los atributos [HttpGet]/[HttpPost]… deciden el método)
     → validación de la PETICIÓN (anotaciones de la petición del verbo → 422 automático)
     → ProductoController     (try/catch: traduce excepciones a códigos HTTP)
     → IServicioProducto      (interfaz — reglas de negocio)
     → IRepositorioProducto   (interfaz — el servicio no sabe qué motor hay detrás)
     → RepositorioProductoSqlServer (Dapper + parámetros @)
     → SQL Server
```

## 4. Decisiones de diseño clave

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

### 4.2 La validación vive en las PETICIONES (una por verbo)
- `ProductoCrear`      → POST: todos obligatorios (con código)
- `ProductoReemplazo`  → PUT: todos obligatorios (el código va en la URL)
- `ProductoActualizar` → PATCH: todos opcionales (se valida lo que llegue)

### 4.3 El ensamblador: la sección de DI de Program.cs
```csharp
builder.Services.AddScoped<IRepositorioProducto>(
    _ => new RepositorioProductoSqlServer(cadenaConexion));
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();
```

### 4.4 SQL del repositorio (Dapper, siempre parametrizado)
```sql
SELECT codigo, nombre, stock, valorunitario FROM producto ORDER BY codigo LIMIT @limite
SELECT … WHERE codigo = @codigo
INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES (@codigo, @nombre, @stock, @valorunitario)
UPDATE producto SET … WHERE codigo = @codigo_clave
DELETE FROM producto WHERE codigo = @codigo
```

### 4.5 Traducción de excepciones a HTTP (en el controller)
| Situación | HTTP |
|---|---|
| (Body con errores de forma) | 422 |
| `ArgumentException` | 400 |
| `NoEncontradoExcepcion` | 404 |
| `Microsoft.Data.SqlClientException` y cualquier otra | 500 (mensaje del motor en `detalle`) |

### 4.6 SQL Server se siembra SOLO
SQL Server ejecuta automáticamente los scripts montados en `/docker-entrypoint-initdb.d/` la primera vez.

## 5. Docker: un solo comando desde v1
`docker compose up -d --build` deja corriendo la base, la API y la interfaz gráfica.
