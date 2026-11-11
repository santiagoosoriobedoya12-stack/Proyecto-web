# Especificación — Versión 1: las SEIS tablas sin clave foránea, con su interfaz gráfica

> **Versión 1** del desarrollo incremental.
> En v1 el sistema completo ES esto: la BD `bdfacturas` sí se crea COMPLETA desde el inicio — es infraestructura dada.

## 1. Propósito de la v1
Construir la primera rebanada vertical de la API de facturación en C# / ASP.NET Core: el CRUD completo de una sola entidad (`producto`) contra el motor de base de datos — con la arquitectura en capas completa desde el primer día: controlador → servicio → repositorio, comunicados por interfaces de C#.

## 2. Alcance
CRUD de los recursos sin clave foránea e interfaz gráfica asociada:
- `producto`
- `empresa`
- `persona`
- `rol`
- `ruta`
- `usuario`

## 3. Requisitos funcionales
- **RF1 — Listar** (`GET /api/producto`)
- **RF2 — Obtener por código** (`GET /api/producto/{codigo}`)
- **RF3 — Crear** (`POST /api/producto`)
- **RF4 — Reemplazar** (`PUT /api/producto/{codigo}`)
- **RF5 — Actualizar parcialmente** (`PATCH /api/producto/{codigo}`)
- **RF6 — Eliminar** (`DELETE /api/producto/{codigo}`)
- **RF7 — Diagnóstico** (`GET /`)

## 4. Requisitos no funcionales
- **RNF1 — Capas estrictas**
- **RNF2 — SQL a la vista**
- **RNF3 — SQL siempre parametrizado**
- **RNF4 — Asíncrona (async/await)**
- **RNF5 — Errores uniformes**
- **RNF6 — Sin anticipación**

## 5. Criterios de aceptación
1. `docker compose up -d --build` deja corriendo la BD y la API en `http://localhost:8032/`.
2. Listado y límites funcionan según contrato.
3. Ciclo completo de los 5 verbos funcional.
4. Validación 422 en peticiones de entrada.
5. Frontend Blazor en `http://localhost:8096/productos` funcional de punta a punta.
