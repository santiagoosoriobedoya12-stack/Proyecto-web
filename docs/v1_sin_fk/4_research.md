# Investigación y decisiones — Versión 1: las seis tablas sin clave foránea

> **Versión 1** · **Lectura opcional** (el porqué de las decisiones del plan,
> con las alternativas que se evaluaron y descartaron). Complementa a
> [3_plan.md](3_plan.md); el orden de trabajo está en [8_tasks.md](8_tasks.md).

---

## D1 — El ejecutor de la capa de datos: Dapper (un ADR completo)
**Decisión:** Dapper con SQL parametrizado a mano (`@param`). EF Core queda descartado para no ocultar el SQL.

## D2 — Capas completas desde el día 1
Controller → Servicio → Repositorio con interfaces desde v1.

## D3 — Sin fábrica ni selección de motor
Dos registros `AddScoped` en `Program.cs`.

## D4 — La BD completa desde la v1
`db/bdfacturas.sql` crea `bdfacturas` completa (12 tablas, triggers, SPs).

## D5 — La validación vive en las PETICIONES (una por verbo)
Tres clases de petición (`ProductoCrear`, `ProductoReemplazo`, `ProductoActualizar`).

## D6 — Motor en contenedor
Contenedor oficial en Docker, ejecuta scripts montados en `/docker-entrypoint-initdb.d/`.

## D7 — dotnet watch dentro del contenedor
Imagen SDK con recarga en vivo y volúmenes anónimos para `bin/` y `obj/`.

## D8 — Docker compose desde la v1
`docker compose up -d --build` para levantar BD, API y Frontend.
