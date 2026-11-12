# Investigación y decisiones — Versión 1: las seis tablas sin clave foránea

> **Versión 1** · **Lectura opcional** (el porqué de las decisiones del plan,
> con las alternativas que se evaluaron y descartaron). Complementa a
> [3_plan.md](3_plan.md); el orden de trabajo está en [8_tasks.md](8_tasks.md).

---

## D1 — El ejecutor de la capa de datos: PyMSSQL (SQL directo)
**Decisión:** PyMSSQL con SQL parametrizado a mano (`%s`) y cursor en modo diccionario (`as_dict=True`). Se descartan ORMs pesados como SQLAlchemy ORM para mantener el SQL completamente visible y auditable.

## D2 — Capas completas desde el día 1
Router (`routers/router_v1.py`) → Servicio (`servicios/servicio_v1.py`) → Repositorio (`repositorios/repositorio_v1.py`) desacoplados y verificables en pruebas unitarias sin base de datos (`pruebas/test_capas.py`).

## D3 — Inyección por composición explícita y estado en contenedor
El ensamblaje en `api_python/main.py` centraliza los manejadores globales de excepciones para traducir errores de negocio en respuestas JSON homogéneas.

## D4 — La BD completa desde la v1
`db/bdfacturas.sql` crea `bdfacturas_sqlserver_local` completa (12 tablas, triggers, SPs) desde el arranque mediante `db/init.sh`.

## D5 — La validación vive en las PETICIONES (una por verbo)
Tres esquemas Pydantic por recurso (`ProductoCrear`, `ProductoReemplazo`, `ProductoActualizar`).

## D6 — Motor en contenedor
Contenedor oficial de SQL Server 2022 inicializado automáticamente por `sqlserver-init`.

## D7 — Recarga en vivo con Uvicorn dentro del contenedor
Imagen `python:3.12-slim` montando `./api_python:/app` y `./front_python:/app` para reflejar cambios al guardar cualquier `.py` o plantilla HTML.

## D8 — Docker Compose desde la v1
`docker compose up -d --build` levanta BD, API Python (`8035`) y Frontend Python (`8099`).
