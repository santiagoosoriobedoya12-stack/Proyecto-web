# Plan técnico — Versión 1: las seis tablas sin FK (Python: FastAPI + Flask + Jinja2)

> **Versión 1** · CÓMO construir lo especificado en [2_spec.md](2_spec.md).
> El porqué de cada decisión: [4_research.md](4_research.md) · contratos
> exactos: [6_contracts.md](6_contracts.md) · orden: [8_tasks.md](8_tasks.md).

---

## 1. Stack

| Pieza | Elección | Por qué |
|---|---|---|
| Lenguaje / framework | **Python 3.12 con FastAPI + Uvicorn** | Tipado con Pydantic, enrutamiento declarativo, OpenAPI automático en `/swagger`, async nativo |
| Acceso a datos | **PyMSSQL** con SQL parametrizado a mano (`%s`) | SQL completamente visible — PyMSSQL ejecuta SQL escrito a mano y retorna diccionarios (`as_dict=True`) sin magia oculta de un ORM (Art. 2) |
| Validación | **Esquemas Pydantic segregados por verbo** | FastAPI valida el body contra el esquema del verbo (`Crear`, `Reemplazo`, `Actualizar`) y dispara `RequestValidationError` → **HTTP 422** |
| Motor (v1) | **SQL Server 2022** (contenedor oficial) | Motor relacional del proyecto con las 12 tablas, triggers y SPs |
| Contenedor de la API | `python:3.12-slim` + `uvicorn --reload` | Guardar un `.py` recarga el servidor automáticamente |

## 2. Estructura de carpetas

```text
(raíz del proyecto)
├── docker-compose.yml                # UN comando: sqlserver + sqlserver-init + api-facturas + front-python
├── db/
│   ├── bdfacturas.sql                # la BD completa, PROVISTA (12 tablas, triggers, SPs)
│   └── init.sh                       # inicialización idempotente de la base
└── api_python/
    ├── requirements.txt              # fastapi, uvicorn, pymssql, pydantic[email], pyjwt, bcrypt
    ├── config.py                     # variables de entorno (cadena SQL, JWT, puertos)
    ├── database.py                   # conexión PyMSSQL, extracción de errores SQL y normalización JSON
    ├── excepciones.py                # excepciones de dominio (400, 401, 403, 404, 409)
    ├── main.py                       # ensamblador FastAPI, manejadores de error globales y Swagger
    ├── Dockerfile                    # imagen python:3.12-slim en puerto 8032 (expuesto en 8035)
    ├── peticiones/
    │   └── esquemas.py               # ProductoCrear, ProductoReemplazo, ProductoActualizar (y las demás tablas)
    ├── modelos/
    │   └── entidades.py              # Entidades POO tipadas (Producto, Empresa, Persona, Rol, Ruta, Usuario)
    ├── controladores/
    │   └── controlador_v1.py         # HTTP: GET/POST/PUT/PATCH/DELETE + códigos de estado
    ├── servicios/
    │   └── servicio_v1.py            # reglas de negocio (compone IRepositorio* vía constructor)
    ├── repositorios/
    │   ├── interfaces.py             # contratos abstractos ABC (IRepositorioProducto, etc.)
    │   └── repositorio_v1.py         # PyMSSQL + SQL explícito parametrizado a mano
    ├── ensamblador.py                # ÚNICO archivo que instancia concretos e inyecta servicios
    └── pruebas/
        └── test_capas.py             # pruebas unittest con polimorfismo en memoria (sin BD)
```

### 2bis. El front, que es un proyecto APARTE (Flask + Jinja2)

```text
front_python/
├── requirements.txt                  # flask, httpx (SIN pymssql)
├── app.py                            # rutas Flask explícitas por recurso y carga paralela
├── Dockerfile                        # python:3.12-slim en puerto 8096 (expuesto en 8099)
├── servicios/
│   ├── cliente_api.py                # transporte httpx y sesiones HttpOnly en servidor
│   └── servicios_recursos.py         # funciones explícitas por operación (listar_productos, etc.)
├── templates/
│   ├── base.html                     # layout base + navegación dinámica
│   ├── home.html                     # página de bienvenida
│   └── crud_recurso.html             # interfaz CRUD reutilizable por vistas explícitas
└── static/css/
    ├── bootstrap.min.css             # SERVIDO DESDE AQUÍ, nunca por CDN
    └── app.css                       # capa visual encima de Bootstrap
```

