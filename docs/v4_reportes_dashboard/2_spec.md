# Especificación — Versión 4: Reportes, Consultas Multitabla y Tablero de Control

> | | |
> |---|---|
> | **La ruta** | [Mapa de versiones](../README.md) |
> | **Lo anterior** | [v1](../v1_sin_fk/2_spec.md) las 6 sin FK · [v2](../v2_con_fk/2_spec.md) las 6 con FK · [v3](../v3_seguridad_rbac/2_spec.md) la seguridad RBAC y JWT |
> | **Lo que rige** | [Arquitectura general](../README.md) |

---

## 1. Propósito de la v4

**Explotar analíticamente el modelo relacional completo y presentar el valor del negocio.**

Con la v1 se levantó la arquitectura base en tres capas, con la v2 se completaron las 12 tablas con integridad referencial y lógica contable en base de datos, y con la v3 se blindó el sistema con autenticación JWT y control de acceso por roles (RBAC).

**La v4 no crea nuevas tablas ni modifica los contratos existentes.** La v4 toma las **12 tablas operativas** y construye la capa analítica de toma de decisiones:
1. **10 Consultas Multitabla Complejas:** Cada consulta cruza obligatoriamente **4 o más tablas**, aplicando agregaciones, agrupaciones y filtros analíticos para resolver preguntas reales de negocio.
2. **Tablero de Control (Dashboard):** Visualización ejecutiva de indicadores clave de rendimiento (KPIs), métricas financieras y resúmenes gráficos en Blazor Server.
3. **Identidad Corporativa y Manual de Marca:** Estandarización visual, paleta institucional moderna, tipografía y experiencia de usuario profesional.
4. **Seguridad y Regresión:** Todas las consultas analíticas respetan la seguridad de la v3 (exigen token Bearer y validación de rol).

---

## 2. Alcance

> **Qué define la v4:** No se crean tablas nuevas. Se aprovecha la riqueza de las 12 tablas para generar inteligencia de negocios y visualización ejecutiva.

**Incluye:**
- **10 endpoints analíticos** bajo `/api/consultas/c1` hasta `/api/consultas/c10`, cada uno cruzando un mínimo de 4 tablas.
- **1 endpoint de métricas ejecutivas** `/api/dashboard/kpis` para alimentar el tablero gerencial.
- **Capa de repositorio analítico con Dapper** optimizado para lectura rápida (consultas sin tracking, índices relacionales y agregaciones en el motor).
- **Interfaz gráfica de Dashboard en Blazor:** Tarjetas KPI interactivas, tablas de clasificación y barras analíticas visuales.
- **Interfaz gráfica de Explorador de Consultas en Blazor:** Vista interactiva con selector de consulta, filtros y tabla de resultados responsiva.
- **Manual de Marca:** Estilos corporativos unificados en CSS, paleta de colores institucional (Slate/Navy/Blue/Emerald) y logotipo del sistema.
- **Bitácora de Prompts:** Registro transparente de la construcción de cada consulta y componente en `prompts.md`.

**No incluye (y es deliberado):**
- Modificar los endpoints CRUD de las 12 tablas de v1/v2 (principio de sustitución y desarrollo incremental).
- Romper o saltear la autenticación de v3: nadie sin token puede consumir las consultas ni ver el dashboard.

---

## 3. Requisitos Funcionales — Las 10 Consultas Multitabla (4+ tablas)

### C1 — Consolidado Maestro de Facturación con Detalle de Partes
- **Tablas cruzadas (5 tablas):** `factura`, `cliente`, `persona` (titular cliente), `empresa` (cliente corporativo), `vendedor`, `persona` (titular vendedor).
- **Propósito de negocio:** Visión 360° de las facturas emitidas discriminando quién compró (persona o empresa), quién atendió la venta y el estado contable.

### C2 — Ranking de Productos más Vendidos y Rentabilidad Generada
- **Tablas cruzadas (4 tablas):** `producto`, `productosporfactura`, `factura`, `cliente`.
- **Propósito de negocio:** Clasificación de inventario de mayor rotación (Análisis ABC), volumen total de unidades vendidas y recaudación neta.

### C3 — Desempeño Comercial y Productividad por Vendedor
- **Tablas cruzadas (5 tablas):** `vendedor`, `persona` (vendedor), `factura`, `productosporfactura`, `producto`.
- **Propósito de negocio:** Cuadro de comisiones y productividad con facturas emitidas, facturas anuladas, volumen facturado y ticket promedio de venta.

### C4 — Segmentación de Clientes de Alto Valor y Fidelización
- **Tablas cruzadas (5 tablas):** `cliente`, `persona`, `empresa`, `factura`, `productosporfactura`.
- **Propósito de negocio:** Identificación de clientes VIP (Principio de Pareto 80/20), frecuencia de compra, volumen monetario y variedad de catálogo consumido.

