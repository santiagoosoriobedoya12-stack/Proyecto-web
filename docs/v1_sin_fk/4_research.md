# Investigaci├│n y decisiones ÔÇö Versi├│n 1: las seis tablas sin clave for├ínea

> **Versi├│n 1** ┬À **Lectura opcional** (el porqu├® de las decisiones del plan,
> con las alternativas que se evaluaron y descartaron). Complementa a
> [3_plan.md](3_plan.md); el orden de trabajo est├í en [8_tasks.md](8_tasks.md).

---

## D1 ÔÇö El ejecutor de la capa de datos: Dapper (un ADR completo)
**Decisi├│n:** Dapper con SQL parametrizado a mano (`@param`). EF Core queda descartado para no ocultar el SQL.

## D2 ÔÇö Capas completas desde el d├¡a 1
Controller ÔåÆ Servicio ÔåÆ Repositorio con interfaces desde v1.

## D3 ÔÇö Sin f├íbrica ni selecci├│n de motor
Dos registros `AddScoped` en `Program.cs`.

## D4 ÔÇö La BD completa desde la v1
`db/bdfacturas.sql` crea `bdfacturas` completa (12 tablas, triggers, SPs).

## D5 ÔÇö La validaci├│n vive en las PETICIONES (una por verbo)
Tres clases de petici├│n (`ProductoCrear`, `ProductoReemplazo`, `ProductoActualizar`).

## D6 ÔÇö Motor en contenedor
Contenedor oficial en Docker, ejecuta scripts montados en `/docker-entrypoint-initdb.d/`.

## D7 ÔÇö dotnet watch dentro del contenedor
Imagen SDK con recarga en vivo y vol├║menes an├│nimos para `bin/` y `obj/`.

## D8 ÔÇö Docker compose desde la v1
`docker compose up -d --build` para levantar BD, API y Frontend.
