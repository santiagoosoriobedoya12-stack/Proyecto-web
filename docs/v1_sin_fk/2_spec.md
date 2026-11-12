# Especificación — Versión 1: las SEIS tablas sin clave foránea, con su interfaz gráfica

> **Versión 1** del desarrollo incremental.
> En v1 el sistema completo ES esto: la BD `bdfacturas` sí se crea COMPLETA desde el inicio — es infraestructura dada.

## 1. Propósito de la v1
Construir la primera rebanada vertical de la API de facturación en **Python (FastAPI + PyMSSQL)**: el CRUD completo de las tablas independientes comenzando por `producto` contra el motor de base de datos, con arquitectura en 3 capas desde el primer día: router → servicio → repositorio.

## 2. Alcance
CRUD de los recursos sin clave foránea e interfaz gráfica asociada en `front_python/`:
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
- **RNF1 — Capas estrictas** (`routers` -> `servicios` -> `repositorios`)
- **RNF2 — SQL a la vista** (sin ORM oculto)
- **RNF3 — SQL siempre parametrizado** (`%s` vía PyMSSQL)
- **RNF4 — Concurrencia y separación de procesos** (`front_python` independiente vía `httpx`)
- **RNF5 — Errores uniformes** (`400`, `404`, `409`, `422`, `500`)
- **RNF6 — Bootstrap y estilos corporativos servidos localmente** (sin CDN)

## 5. Criterios de aceptación
1. `docker compose up -d --build` deja corriendo la BD y la API en `http://localhost:8035/`.
2. Listado y límites funcionan según contrato.
3. Ciclo completo de los 5 verbos funcional.
4. Validación `422` en peticiones de entrada (Pydantic).
5. Frontend Python en `http://localhost:8099/productos` funcional de punta a punta.
