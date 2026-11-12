# Tareas — Versión 2: las tablas CON clave foránea y lógica en la base

> **Versión 2** — El orden de construcción y verificación paso a paso en Python.
> Cada fase termina en un entregable **verificable y funcional**.
> Requisitos: [2_spec.md](2_spec.md) · Técnica: [3_plan.md](3_plan.md) · Contratos: [6_contracts.md](6_contracts.md) · Validación final: [7_quickstart.md](7_quickstart.md).

---

## Fase 0 — Verificación de base de datos y esquema
- [x] Verificar que las 12 tablas de `bdfacturas` estén creadas y con sus restricciones activas.
- [x] Confirmar la presencia de los procedimientos almacenados de facturación y usuario:
  - `sp_listar_facturas_y_productosporfactura`
  - `sp_consultar_factura_y_productosporfactura`
  - `sp_insertar_factura_y_productosporfactura`
  - `sp_anular_factura`
  - `crear_usuario_con_roles`, `actualizar_usuario_con_roles`, `consultar_usuario_con_roles`, `listar_usuarios_con_roles`, `eliminar_usuario_con_roles`
- [x] Verificar el disparador `trg_actualizar_totales_y_stock` sobre `productosporfactura`.
- [x] Implementar `ConflictoExcepcion` en `api_python/excepciones.py` para mapear errores SQL `547` / `2627` / `2601` / `50000` a HTTP `409 Conflict`.

---

## Fase 1 — Entidades 1:N con Clave Foránea (`cliente` y `vendedor`)
- [x] **Esquemas Pydantic de `cliente`:**
  - `ClienteCrear`, `ClienteReemplazo`, `ClienteActualizar` en `api_python/peticiones/esquemas.py` (`credito`, `fkcodpersona`, `fkcodempresa` opcional).
- [x] **Capa de datos, servicio y router de `cliente`:**
  - `RepositorioCliente` en `api_python/repositorios/repositorio_v2.py` (`JOIN` con `persona` y `empresa`, captura error SQL `547` → `ConflictoExcepcion`).
  - `ServicioCliente` en `api_python/servicios/servicio_v2.py` y rutas `/api/cliente` en `api_python/routers/router_v2.py`.
- [x] **Esquemas, repositorio, servicio y router de `vendedor`:**
  - `VendedorCrear`, `VendedorReemplazo`, `VendedorActualizar`.
  - `RepositorioVendedor`, `ServicioVendedor` y rutas `/api/vendedor`.
- [x] **Integridad referencial inversa:**
  - Controlar borrado en tablas independientes (`persona`, `empresa`, `producto`) lanzando HTTP `409` si tienen registros hijos asociados.

---

## Fase 2 — Facturación Maestro-Detalle por Procedimientos y Disparador
- [x] **Esquemas de Factura:**
  - `FacturaCrear` e `ItemFacturaCrear` en `api_python/peticiones/esquemas.py` (**sin** campos de subtotal, total ni precio unitario manual).
- [x] **Repositorio exclusivo por Stored Procedures:**
  - `RepositorioFactura` en `api_python/repositorios/repositorio_v2.py`.
  - Ejecución directa de `sp_listar_facturas_y_productosporfactura`, `sp_consultar_factura_y_productosporfactura`, `sp_insertar_factura_y_productosporfactura` (pasando JSON de detalle) y `sp_anular_factura`.
  - Ninguna sentencia `INSERT INTO factura` ni cálculo aritmético manual de totales en Python.
- [x] **Servicio y Router:**
  - `ServicioFactura` y endpoints `/api/factura` en `router_v2.py`.
  - Regla de stock insuficiente controlada por la base de datos y traducida a `409` (`ConflictoExcepcion`).
  - Anulación mediante `DELETE /api/factura/{numero}` ejecutando `sp_anular_factura` (borrado lógico `estado = 'anulada'`).

---

