# Plan de Tareas — Versión 4: Reportes, Consultas Multitabla y Dashboard

> | | |
> |---|---|
> | **La especificación** | [2_spec.md](2_spec.md) |
> | **El plan técnico** | [3_plan.md](3_plan.md) |
> | **Los contratos** | [6_contracts.md](6_contracts.md) |

---

## 1. Desglose de Fases de Construcción

```
FASE 1: Paquete Documental Completo (docs/v4_reportes_dashboard/)
  │
  ▼
FASE 2: DTOs y Repositorio de Consultas con Dapper (Backend)
  │
  ▼
FASE 3: Servicio de Negocio y Controladores REST (Consultas y Dashboard)
  │
  ▼
FASE 4: Dashboard y Explorador de Consultas en Blazor (Frontend)
  │
  ▼
FASE 5: Identidad Corporativa, Manual de Marca y Navegación
  │
  ▼
FASE 6: Smoke Test, Verificación de Regresión y Bitácora de Prompts
```

---

## 2. Tareas Detalladas

### Fase 1: Documentación Oficial
- [x] **T1.1**: Redactar `2_spec.md` con los requisitos funcionales de las 10 consultas, dashboard y criterios de aceptación.
- [x] **T1.2**: Redactar `3_plan.md` con la arquitectura en capas y decisiones técnicas.
- [x] **T1.3**: Redactar `4_research.md` analizando Dapper vs EF Core y agregaciones en base de datos.
- [x] **T1.4**: Redactar `5_data_model.md` con los 10 scripts SQL analíticos (4+ tablas).
- [x] **T1.5**: Redactar `6_contracts.md` con los contratos OpenAPI de `/api/consultas` y `/api/dashboard`.
- [x] **T1.6**: Redactar `7_quickstart.md` con las pruebas `curl` y validación de seguridad.
- [x] **T1.7**: Redactar `8_tasks.md` con el desglose de fases y tareas.
- [ ] **T1.8**: Redactar `prompts.md` con los prompts detallados de construcción.
- [ ] **T1.9**: Redactar `README.md` de la versión 4.

### Fase 2: Backend — DTOs y Repositorio Dapper
- [ ] **T2.1**: Crear los DTOs fuertemente tipados en `api_facturas/Modelos/Consultas/` para las 10 consultas y los KPIs del Dashboard.
- [ ] **T2.2**: Definir la interfaz `IRepositorioConsultas.cs` con métodos asíncronos para cada consulta y para el dashboard.
- [ ] **T2.3**: Implementar `RepositorioConsultasSqlServer.cs` con Dapper ejecutando el SQL optimizado.

### Fase 3: Backend — Servicio y Controladores REST
- [ ] **T3.1**: Definir interfaz `IServicioConsultas.cs` e implementar `ServicioConsultas.cs`.
- [ ] **T3.2**: Crear `ConsultasController.cs` con los endpoints `/api/consultas/c1` hasta `/api/consultas/c10` protegidos con `[Authorize]`.
- [ ] **T3.3**: Crear `DashboardController.cs` con el endpoint `/api/dashboard/kpis` protegido con `[Authorize]`.
- [ ] **T3.4**: Registrar servicios y repositorios en el contenedor de dependencias (`Program.cs`).

### Fase 4: Frontend — Dashboard y Consultas en Blazor
- [ ] **T4.1**: Crear modelos DTO y servicio `ServicioConsultasFront.cs` en `front_blazor` con inyección de `EstadoSesion`.
- [ ] **T4.2**: Desarrollar la vista ejecutiva `Dashboard.razor` con tarjetas KPI y gráficos de barras CSS.
- [ ] **T4.3**: Desarrollar la vista interactiva `Consultas.razor` para explorar dinámicamente las 10 consultas multitabla.

### Fase 5: Marca e Identidad Corporativa
- [ ] **T5.1**: Actualizar `NavMenu.razor` agregando enlaces al Dashboard y al explorador de Consultas.
- [ ] **T5.2**: Aplicar la paleta de colores institucional (Navy/Slate/Blue/Emerald) y logotipo corporativo "FactuCorp ERP".

### Fase 6: Pruebas y Cierre
- [ ] **T6.1**: Ejecutar pruebas de compilación en Backend y Frontend.
- [ ] **T6.2**: Validar smoke test con token JWT y control de errores.
- [ ] **T6.3**: Actualizar `docs/README.md` y `README.md` principal del repositorio.

