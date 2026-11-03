# Tareas — Versión 2: las tablas CON clave foránea y lógica en la base

> **Versión 2** — El orden de construcción y verificación paso a paso.
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
- [x] Agregar la clase de excepción de dominio `ConflictoExcepcion.cs` para mapear a HTTP 409 Conflict.
- [x] Actualizar el endpoint de diagnóstico `GET /` para reportar `"version": "v2"`.

---

## Fase 1 — Entidades 1:N con Clave Foránea (`cliente` y `vendedor`)
- [x] **Modelo y peticiones de `cliente`:**
  - `Cliente.cs` (`id`, `credito`, `fkcodpersona`, `fkcodempresa` nullable).
  - Peticiones segregadas: `ClienteCrear.cs` (todos obligatorios salvo empresa), `ClienteReemplazo.cs` (PUT con 422 si falta alguno), `ClienteActualizar.cs` (PATCH parcial).
- [x] **Capa de datos y dominio de `cliente`:**
  - `IRepositorioCliente.cs` y `RepositorioClienteSqlServer.cs` (captura SqlException 547 y lanza `ConflictoExcepcion`).
  - `IServicioCliente.cs` y `ServicioCliente.cs`.
  - `ClienteController.cs` (mapea `ConflictoExcepcion` a 409).
- [x] **Modelo y peticiones de `vendedor`:**
  - `Vendedor.cs` (`id`, `carnet`, `direccion`, `fkcodpersona`).
  - Peticiones: `VendedorCrear.cs`, `VendedorReemplazo.cs`, `VendedorActualizar.cs`.
- [x] **Capa de datos y dominio de `vendedor`:**
  - `IRepositorioVendedor.cs` y `RepositorioVendedorSqlServer.cs` (captura SqlException 547 y lanza `ConflictoExcepcion`).
  - `IServicioVendedor.cs` y `ServicioVendedor.cs`.
  - `VendedorController.cs`.
- [x] **Integridad referencial inversa:**
  - Controlar borrado en tablas independientes (`persona`, `empresa`) lanzando 409 si tienen hijos asociados.

---

## Fase 2 — Facturación Maestro-Detalle por Procedimientos y Disparador
- [x] **Modelos de Factura:**
  - `Factura.cs`, `FacturaDetalle.cs`, `ProductoDeFactura.cs`.
  - `FacturaCrear.cs` e `ItemFacturaCrear.cs` (**sin** campos de subtotal, total ni precio unitario manual).
- [x] **Repositorio exclusivo por Stored Procedures:**
  - `IRepositorioFactura.cs` y `RepositorioFacturaSqlServer.cs`.
  - Llamadas a `sp_listar_facturas_y_productosporfactura`, `sp_consultar_factura_y_productosporfactura`, `sp_insertar_factura_y_productosporfactura` (pasando JSON al TVP/parámetro) y `sp_anular_factura`.
  - Ninguna sentencia `INSERT INTO factura` directa en C#.
- [x] **Servicio y Controlador:**
  - `IServicioFactura.cs`, `ServicioFactura.cs`, `FacturaController.cs`.
  - Validaciones de negocio: regla de stock controlada por la base y traducida a 409 / 400.
  - Anulación mediante `DELETE /api/factura/{numero}` ejecutando `sp_anular_factura` (borrado lógico `estado = 0`).

---

## Fase 3 — Tablas Puente M:N (`rol_usuario` y `rutarol`)
- [x] **Recurso `rol_usuario` (`/api/rol-usuario`):**
  - Modelos y contratos: `RolUsuario.cs`, `RolUsuarioCrear.cs`.
  - Repositorio y Servicio: `IRepositorioRolUsuario.cs`, `RepositorioRolUsuarioSqlServer.cs`, `ServicioRolUsuario.cs`.
  - `RolUsuarioController.cs`:
    - `GET /api/rol-usuario` (listar todas las asignaciones).
    - `GET /api/rol-usuario/usuario/{email}` (roles de un usuario).
    - `GET /api/rol-usuario/rol/{idrol}` (usuarios con dicho rol).
    - `POST /api/rol-usuario` (409 si duplicado).
    - `DELETE /api/rol-usuario/{email}/{idrol}` (borrado por clave compuesta).