## Fase 3 — Tablas Puente M:N (`rol_usuario` y `rutarol`)
- [x] **Recurso `rol_usuario` (`/api/rol-usuario`):**
  - Esquema `RolUsuarioCrear` en `api_python/peticiones/esquemas.py`.
  - `RepositorioRolUsuario`, `ServicioRolUsuario` y rutas en `router_v2.py`:
    - `GET /api/rol-usuario`, `GET /api/rol-usuario/usuario/{email}`, `GET /api/rol-usuario/rol/{idrol}`.
    - `POST /api/rol-usuario` (`409` si duplicado o FK inexistente).
    - `DELETE /api/rol-usuario/{email}/{idrol}` (borrado por clave compuesta).
- [x] **Recurso `rutarol` (`/api/rutarol`):**
  - Esquema `RutaRolCrear`.
  - `RepositorioRutaRol`, `ServicioRutaRol` y rutas `/api/rutarol`:
    - `GET /api/rutarol`, `GET /api/rutarol/ruta/{idruta}`, `GET /api/rutarol/rol/{idrol}`.
    - `POST /api/rutarol` y `DELETE /api/rutarol/{idruta}/{idrol}`.

---

## Fase 4 — Recurso Compuesto Atómico (`/api/usuario-con-roles`)
- [x] **Esquemas Pydantic:**
  - `UsuarioConRolesCrear` (con lista `roles` de mínimo 1 rol → `422` si llega vacía) y `UsuarioConRolesActualizar` (`contrasena` opcional; si llega vacía, conserva la actual).
- [x] **Capa de Datos con Procedimientos Almacenados:**
  - `RepositorioUsuarioConRoles` en `api_python/repositorios/repositorio_v2.py`.
  - Invoca los 5 SPs (`listar_usuarios_con_roles`, `consultar_usuario_con_roles`, `crear_usuario_con_roles` con `@p_roles_json`, `actualizar_usuario_con_roles` con `@p_roles`, `eliminar_usuario_con_roles`).
- [x] **Servicio y Router:**
  - `ServicioUsuarioConRoles`: transforma la lista `[1, 2]` en JSON `[{"fkidrol": 1}, {"fkidrol": 2}]` compatible con `OPENJSON` en SQL Server y hashea la contraseña con BCrypt costo 12 cuando se envía.
  - Endpoints `/api/usuario-con-roles` en `router_v2.py` con envoltura estándar `{ "tabla": "usuario_con_roles", "total": N, "datos": [...] }`.

---

## Fase 5 — Interfaz Gráfica en Python (`front_python/`)
- [x] **Cliente HTTP asíncrono centralizado (`front_python/servicios/cliente_api.py`):**
  - Peticiones con `httpx.AsyncClient` hacia todos los endpoints de v2.
- [x] **Vistas de Clientes y Vendedores con Desplegables:**
  - Selectores `<select>` poblados dinámicamente desde `api/persona` y `api/empresa` en `front_python/templates/crud_generico.html` (incluyendo opción vacía/nula en empresa para persona natural).
- [x] **Módulo de Facturación:**
  - `facturas.html`: Listado de facturas con acceso al detalle y botón de anular.
  - `emitir_factura.html`: Formulario Maestro-Detalle interactivo que envía el arreglo `items` a la API sin calcular subtotales en el navegador.
  - `factura_detalle.html`: Ficha individual `/facturas/{numero}` con detalle de productos, subtotales calculados por la BD y estado.
- [x] **Gestión de Permisos y Roles:**
  - `usuarios_y_roles.html`: Alta y edición atómica de usuario con checkboxes de roles.
  - `roles_por_usuario.html`: Gestión de tabla puente `rol_usuario`.
  - `permisos_por_rol.html`: Gestión de tabla puente `rutarol`.
- [x] **Resiliencia y Concurrencia:**
  - Carga paralela con `asyncio.gather(...)` en `front_python/main.py` para poblar múltiples catálogos foráneos simultáneamente.

---

## Fase 6 — Verificación, Regresión y Cierre
- [x] **Pruebas unitarias con mocks en memoria:** Ejecutar `api_python/pruebas/test_capas.py` (`TestCapasServicios` sin dependencia de SQL Server).
- [x] **Regresión de v1:** Verificar que los 6 recursos de v1 continúen operativos junto a los 6 recursos nuevos de v2.
- [x] **Documentación separada y coherente:** Actualizar `docs/v1_sin_fk/` y `docs/v2_con_fk/` con la arquitectura 100% Python.