### C5 — Alerta Preventiva de Stock Crítico y Rotación de Demanda
- **Tablas cruzadas (4 tablas):** `producto`, `productosporfactura`, `factura`, `vendedor`.
- **Propósito de negocio:** Monitoreo de inventario en riesgo de agotamiento (stock bajo) correlacionado con el ritmo histórico de salida en facturas activas.

### C6 — Auditoría de Seguridad, Roles y Trazabilidad de Accesos
- **Tablas cruzadas (5 tablas):** `usuario`, `rol_usuario`, `rol`, `rutarol`, `ruta`.
- **Propósito de negocio:** Matriz de control y gobierno de TI que audita qué rutas y privilegios efectivos tiene asignado cada usuario registrado.

### C7 — Renglones Detallados de Facturación con Trazabilidad Completa
- **Tablas cruzadas (5 tablas):** `factura`, `productosporfactura`, `producto`, `cliente`, `persona`.
- **Propósito de negocio:** Desglose transaccional ítem por ítem para auditoría contable, conciliación fiscal y resolución de quejas.

### C8 — Utilización de Línea de Crédito y Exposición Financiera
- **Tablas cruzadas (4 tablas):** `cliente`, `persona`, `empresa`, `factura`.
- **Propósito de negocio:** Comparación del cupo de crédito otorgado frente al saldo facturado activo para mitigar riesgos de mora comercial.

### C9 — Detección de Clientes Inactivos sin Compras Recientes
- **Tablas cruzadas (4 tablas):** `cliente`, `persona`, `empresa`, `factura`.
- **Propósito de negocio:** Identificación de clientes registrados que nunca han generado facturas o llevan tiempo inactivos para campañas de marketing.

### C10 — Auditoría de Facturas Anuladas e Impacto Financiero
- **Tablas cruzadas (5 tablas):** `factura`, `vendedor`, `persona` (vendedor), `cliente`, `productosporfactura`.
- **Propósito de negocio:** Análisis de pérdidas operativas, volumen de transacciones revertidas y vendedores asociados a facturas anuladas.

---

## 4. Requisitos del Tablero de Control (Dashboard) y Marca

### RF11 — Tablero Ejecutivo de Métricas (Dashboard)
- Tarjetas resumen con indicadores clave:
  - **Ventas Totales Activas:** Suma monetaria de todas las facturas en estado `'activa'`.
  - **Facturas Emitidas:** Conteo total diferenciando activas vs anuladas.
  - **Clientes Registrados:** Volumen total de compradores registrados.
  - **Productos en Alerta:** Cantidad de productos con stock menor o igual a 15 unidades.
- Gráficos y barras de rendimiento en Blazor:
  - Participación de ventas por vendedor.
  - Top 5 productos más demandados.

### RF12 — Marca Corporativa e Identidad Visual
- Paleta institucional coherente basada en la psicología del color para sistemas financieros:
  - **Primario Corporativo:** Azul Petróleo / Navy (`#0f172a`, `#1e3a8a`).
  - **Acento Financiero:** Verde Esmeralda (`#059669`, `#10b981`).
  - **Alerta / Precaución:** Ámbar cálido (`#d97706`).
  - **Fondo y Superficie:** Slate neutro limpio (`#f8fafc`, `#ffffff`).
- Isotipo y logotipo distintivo: **FactuCorp — Sistema Empresarial de Facturación**.
- Tipografía del sistema: Inter / Segoe UI para legibilidad en tablas financieras densas.

---

## 5. Criterios de Aceptación de la v4

| # | Criterio | Verificación |
|---|---|---|
| **1** | Cada una de las 10 consultas cruza **al menos 4 tablas** de las 12 del modelo relacional | Inspección del SQL y del plan de ejecución |
| **2** | Los endpoints `/api/consultas/c1` a `/c10` devuelven códigos `200 OK` con JSON fuertemente tipado | Pruebas de integración con `curl` |
| **3** | El endpoint `/api/dashboard/kpis` entrega métricas agregadas consistentes con la base de datos | Comprobación matemática de totales |
| **4** | Todas las consultas exigen token JWT válido (`401 Unauthorized` si no viaja el token) | Validación de seguridad transversal v3 |
| **5** | El Dashboard en Blazor Server carga métricas en tiempo real con tarjetas KPI y barras de avance | Navegación en la interfaz gráfica |
| **6** | La pantalla de Consultas en Blazor permite explorar las 10 consultas con presentación tabular clara | Uso del componente interactivo |
| **7** | Se aplica la identidad corporativa y paleta unificada en toda la aplicación | Revisión visual de estilos y logotipo |
| **8** | Regresión completa: Las pruebas de v1, v2 y v3 siguen pasando sin degradación | Smoke test integral de versiones |
| **9** | Bitácora de prompts documentada en `docs/v4_reportes_dashboard/prompts.md` | Inspección de la documentación |
| **10** | Paquete documental completo con sus 8 archivos normativos en la carpeta de la versión | Verificación de estructura de archivos |

