# Plan técnico — Versión 2: las tablas con clave foránea (Python / FastAPI + Jinja2)

> | | |
> |---|---|
> | **Lo que hay que construir** | [2_spec.md](2_spec.md) |
> | **Los formatos exactos** | [6_contracts.md](6_contracts.md) |
> | **El orden** | [8_tasks.md](8_tasks.md) |

---

## 1. La pila, que no cambia

| | |
|---|---|
| **Lenguaje** | **Python 3.12** sobre **FastAPI + Uvicorn** |
| **Base** | SQL Server 2022 |
| **Acceso a datos** | **PyMSSQL** — SQL y Stored Procedures a la vista, sin ORM pesado |
| **Interfaz gráfica** | **FastAPI + Jinja2 + httpx**, en su propio proyecto (`front_python/`) y su propio contenedor |
| **Todo** | Docker Compose, con un solo `docker compose up -d --build` |

---

## 2. Las capas, que tampoco cambian

```text
   HTTP  →  ROUTER      →  SERVICIO    →  REPOSITORIO  →  SQL Server
            HTTP/JSON      reglas de      SQL o EXEC       bdfacturas
            y códigos      dominio        parametrizado
```

| Capa | Qué sabe | Qué NO sabe |
|---|---|---|
| **Router (`routers/router_v2.py`)** | Verbos FastAPI, rutas, validación Pydantic y traducción automática de excepciones | Nada de SQL |
| **Servicio (`servicios/servicio_v2.py`)** | Las reglas del dominio y formato `OPENJSON` | **Nada de HTTP ni códigos de estado** |
| **Repositorio (`repositorios/repositorio_v2.py`)** | El SQL con `JOIN`, o el `EXEC` del procedimiento almacenado vía PyMSSQL | Nada de reglas de negocio |

**Regla clave en v2:** el servicio **nunca** devuelve HTTP `409`. Si una operación falla por clave foránea (`547`), duplicado (`2627`/`2601`) o stock insuficiente (`50000`+), se lanza `ConflictoExcepcion` y los manejadores globales en `api_python/main.py` la traducen a **409 Conflict**.

---

## 3. Lo que la v2 agrega, módulo por módulo

### 3.1 Los dos recursos con FK — `cliente` y `vendedor`

```text
api_python/peticiones/esquemas.py       ClienteCrear/Reemplazo/Actualizar · VendedorCrear/Reemplazo/Actualizar
api_python/repositorios/repositorio_v2.py RepositorioCliente · RepositorioVendedor (con JOINs descriptivos)
api_python/servicios/servicio_v2.py     ServicioCliente · ServicioVendedor
api_python/routers/router_v2.py         /api/cliente · /api/vendedor
```

### 3.2 `factura` — el repositorio que delega 100% en procedimientos

```text
api_python/peticiones/esquemas.py       FacturaCrear · ItemFacturaCrear ({ codigo, cantidad })
api_python/repositorios/repositorio_v2.py RepositorioFactura (4 SPs, cero cálculos en Python)
api_python/servicios/servicio_v2.py     ServicioFactura
api_python/routers/router_v2.py         GET, GET/{num}, POST y DELETE (anulación lógica)
```

### 3.3 Las dos tablas puente — `rol_usuario` y `rutarol`

```text
api_python/peticiones/esquemas.py       RolUsuarioCrear · RutaRolCrear (sin PUT ni PATCH)
api_python/repositorios/repositorio_v2.py RepositorioRolUsuario · RepositorioRutaRol
api_python/servicios/servicio_v2.py     ServicioRolUsuario · ServicioRutaRol
api_python/routers/router_v2.py         /api/rol-usuario · /api/rutarol
```

### 3.4 `usuario-con-roles` — operación atómica con `OPENJSON`

```text
api_python/peticiones/esquemas.py       UsuarioConRolesCrear · UsuarioConRolesActualizar
api_python/repositorios/repositorio_v2.py RepositorioUsuarioConRoles (5 SPs: @p_roles_json / @p_roles)
api_python/servicios/servicio_v2.py     ServicioUsuarioConRoles (convierte [1,2] -> [{"fkidrol":1},...])
api_python/routers/router_v2.py         /api/usuario-con-roles
```

### 3.5 Las plantillas web en `front_python/templates/`

| Plantilla | Qué resuelve |
|---|---|
| `crud_generico.html` | En `/clientes` y `/vendedores`: `<select>` dinámicos (opción `(ninguna)` → `None` para persona natural) |
| `facturas.html` | Tabla de facturas emitidas, acceso a ficha detallada y botón de anular |
| `emitir_factura.html` | Maestro-Detalle interactivo en el navegador enviando ítems a `POST /api/factura` |
| `factura_detalle.html` | Ficha individual `/facturas/{numero}` con renglones y totales calculados por SQL Server |
| `usuarios_y_roles.html` | Formulario compuesto de usuario con checkboxes de roles |
| `roles_por_usuario.html` | Gestión de tabla puente `rol_usuario` por clave compuesta |
| `permisos_por_rol.html` | Gestión de tabla puente `rutarol` por clave compuesta |