**Que el front y la API estén ambos en Python no rompe el aislamiento entre procesos**, y hay que
cuidarlo: la separación real garantiza cero dependencias compartidas.

| Regla | Por qué |
|---|---|
| **El front consume contratos JSON, no clases compartidas** | La API y el front se comunican exclusivamente por HTTP/JSON; el front no importa módulos de `api_python` |
| **`front_python/requirements.txt` no tiene PyMSSQL** | No es un olvido: comprueba que este proceso **no puede** conectarse directamente a SQL Server |
| **Cliente HTTP único en `cliente_api.py`** | Centraliza `httpx.AsyncClient`, traducción de errores 401/403/409/422 y manejo de caída de red |
| **Bootstrap SI, por CDN NO** | Se sirve desde `/static/css/bootstrap.min.css`, y `/static/css/app.css` va **encima** con las clases del dominio |

## 3. Arquitectura en capas (flujo de una petición)

```text
HTTP → FastAPI routing        (decoradores @router.get/@router.post… deciden el método)
     → validación Pydantic    (esquemas Crear/Reemplazo/Actualizar → 422 automático)
     → Controlador (controlador_v1.py) (delega al servicio ensamblado y retorna JSON)
     → Servicio               (servicio_v1.py — recibe IRepositorio*, sin conocer PyMSSQL)
     → IRepositorio* (ABC)    (interfaces.py — abstracción pura)
     → Repositorio*SqlServer  (repositorio_v1.py — PyMSSQL + parámetros %s)
     → SQL Server
```

## 4. Decisiones de diseño clave

### 4.1 Interfaces abstractas (`ABC`) y ensamblador explícito
En `api_python/repositorios/interfaces.py`, cada tabla declara su contrato abstracto (`IRepositorioProducto`, `IRepositorioEmpresa`, etc.). Las implementaciones SQL Server en `repositorio_v1.py` declaran explícitamente su SQL y lista blanca de actualización, y `ensamblador.py` es el único archivo que instancia concretos.

### 4.2 La validación vive en las PETICIONES (una por verbo)
- `ProductoCrear`      → POST: todos obligatorios (con código)
- `ProductoReemplazo`  → PUT: todos obligatorios (el código va en la URL)
- `ProductoActualizar` → PATCH: todos opcionales (se valida lo que llegue)

### 4.3 Manejadores globales de excepciones en `api_python/main.py`
```python
@app.exception_handler(NoEncontradoExcepcion)
async def manejar_404(_req: Request, exc: NoEncontradoExcepcion):
    return JSONResponse(status_code=404, content={"estado": 404, "mensaje": str(exc)})
```

### 4.4 SQL del repositorio (PyMSSQL, siempre parametrizado)
```sql
SELECT TOP (%s) codigo, nombre, stock, valorunitario FROM producto ORDER BY codigo
SELECT ... FROM producto WHERE codigo = %s
INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES (%s, %s, %s, %s)
UPDATE producto SET ... WHERE codigo = %s
DELETE FROM producto WHERE codigo = %s
```

### 4.5 Traducción de excepciones a HTTP
| Situación | HTTP |
|---|---|
| `RequestValidationError` (Pydantic: body con errores de forma) | 422 |
| `ValidacionNegocioExcepcion` | 400 |
| `NoAutenticadoExcepcion` | 401 |
| `SinPermisoExcepcion` | 403 |
| `NoEncontradoExcepcion` | 404 |
| `ConflictoExcepcion` (error SQL 2627/2601 duplicado o 547 FK) | 409 |
| Error inesperado de SQL Server / sistema | 500 |

### 4.6 SQL Server se siembra automáticamente
El servicio `sqlserver-init` ejecuta `db/init.sh`, verifica si la base `bdfacturas_sqlserver_local` existe y si no la crea e importa `db/bdfacturas.sql`.

## 5. Docker: un solo comando desde v1
`docker compose up -d --build` deja corriendo `sqlserver`, `sqlserver-init`, `api-facturas` (puerto `8035`) y `front-python` (puerto `8099`).
