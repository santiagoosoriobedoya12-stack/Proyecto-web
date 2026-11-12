# Documentación General — Arquitectura y Evolución por Versiones

> **Proyecto:** Sistema Empresarial de Facturación (`bdfacturas`)  
> **Asignatura:** Aplicación y Servicios Web  
> **Tecnologías:** Python 3.12 (FastAPI + PyMSSQL + PyJWT + BCrypt) · Frontend Web Python (FastAPI + Jinja2 + httpx + Bootstrap local) · SQL Server 2022 · Docker Compose  
> **Metodología:** Desarrollo Incremental Acumulativo ($v1 \subset v2 \subset v3$).

---

## 1. La Metodología Incremental Acumulativa

El proyecto sigue una progresión estricta donde **cada versión incluye a la anterior sin modificar sus contratos**:

$$v1 \subset v2 \subset v3$$

```text
   ┌────────────────────────────────────────────────────────────────────────┐
   │ VERSIÓN 3: Seguridad Transversal (JWT, RBAC, 401 y 403)                │
   │ ┌────────────────────────────────────────────────────────────────────┐ │
   │ │ VERSIÓN 2: Las 6 Tablas Dependientes (FK, SPs, Triggers y 409)     │ │
   │ │ ┌────────────────────────────────────────────────────────────────┐ │ │
   │ │ │ VERSIÓN 1: Las 6 Tablas Independientes (CRUD base y corte)     │ │ │
   │ │ └────────────────────────────────────────────────────────────────┘ │ │
   │ └────────────────────────────────────────────────────────────────────┘ │
   └────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Matriz de Diferencias Clave entre Versiones

Esta matriz resume exactamente **qué tiene de diferente cada versión** para facilitar la explicación técnica y la sustentación:

| Dimensión | Versión 1 (`v1_sin_fk`) | Versión 2 (`v2_con_fk`) | Versión 3 (`v3_seguridad_rbac`) |
|---|---|---|---|
| **Propósito Principal** | Establecer la arquitectura limpia en 3 capas (Router-Servicio-Repositorio) con tablas planas. | Introducir la integridad referencial, lógica delegada en base de datos (Stored Procedures y Triggers) y transacciones contables. | Introducir la capa transversal de seguridad, protegiendo todos los endpoints y vistas según identidad y roles. |
| **Tablas que Gestiona** | **6 tablas independientes:**<br>• `producto`<br>• `persona`<br>• `empresa`<br>• `rol`<br>• `ruta`<br>• `usuario` | **Las 12 tablas completas:**<br>*(las 6 de la v1)* +<br>• `cliente`<br>• `vendedor`<br>• `factura`<br>• `productosporfactura`<br>• `rol_usuario`<br>• `rutarol`<br>+ Recurso `usuario-con-roles` | **Las 12 tablas protegidas:**<br>Aplica control de acceso sobre las 12 tablas sin modificar sus modelos de datos. |
| **¿Dónde vive la lógica?** | En Python (Servicios de dominio). SQL Server solo almacena datos con sentencias SQL directas ejecutadas vía PyMSSQL. | **En SQL Server:** Los subtotales, totales, validación de stock y anulación contable los ejecutan **Procedimientos Almacenados y un Trigger**. Python solo orquesta. | **En Dependencias FastAPI (`Depends`):** La validación de tokens JWT y autorización por rutas (`verificar_acceso_ruta`) se ejecuta antes de entrar al endpoint. |
| **Códigos HTTP** | • `200 OK`<br>• `201 Created`<br>• `204 No Content`<br>• `400 Bad Request`<br>• `404 Not Found`<br>• `422 Unprocessable`<br>• `500 Server Error` | **Agrega `409 Conflict`:**<br>Para violaciones de clave foránea (error SQL `547`) y registros duplicados en tablas puente (`2627`/`2601`). | **Agrega `401 Unauthorized` y `403 Forbidden`:**<br>• `401`: Token ausente, expirado o credenciales inválidas.<br>• `403`: Usuario autenticado cuyo rol no tiene permiso sobre la ruta solicitada. |
| **Peticiones POST / PUT** | El cliente envía todos los datos requeridos por la tabla. | En `factura`, el cliente **NO envía totales, subtotales ni precio unitario**. La base de datos es la única fuente de verdad contable. | Las peticiones exigen encabezado de autorización: `Authorization: Bearer <token>`. |
| **Interfaz en Python (`front_python/`)** | Vistas CRUD independientes con plantillas Jinja2. Campos de texto donde los identificadores se digitan manualmente. | • Desplegables dinámicos (`<select>`) alimentados por la API.<br>• Formulario Maestro-Detalle para emitir facturas.<br>• Ficha de factura.<br>• Carga paralela con `asyncio.gather`. | • Pantalla de Login (`/sesion`).<br>• Sesión segura en servidor con cookie `HttpOnly`.<br>• Menú dinámico que oculta opciones según el rol. |
| **Seguridad / Acceso** | Público y anónimo. | Público y anónimo (con integridad referencial validada). | **Privado y autenticado:** Gestión de tokens JWT, BCrypt costo 12 y verificación dinámica vía `verificar_acceso_ruta`. |

---

## 3. Mapa de las 12 Tablas de la Base de Datos (`bdfacturas`)

La base de datos completa contiene exactamente 12 tablas. Su reparto por versión responde al orden de dependencias:

```text
   TABLAS INDEPENDIENTES (Versión 1)           TABLAS DEPENDIENTES (Versión 2)
   ┌─────────┐                                 ┌─────────┐
   │ persona │ ──────────────┬───────────────► │ cliente │ ───────────┐
   └─────────┘               │                 └─────────┘            │
   ┌─────────┐               │                                        ▼
   │ empresa │ ──────────────┼──────────────────────────────► ┌─────────────┐
   └─────────┘               │                                │   factura   │ ◄──┐
                             │                 ┌─────────┐    └─────────────┘    │
                             └───────────────► │vendedor │ ───────────┘          │
                                               └─────────┘                       │
   ┌──────────┐                                                                  ▼
   │ producto │ ───────────────────────────────────────────────────► ┌─────────────────────┐
   └──────────┘                                                      │ productosporfactura │
                                                                     └─────────────────────┘
   ┌─────────┐                                 ┌─────────────┐
   │ usuario │ ──────────────────────────────► │ rol_usuario │ ◄──┐
   └─────────┘                                 └─────────────┘    │
                                                                  ▼
                                                              ┌─────────┐
                                                              │   rol   │ (v1)
                                                              └─────────┘
                                                                  ▲
   ┌─────────┐                                 ┌─────────────┐    │
   │  ruta   │ ──────────────────────────────► │   rutarol   │ ───┘
   └─────────┘                                 └─────────────┘
