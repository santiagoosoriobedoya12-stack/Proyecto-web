# Documentación General — Arquitectura y Evolución por Versiones

> **Proyecto:** Sistema Empresarial de Facturación (`bdfacturas`)  
> **Asignatura:** Aplicación y Servicios Web  
> **Tecnologías:** C# .NET 10 (ASP.NET Core Web API + Dapper) · Blazor Server .NET 10 (Bootstrap local) · SQL Server 2022 · Docker Compose  
> **Metodología:** Desarrollo Incremental Acumulativo ($v1 \subset v2 \subset v3 \subset v4$).

---

## 1. La Metodología Incremental Acumulativa

El proyecto sigue una progresión estricta donde **cada versión incluye a la anterior sin modificar sus contratos**:

$$v1 \subset v2 \subset v3 \subset v4$$

```
   ┌────────────────────────────────────────────────────────────────────────┐
   │ VERSIÓN 4: Reportes, Transacciones Complejas y Rendimiento             │
   │ ┌────────────────────────────────────────────────────────────────────┐ │
   │ │ VERSIÓN 3: Seguridad Transversal (JWT, RBAC, 401 y 403)            │ │
   │ │ ┌────────────────────────────────────────────────────────────────┐ │ │
   │ │ │ VERSIÓN 2: Las 6 Tablas Dependientes (FK, SPs, Triggers y 409) │ │ │
   │ │ │ ┌────────────────────────────────────────────────────────────┐ │ │ │
   │ │ │ │ VERSIÓN 1: Las 6 Tablas Independientes (CRUD base y corte) │ │ │ │
   │ │ │ └────────────────────────────────────────────────────────────┘ │ │ │
   │ │ └────────────────────────────────────────────────────────────────┘ │ │
   │ └────────────────────────────────────────────────────────────────────┘ │
   └────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Matriz de Diferencias Clave entre Versiones

Esta matriz resume exactamente **qué tiene de diferente cada versión** para facilitar la explicación técnica y la sustentación:

| Dimensión | Versión 1 (`v1_sin_fk`) | Versión 2 (`v2_con_fk`) | Versión 3 (`v3_seguridad_rbac`) |
|---|---|---|---|
| **Propósito Principal** | Establecer la arquitectura limpia en 3 capas (Controlador-Servicio-Repositorio) con tablas planas. | Introducir la integridad referencial, lógica delegada en base de datos (Stored Procedures y Triggers) y transacciones contables. | Introducir la capa transversal de seguridad, protegiendo todos los endpoints y vistas según identidad y roles. |
| **Tablas que Gestiona** | **6 tablas independientes:**<br>• `producto`<br>• `persona`<br>• `empresa`<br>• `rol`<br>• `ruta`<br>• `usuario` | **Las 12 tablas completas:**<br>*(las 6 de la v1)* +<br>• `cliente`<br>• `vendedor`<br>• `factura`<br>• `productosporfactura`<br>• `rol_usuario`<br>• `rutarol`<br>+ Recurso `usuario-con-roles` | **Las 12 tablas protegidas:**<br>Aplica control de acceso sobre las 12 tablas sin modificar sus modelos de datos. |
| **¿Dónde vive la lógica?** | En C# (Servicios de dominio). SQL Server solo almacena datos con sentencias SQL directas ejecutadas vía Dapper. | **En SQL Server:** Los subtotales, totales, validación de stock y anulación contable los ejecutan **Procedimientos Almacenados y un Trigger**. C# solo orquesta. | **En Middlewares e Interceptores:** La validación de credenciales y autorización por rutas se ejecuta antes de que la petición toque los controladores. |
| **Códigos HTTP** | • `200 OK`<br>• `201 Created`<br>• `204 No Content`<br>• `400 Bad Request`<br>• `404 Not Found`<br>• `422 Unprocessable`<br>• `500 Server Error` | **Agrega `409 Conflict`:**<br>Para violaciones de clave foránea (SqlException 547) y registros duplicados en tablas puente (SqlException 2627/2601). | **Agrega `401 Unauthorized` y `403 Forbidden`:**<br>• `401`: Token ausente, expirado o credenciales inválidas.<br>• `403`: Usuario autenticado cuyo rol no tiene permiso sobre la ruta solicitada. |
| **Peticiones POST / PUT** | El cliente envía todos los datos requeridos por la tabla. | En `factura`, el cliente **NO envía totales, subtotales ni precio unitario**. La base de datos es la única fuente de verdad contable. | Las peticiones exigen encabezado de autorización: `Authorization: Bearer <token>`. |
| **Interfaz en Blazor** | Vistas CRUD independientes. Campos de texto planos donde los identificadores se digitan manualmente. | • Desplegables dinámicos (`<select>`) alimentados por la API.<br>• Formulario Maestro-Detalle interactivo en memoria para emitir facturas.<br>• Ficha de factura.<br>• Carga paralela con `Task.WhenAll`. | • Pantalla de Login obligatoria.<br>• Menú dinámico que oculta opciones según el rol.<br>• Redirección automática ante sesiones expiradas. |
| **Seguridad / Acceso** | Público y anónimo. | Público y anónimo (con integridad referencial validada). | **Privado y autenticado:** Gestión de tokens JWT, Claims y verificación de roles vía `verificar_acceso_ruta`. |

---

## 3. Mapa de las 12 Tablas de la Base de Datos (`bdfacturas`)

La base de datos completa contiene exactamente 12 tablas. Su reparto por versión responde al orden de dependencias:

```
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

