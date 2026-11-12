# Tareas — Versión 1: las seis tablas sin FK, con su interfaz gráfica

> **Versión 1** · El orden de construcción, partiendo de CERO. Cada fase
> termina en algo **verificable**. Requisitos: [2_spec.md](2_spec.md) ·
> técnica: [3_plan.md](3_plan.md) · contratos: [6_contracts.md](6_contracts.md) ·
> validación final: [7_quickstart.md](7_quickstart.md).

---

## Fase 0 — Base de datos y esqueleto
- [x] Copiar a `db/` el archivo **provisto** con esta versión: `bdfacturas.sql` (la BD completa) y `init.sh`.
- [x] Configurar `docker-compose.yml` con los servicios `sqlserver` (puerto 11463) y `sqlserver-init` para auto-siembra idempotente.
- [x] Crear `api_python/` con `peticiones/`, `modelos/`, `controladores/`, `servicios/`, `repositorios/`, `autorizacion/`, `ensamblador.py` y `pruebas/`.

**Verificar:** `docker compose ps` muestra `sqlserver` healthy y `SELECT count(*) FROM producto` devuelve 8.

## Fase 1 — El proyecto FastAPI y la configuración base
- [x] `api_python/requirements.txt` con `fastapi`, `uvicorn`, `pymssql` y `pydantic[email]`.
- [x] `api_python/config.py` y `api_python/database.py` para lectura de variables de entorno, conexión a SQL Server y normalización de claves minúsculas.

## Fase 2 — Las peticiones por verbo (frontera de entrada) y excepciones
- [x] `api_python/peticiones/esquemas.py`: esquemas Pydantic segregados por verbo (`*Crear`, `*Reemplazo`, `*Actualizar`) para `producto`, `empresa`, `persona`, `rol`, `ruta` y `usuario`.
- [x] `api_python/excepciones.py`: excepciones de dominio (`NoEncontradoExcepcion`, `ValidacionNegocioExcepcion`, `ConflictoExcepcion`).

## Fase 3 — Repositorios de datos (PyMSSQL)
- [x] `api_python/repositorios/interfaces.py` y `repositorio_v1.py`: interfaces `ABC` (`IRepositorio*`) y repositorios concretos (`RepositorioProductoSqlServer`, `RepositorioEmpresaSqlServer`, `RepositorioPersonaSqlServer`, `RepositorioRolSqlServer`, `RepositorioRutaSqlServer`, `RepositorioUsuarioSqlServer`) usando SQL parametrizado con `%s`.

## Fase 4 — Servicios de dominio (y prueba de capas en memoria)
- [x] `api_python/servicios/servicio_v1.py`: `ServicioProducto`, `ServicioEmpresa`, `ServicioPersona`, `ServicioRol`, `ServicioRuta` y `ServicioUsuario` (con hash BCrypt costo 12 al guardar usuario).
- [x] `api_python/pruebas/test_capas.py`: pruebas unitarias con `unittest` usando repositorios falsos en memoria (sin SQL Server).

## Fase 5 — Controladores y ensamblaje en `main.py`
- [x] `api_python/controladores/controlador_v1.py` y `api_python/ensamblador.py`: endpoints CRUD completos de los 6 recursos (`200`, `201`, `400`, `404`, `409`, `422`, `500`).
- [x] `api_python/main.py`: aplicación FastAPI, handlers globales de excepción, documentación en `/swagger` y diagnóstico `GET /`.

## Fase 6 — La interfaz gráfica (Flask + Jinja2)
- [x] Proyecto `front_python/` expuesto en puerto `8099` (`8096` interno) sin `pymssql` ni acceso directo a SQL Server.
- [x] `front_python/servicios/cliente_api.py` y `servicios_recursos.py`: cliente HTTP y funciones explícitas por operación.
- [x] `front_python/templates/base.html` y `crud_recurso.html` + Bootstrap local en `front_python/static/css/bootstrap.min.css` y `app.css`.

## Fase 7 — Prueba de resiliencia
- [x] Detener `api-facturas` y validar que el frontend sigue respondiendo en el navegador mostrando alerta de error de conexión sin romperse.

## Fase 8 — Docker: un solo comando
- [x] Dockerfiles de `api_python` y `front_python` con `uvicorn` orquestados en `docker-compose.yml`.

## Fase 9 — Cierre de la versión
- [x] Verificación funcional de todos los criterios de v1.
- [x] Documentación y guía de sustentación actualizadas.