```

---

## 4. Directorios de Documentación

Cada versión cuenta con su propio paquete documental completo y aislado:

### [Versión 1 — Recursos Independientes (`docs/v1_sin_fk/`)](v1_sin_fk/README.md)
- [2_spec.md](v1_sin_fk/2_spec.md): Especificación funcional y criterios de aceptación.
- [3_plan.md](v1_sin_fk/3_plan.md): Plan de arquitectura en 3 capas en Python y esquemas Pydantic segregados.
- [4_research.md](v1_sin_fk/4_research.md): Decisiones técnicas (PyMSSQL vs ORM pesado, validación por verbo).
- [5_data_model.md](v1_sin_fk/5_data_model.md): Definición de las 6 tablas planas.
- [6_contracts.md](v1_sin_fk/6_contracts.md): Contratos OpenAPI/REST de los endpoints de v1.
- [7_quickstart.md](v1_sin_fk/7_quickstart.md): Guía de arranque y smoke tests con `curl`.
- [8_tasks.md](v1_sin_fk/8_tasks.md): Desglose de fases de construcción.

---

### [Versión 2 — Tablas con Clave Foránea y Lógica en BD (`docs/v2_con_fk/`)](v2_con_fk/README.md)
- [2_spec.md](v2_con_fk/2_spec.md): Especificación de las 6 tablas dependientes y los 18 criterios de aceptación.
- [3_plan.md](v2_con_fk/3_plan.md): Arquitectura de excepciones (`ConflictoExcepcion`), Pydantic y Stored Procedures.
- [4_research.md](v2_con_fk/4_research.md): Justificación de cálculo en triggers vs Python, `409` vs `422`, JSON para `OPENJSON`.
- [5_data_model.md](v2_con_fk/5_data_model.md): Código SQL completo de los SPs, triggers y tablas dependientes.
- [6_contracts.md](v2_con_fk/6_contracts.md): Contratos OpenAPI de `cliente`, `vendedor`, `factura`, `rol-usuario`, `rutarol` y `usuario-con-roles`.
- [7_quickstart.md](v2_con_fk/7_quickstart.md): Smoke test de v2 y protocolo de regresión de v1.
- [8_tasks.md](v2_con_fk/8_tasks.md): Fases de construcción y verificación de v2.

---

### [Versión 3 — Seguridad y Control de Acceso RBAC (`docs/v3_seguridad_rbac/`)](v3_seguridad_rbac/README.md)
- [2_spec.md](v3_seguridad_rbac/2_spec.md): Especificación funcional de autenticación, JWT, RBAC y criterios de aceptación.
- [3_plan.md](v3_seguridad_rbac/3_plan.md): Plan técnico: Dependencias FastAPI, `PyJWT`, `bcrypt` y sesión en servidor Jinja2.
- [4_research.md](v3_seguridad_rbac/4_research.md): Análisis técnico: JWT sin permisos embebidos, Salting BCrypt, `401` vs `403`.
- [5_data_model.md](v3_seguridad_rbac/5_data_model.md): Grafo relacional de seguridad y Stored Procedure `verificar_acceso_ruta`.
- [6_contracts.md](v3_seguridad_rbac/6_contracts.md): Contratos de `POST /api/sesion`, `GET /api/permisos/mios` y sobre de errores 401/403.
- [7_quickstart.md](v3_seguridad_rbac/7_quickstart.md): Guía de pruebas rápidas con `curl` para bloqueo `401`, login y bloqueo `403`.
- [8_tasks.md](v3_seguridad_rbac/8_tasks.md): Fases de construcción ordenadas paso a paso para la implementación de la v3.

---

## 5. Guía Rápida para Sustentación ante Docentes

Al momento de defender el proyecto, ten presentes estos conceptos clave:

1. **¿Por qué usamos PyMSSQL con SQL directo y no un ORM pesado como SQLAlchemy?**
   - PyMSSQL nos permite dejar el SQL explícito y completamente legible dentro del repositorio, controlar cada `JOIN` y llamar Procedimientos Almacenados directamente sin la sobrecarga ni las consultas ocultas de un ORM tradicional.
2. **¿Por qué la regla de negocio de stock y totales está en la base de datos en la v2?**
   - Para garantizar la consistencia contable y prevenir condiciones de carrera (*race conditions*). Al emitir una factura, el disparador y el procedimiento almacenado en SQL Server actualizan el stock y totalizan los renglones dentro de una transacción atómica. Si se hiciera en Python, dos peticiones concurrentes podrían vender el mismo inventario disponible.
3. **¿Cuál es la diferencia entre el error 409 y el 422?**
   - **422 Unprocessable Entity:** Error de forma, tipo o validación Pydantic en el cuerpo JSON enviado (ej. campo obligatorio omitido o cantidad negativa).
   - **409 Conflict:** La petición está bien formada sintácticamente, pero choca con el estado actual de la base de datos (ej. violar una clave foránea porque el padre no existe, o intentar registrar un duplicado).
4. **¿Por qué en `front_python/` usamos `asyncio.gather` con `httpx`?**
   - Si una pantalla necesita cargar personas y empresas de la API de forma secuencial (`await llamar_api(...)` uno tras otro), las latencias se suman. Con `asyncio.gather`, ambas peticiones viajan concurrentemente en paralelo reduciendo el tiempo total de respuesta.

---

## [Guía Completa de Retroalimentación y Preguntas de Sustentación](RETROALIMENTACION_Y_SUSTENTACION.md)

Para el desglose técnico versión por versión y las preguntas resueltas de examen en Python, consulta **[docs/RETROALIMENTACION_Y_SUSTENTACION.md](RETROALIMENTACION_Y_SUSTENTACION.md)**.