### 📁 [Versión 1 — Recursos Independientes (`docs/v1_sin_fk/`)](v1_sin_fk/README.md)
- [2_spec.md](v1_sin_fk/2_spec.md): Especificación funcional y criterios de aceptación.
- [3_plan.md](v1_sin_fk/3_plan.md): Plan de arquitectura en 3 capas y separación de peticiones.
- [4_research.md](v1_sin_fk/4_research.md): Decisiones técnicas (Dapper vs ORM, DTOs segregados).
- [5_data_model.md](v1_sin_fk/5_data_model.md): Definición de las 6 tablas planas.
- [6_contracts.md](v1_sin_fk/6_contracts.md): Contratos OpenAPI/REST de los endpoints de v1.
- [7_quickstart.md](v1_sin_fk/7_quickstart.md): Guía de arranque y smoke tests con `curl`.
- [8_tasks.md](v1_sin_fk/8_tasks.md): Desglose de fases de construcción.

---

### 📁 [Versión 2 — Tablas con Clave Foránea y Lógica en BD (`docs/v2_con_fk/`)](v2_con_fk/README.md)
- [2_spec.md](v2_con_fk/2_spec.md): Especificación de las 6 tablas dependientes y los 18 criterios de aceptación.
- [3_plan.md](v2_con_fk/3_plan.md): Arquitectura de excepciones (`ConflictoExcepcion`), DTOs y Stored Procedures.
- [4_research.md](v2_con_fk/4_research.md): Justificación de cálculo en triggers vs C#, 409 vs 422, JSON para `OPENJSON`.
- [5_data_model.md](v2_con_fk/5_data_model.md): Código SQL completo de los SPs, triggers y tablas dependientes.
- [6_contracts.md](v2_con_fk/6_contracts.md): Contratos OpenAPI de `cliente`, `vendedor`, `factura`, `rol-usuario`, `rutarol` y `usuario-con-roles`.
- [7_quickstart.md](v2_con_fk/7_quickstart.md): Smoke test de v2 y protocolo de regresión de v1.
- [8_tasks.md](v2_con_fk/8_tasks.md): Fases de construcción y verificación de v2.

---

### 📁 [Versión 3 — Seguridad y Control de Acceso RBAC (`docs/v3_seguridad_rbac/`)](v3_seguridad_rbac/README.md)
- [2_spec.md](v3_seguridad_rbac/2_spec.md): Especificación funcional de autenticación, JWT, RBAC y criterios de aceptación.
- [3_plan.md](v3_seguridad_rbac/3_plan.md): Plan técnico: Middlewares de seguridad, JwtBearer, PBKDF2 y AuthenticationStateProvider.
- [4_research.md](v3_seguridad_rbac/4_research.md): Análisis técnico: JWT vs Cookies, Salting criptográfico, 401 vs 403.
- [5_data_model.md](v3_seguridad_rbac/5_data_model.md): Grafo relacional de seguridad y Stored Procedure `verificar_acceso_ruta`.
- [6_contracts.md](v3_seguridad_rbac/6_contracts.md): Contratos de `/api/auth/login`, `/api/auth/perfil` y sobre de errores 401/403.
- [7_quickstart.md](v3_seguridad_rbac/7_quickstart.md): Guía de pruebas rápidas con `curl` para bloqueo 401, login y bloqueo 403.
- [8_tasks.md](v3_seguridad_rbac/8_tasks.md): Fases de construcción ordenadas paso a paso para la implementación de la v3.

---

## 5. Guía Rápida para Sustentación ante Docentes

Al momento de defender el proyecto, tenga presentes estos conceptos clave:

1. **¿Por qué usamos Dapper y no Entity Framework?**
   - Dapper es un micro-ORM de alto rendimiento que deja el SQL a la vista. Permite controlar exactamente qué consulta se ejecuta y mapear directamente los resultados de Procedimientos Almacenados sin la sobrecarga ni la magia oculta de un ORM pesado.
2. **¿Por qué la regla de negocio de stock y totales está en la base de datos en la v2?**
   - Para garantizar la consistencia contable y prevenir condiciones de carrera (*race conditions*). Al emitir una factura, el disparador en SQL Server actualiza el stock y totaliza los renglones dentro de una transacción atómica. Si se hiciera en C#, dos peticiones simultáneas podrían vender el mismo stock disponible.
3. **¿Cuál es la diferencia entre el error 409 y el 422?**
   - **422 Unprocessable Entity:** Error de forma o sintaxis en el cuerpo de la petición (ej. campo obligatorio omitido, o formato incorrecto).
   - **409 Conflict:** La petición está bien formada sintácticamente, pero entra en conflicto con el estado actual de la base de datos (ej. violar una llave foránea porque el padre no existe, o intentar registrar un duplicado).
4. **¿Por qué en Blazor usamos `Task.WhenAll`?**
   - Si una pantalla necesita cargar personas y empresas de la API de forma secuencial (`await Servicio1; await Servicio2`), los tiempos de espera se suman. Con `Task.WhenAll`, ambas peticiones viajan concurrentemente, reduciendo el tiempo de carga a una fracción.

---

## 🎓 [Guía Completa de Retroalimentación y Preguntas de Sustentación (Leer Aquí)](RETROALIMENTACION_Y_SUSTENTACION.md)

Para un desglose completo de la retroalimentación técnica, justificaciones arquitectónicas versión por versión y las 10 preguntas clave de examen resueltas, consulta el documento:
👉 **[docs/RETROALIMENTACION_Y_SUSTENTACION.md](RETROALIMENTACION_Y_SUSTENTACION.md)**
