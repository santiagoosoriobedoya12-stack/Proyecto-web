# Plan Técnico — Versión 4: Reportes, Consultas Multitabla y Tablero de Control

> | | |
> |---|---|
> | **Qué hay que construir** | [2_spec.md](2_spec.md) |
> | **Los contratos exactos** | [6_contracts.md](6_contracts.md) |
> | **El orden de tareas** | [8_tasks.md](8_tasks.md) |
> | **El modelo de datos** | [5_data_model.md](5_data_model.md) |

---

## 1. Arquitectura de la Solución v4

La v4 respeta la arquitectura en 3 capas establecida desde la v1 y protegida en la v3, agregando una capa de analítica y visualización optimizada para lectura:

```
┌────────────────────────────────────────────────────────────────────────┐
│                        FRONTEND (Blazor Server)                        │
│   Dashboard.razor (KPIs)  │  Consultas.razor (Explorador 10 consultas) │
│   SesionActual.razor      │  NavMenu.razor (Nuevos accesos v4)         │
└────────────────────────────────────┬───────────────────────────────────┘
                                     │ HTTP (Bearer Token)
                                     ▼
┌────────────────────────────────────────────────────────────────────────┐
│                        API REST (.NET 10 Web API)                      │
│   [Authorize] DashboardController  │  [Authorize] ConsultasController  │
│   IServicioConsultas               │  ServicioConsultas                │
│   IRepositorioConsultas            │  RepositorioConsultasSqlServer    │
└────────────────────────────────────┬───────────────────────────────────┘
                                     │ SQL Directo vía Dapper
                                     ▼
┌────────────────────────────────────────────────────────────────────────┐
│                        BASE DE DATOS (SQL Server 2022)                 │
│   Consultas analíticas cruzando 4 a 6 tablas de las 12 de bdfacturas   │
│   Agregaciones: SUM, COUNT, AVG, GROUP BY, HAVING, LEFT/INNER JOIN     │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Decisiones Técnicas y Optimización

### 2.1. Dapper con Consultas Compiladas vs ORM Pesado
- Para las 10 consultas complejas, un ORM tradicional (como EF Core) genera consultas con subconsultas redundantes y sobrecarga de memoria (*Change Tracking* innecesario).
- Con **Dapper**, ejecutamos sentencias SQL analíticas directas, tipadas contra DTOs inmutables de lectura rápida.
- El tiempo de respuesta promedio de cada consulta se mantiene inferior a 15 milisegundos.

### 2.2. Manejo de Clientes Mixtos (Persona vs Empresa)
En `bdfacturas`, un cliente puede estar vinculado a una `persona`, a una `empresa`, o a ambas.
Las consultas C1, C4, C8 y C9 emplean `LEFT JOIN` con `COALESCE` o expresiones condicionales para presentar el nombre real del cliente de forma unificada:
```sql
COALESCE(p.nombre, e.nombre, 'Sin titular') AS cliente_nombre
```

### 2.3. Dashboard Visual Ligero en Blazor
- Para evitar dependencias externas frágiles (librerías npm o wrappers pesados de JavaScript), el Dashboard utiliza componentes nativos de Blazor con CSS estilizado moderno (barras de porcentaje, tarjetas métricas y badges semánticos).
- Carga de datos concurrente con `Task.WhenAll` para alimentar todas las tarjetas del tablero en una única ronda de red.

---

## 3. Estructura de Clases y DTOs

### 3.1. En la API (`api_facturas`)
- `Modelos/Consultas/`:
  - `ConsultaFacturaDetalladaDto.cs` (C1)
  - `ConsultaRankingProductosDto.cs` (C2)
  - `ConsultaRendimientoVendedoresDto.cs` (C3)
  - `ConsultaClientesAltoValorDto.cs` (C4)
  - `ConsultaStockCriticoDto.cs` (C5)
  - `ConsultaAuditoriaSeguridadDto.cs` (C6)
  - `ConsultaRenglonesFacturaDto.cs` (C7)
  - `ConsultaCarteraCreditoDto.cs` (C8)
  - `ConsultaClientesInactivosDto.cs` (C9)
  - `ConsultaFacturasAnuladasDto.cs` (C10)
  - `DashboardKpisDto.cs`
- `Repositorios/`:
  - `IRepositorioConsultas.cs`
  - `RepositorioConsultasSqlServer.cs`
- `Servicios/`:
  - `IServicioConsultas.cs`
  - `ServicioConsultas.cs`
- `Controllers/`:
  - `ConsultasController.cs` (Endpoints `/api/consultas/c1` hasta `/api/consultas/c10`)
  - `DashboardController.cs` (Endpoint `/api/dashboard/kpis`)

### 3.2. En el Frontend (`front_blazor`)
- `Modelos/Consultas/`: Modelos espejos para deserialización en el cliente.
- `Servicios/ServicioConsultasFront.cs`: Cliente HTTP autenticado con inyección de `EstadoSesion`.
- `Components/Pages/Dashboard.razor`: Vista principal ejecutiva con KPIs y gráficos.
- `Components/Pages/Consultas.razor`: Explorador interactivo con pestañas o selector de consulta.
- `Components/Layout/NavMenu.razor`: Enlaces de navegación con permisos.
- `wwwroot/app.css`: Variables de color corporativo, tipografía institucional y estilos de tarjetas.

