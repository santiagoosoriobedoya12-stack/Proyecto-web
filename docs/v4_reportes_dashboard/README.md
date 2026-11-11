# Versión 4 — Reportes, Consultas Multitabla y Tablero de Control

> **Asignatura:** Aplicación y Servicios Web  
> **Proyecto:** Sistema Empresarial de Facturación (`bdfacturas`)  
> **Versión:** 4 de 4 ($v1 \subset v2 \subset v3 \subset v4$)  
> **Tecnologías:** .NET 10 Web API · Dapper · Blazor Server · SQL Server 2022 · Docker Compose  

---

## 1. Resumen Ejecutivo de la Versión

La **Versión 4** representa el cierre analítico y visual del sistema de facturación. En esta etapa se aprovechan las **12 tablas relacionales completas** construidas en la v1 y v2, y protegidas bajo el esquema de seguridad RBAC y JWT de la v3, para transformar los datos transaccionales en información ejecutiva de valor para la toma de decisiones.

```
   v1 (6 tablas planas) ────────► Arquitectura en 3 capas
          │
   v2 (12 tablas con FK) ───────► Integridad referencial, SPs y Triggers
          │
   v3 (Seguridad transversal) ──► JWT Bearer, Hashes BCrypt, RBAC (401/403)
          │
   v4 (Analítica y Visual) ─────► 10 Consultas Multitabla (4+ tablas) + Dashboard
```

---

## 2. Contenido del Paquete Documental

| Archivo | Contenido |
|---|---|
| [2_spec.md](2_spec.md) | Especificación funcional completa, descripción de las 10 consultas, dashboard, criterios de aceptación y definición de terminada. |
| [3_plan.md](3_plan.md) | Plan técnico de arquitectura, optimizaciones de Dapper vs ORM pesado, y flujo de peticiones. |
| [4_research.md](4_research.md) | Investigación técnica comparativa, justificación de agregaciones en SQL Server y manual de marca. |
| [5_data_model.md](5_data_model.md) | Definición formal de las 10 consultas SQL complejas cruzando entre 4 y 6 tablas relacionales. |
| [6_contracts.md](6_contracts.md) | Especificación de los contratos OpenAPI/REST de `/api/consultas/c1..c10` y `/api/dashboard/kpis`. |
| [7_quickstart.md](7_quickstart.md) | Guía de arranque, smoke tests con `curl`, verificación de bloqueo 401 y validación en Blazor. |
| [8_tasks.md](8_tasks.md) | Plan de tareas ordenadas fase a fase desde la documentación hasta las pruebas. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA para el diseño SQL, backend, frontend y estilos corporativos. |

---

## 3. Las 10 Consultas Multitabla Implementadas

1. **C1 — Consolidado Maestro de Facturación:** Facturas completas con cliente unificado (persona/empresa) y vendedor responsable (6 tablas).
2. **C2 — Ranking de Productos más Vendidos:** Demanda de artículos, volumen vendido y recaudación total (4 tablas).
3. **C3 — Desempeño Comercial por Vendedor:** Productividad, facturas activas vs anuladas y ticket promedio (5 tablas).
4. **C4 — Clientes de Alto Valor:** Segmentación VIP, compras acumuladas y variedad de catálogo (5 tablas).
5. **C5 — Alerta de Stock Crítico:** Semáforo de riesgo de inventario y rotación histórica (4 tablas).
6. **C6 — Auditoría de Seguridad y Roles:** Mapeo de usuarios, roles efectivos y rutas autorizadas (5 tablas).
7. **C7 — Renglones Detallados de Factura:** Desglose transaccional producto por producto con subtotales (5 tablas).
8. **C8 — Utilización de Línea de Crédito:** Comparación de cupo otorgado vs facturado activo (4 tablas).
9. **C9 — Clientes Inactivos:** Detección de compradores sin órdenes para campañas de reactivación (4 tablas).
10. **C10 — Auditoría de Facturas Anuladas:** Control de pérdidas operativas y transacciones revertidas (5 tablas).

---

## 4. Características del Tablero de Control (Dashboard)

- **4 Tarjetas KPI en Tiempo Real:** Ventas Totales Activas, Total Facturas Emitidas, Clientes Registrados y Productos en Stock Crítico.
- **Gráficos y Tablas Integradas:** Ranking de los 5 productos más vendidos con barras visuales de rotación en CSS puro.
- **Identidad Corporativa "FactuCorp ERP":** Paleta institucional moderna (Navy/Slate/Blue/Emerald) y tipografía estandarizada.
- **Seguridad Garantizada:** Todo el dashboard y los reportes están protegidos con autenticación JWT Bearer.