- [x] **Recurso `rutarol` (`/api/rutarol`):**
  - Modelos y contratos: `RutaRol.cs`, `RutaRolCrear.cs`.
  - Repositorio y Servicio: `IRepositorioRutaRol.cs`, `RepositorioRutaRolSqlServer.cs`, `ServicioRutaRol.cs`.
  - `RutaRolController.cs`:
    - `GET /api/rutarol`, `GET /api/rutarol/ruta/{idruta}`, `GET /api/rutarol/rol/{idrol}`.
    - `POST /api/rutarol` y `DELETE /api/rutarol/{idruta}/{idrol}`.

---

## Fase 4 — Recurso Compuesto Atómico (`api/usuario-con-roles`)
- [x] **Modelos y DTOs:**
  - `UsuarioConRoles.cs` (incluye lista de identificadores `roles: int[]`).
  - `UsuarioConRolesCrear.cs` (validación `[MinLength(1)]` devolviendo 422 si la lista de roles está vacía).
  - `UsuarioConRolesActualizar.cs` (contraseña opcional; si llega vacía, conserva la anterior).
- [x] **Capa de Datos con Procedimientos Almacenados:**
  - `IRepositorioUsuarioConRoles.cs` y `RepositorioUsuarioConRolesSqlServer.cs`.
  - Invoca los 5 SPs pasando la lista serializada en JSON para `OPENJSON`.
- [x] **Servicio y Controlador:**
  - `ServicioUsuarioConRoles.cs`: transforma la lista de enteros `[1, 2]` a JSON de objetos `[{"fkidrol": 1}, {"fkidrol": 2}]` antes de entregar al repositorio (regla de dominio).
  - `UsuarioConRolesController.cs`: sobre estándar `{ "tabla": "usuario_con_roles", "total": N, "datos": [...] }`.

---

## Fase 5 — Interfaz Gráfica Blazor Server
- [x] **Servicios HTTP tipados en Front:**
  - `ServicioCliente.cs`, `ServicioVendedor.cs`, `ServicioFactura.cs`, `ServicioRolUsuario.cs`, `ServicioRutaRol.cs`, `ServicioUsuarioConRoles.cs`.
- [x] **Vistas de Clientes y Vendedores con Desplegables:**
  - `Clientes.razor`: Dropdowns dinámicos que muestran nombre y envían ID/código. Opción null para personas naturales.
  - `Vendedores.razor`: Dropdown dinámico para persona.
- [x] **Módulo de Facturación:**
  - `Facturas.razor`: Listado interactivo con detalle desplegable y botón de anular.
  - `EmitirFactura.razor` / `FacturaNueva.razor`: Maestro-detalle en memoria. Agregar y quitar renglones dinámicamente antes de enviar a la API. Sin cálculos manuales de total.
  - `FacturaDetalle.razor`: Ficha individual imprimible `/facturas/{numero}` con subtotales y estado.
- [x] **Gestión de Permisos y Roles:**
  - `UsuariosYRoles.razor` / `UsuarioConRoles.razor`: Lista con checkboxes de roles disponibles.
  - `RolesPorUsuario.razor`: Tabla puente directa.
  - `PermisosPorRol.razor` / `Permisos.razor`: Tabla puente de rutas con advertencia pedagógica de v3.
- [x] **Resiliencia y Concurrencia:**
  - Carga paralela con `Task.WhenAll` para catálogos foráneos, eliminando cuellos de botella de 50 segundos.

---

## Fase 6 — Verificación, Regresión y Cierre
- [x] **Smoke Test de v2:** Ejecución de las pruebas de [7_quickstart.md](7_quickstart.md) con curl.
- [x] **Prueba de Regresión de v1:** Verificar que los 6 recursos de v1 sigan funcionando al 100%.
- [x] **Prueba Unitaria / Mock en Memoria:** Ejecutar `api_facturas/pruebas/Programa.cs` con `RepositorioClienteFalso`.
- [x] **Documentación Separada y Limpia:** Estructurar `docs/v1_sin_fk/` y `docs/v2_con_fk/` con índices explicativos.

