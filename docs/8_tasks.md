# Tareas — Versión 1: las seis tablas sin FK, con su interfaz gráfica

> **Versión 1** · El orden de construcción, partiendo de CERO. Cada fase
> termina en algo **verificable**. Requisitos: [2_spec.md](2_spec.md) ·
> técnica: [3_plan.md](3_plan.md) · contratos: [6_contracts.md](6_contracts.md) ·
> validación final: [7_quickstart.md](7_quickstart.md).

---

## Fase 0 — Base de datos y esqueleto
- [ ] Copiar a `db/` el archivo **provisto** con esta versión: `bdfacturas.sql` (la BD completa).
- [ ] Crear el `docker-compose.yml` con el servicio de base de datos (puerto 11463 y script montado para auto-siembra).
- [ ] Crear `api_facturas/` con subcarpetas `Modelos/`, `Peticiones/`, `Controllers/`, `Servicios/`, `Repositorios/`, `Excepciones/` y `pruebas/`.

**Verificar:** `docker compose ps` muestra el servicio healthy y las 12 tablas con `SELECT count(*) FROM producto` dando 8.

## Fase 1 — El proyecto .NET y el modelo Producto (la clase entidad)
- [ ] `ApiFacturas.csproj`: proyecto Web de .NET 10, paquete `Microsoft.Data.SqlClient`, y exclusión de `pruebas/**`.
- [ ] `appsettings.json` con la cadena de conexión.
- [ ] `Modelos/Producto.cs`: la clase entidad con las 4 propiedades tipadas (`Codigo`, `Nombre`, `Stock`, `Valorunitario`).

## Fase 2 — Las peticiones por verbo (la frontera de entrada) y la excepción
- [ ] `Peticiones/ProductoCrear.cs` (POST: todo obligatorio, con código).
- [ ] `Peticiones/ProductoReemplazo.cs` (PUT: todo obligatorio, sin código).
- [ ] `Peticiones/ProductoActualizar.cs` (PATCH: todo opcional).
- [ ] `Excepciones/NoEncontradoExcepcion.cs`: excepción de negocio para HTTP 404.

## Fase 3 — Contratos (interfaces) y repositorio
- [ ] `Repositorios/IRepositorioProducto.cs`: interface con los 5 métodos async.
- [ ] `Servicios/IServicioProducto.cs`: interface del servicio.
- [ ] `Repositorios/RepositorioProductoSqlServer.cs`: Dapper con consultas parametrizadas.

## Fase 4 — Servicio (y la prueba de capas)
- [ ] `Servicios/ServicioProducto.cs`: valida reglas de negocio y traduce a excepciones de dominio.
- [ ] `pruebas/PruebaCapas.csproj` y `pruebas/Programa.cs`: servicio con repositorio falso en memoria (Criterio 6).

## Fase 5 — Controller y Program.cs
- [ ] `Controllers/ProductoController.cs`: atributos de verbo, try/catch y respuestas 200/204/400/404/500.
- [ ] `Program.cs`: ensamblador DI, 422 personalizado con lista de errores, Swagger y `GET /`.

## Fase 6 — La interfaz gráfica (Blazor Server)
- [ ] Proyecto `front_blazor/` en puerto 8096 sin paquetes de base de datos.
- [ ] Modelo `Producto` propio del front.
- [ ] `ServicioProducto` vía `HttpClient`.
- [ ] Vistas y componentes (`Productos.razor`, layout, Bootstrap local en `wwwroot/lib/bootstrap/` y `app.css`).

## Fase 7 — Prueba de resiliencia
- [ ] Detener `api-facturas` y validar que el frontend sigue en pie informando error de conexión sin filas.

## Fase 8 — Docker: un solo comando
- [ ] Dockerfiles con `dotnet watch` y orquestación unificada en `docker-compose.yml`.

## Fase 9 — Cierre de la versión
- [ ] Smoke test completo de todos los criterios.
- [ ] Commit y tag `v1`.
