# Especificaci├│n ÔÇö Versi├│n 1: las SEIS tablas sin clave for├ínea, con su interfaz gr├ífica

> **Versi├│n 1** del desarrollo incremental.
> En v1 el sistema completo ES esto: la BD `bdfacturas` s├¡ se crea COMPLETA desde el inicio ÔÇö es infraestructura dada.

## 1. Prop├│sito de la v1
Construir la primera rebanada vertical de la API de facturaci├│n en C# / ASP.NET Core: el CRUD completo de una sola entidad (`producto`) contra el motor de base de datos ÔÇö con la arquitectura en capas completa desde el primer d├¡a: controlador ÔåÆ servicio ÔåÆ repositorio, comunicados por interfaces de C#.

## 2. Alcance
CRUD de los recursos sin clave for├ínea e interfaz gr├ífica asociada:
- `producto`
- `empresa`
- `persona`
- `rol`
- `ruta`
- `usuario`

## 3. Requisitos funcionales
- **RF1 ÔÇö Listar** (`GET /api/producto`)
- **RF2 ÔÇö Obtener por c├│digo** (`GET /api/producto/{codigo}`)
- **RF3 ÔÇö Crear** (`POST /api/producto`)
- **RF4 ÔÇö Reemplazar** (`PUT /api/producto/{codigo}`)
- **RF5 ÔÇö Actualizar parcialmente** (`PATCH /api/producto/{codigo}`)
- **RF6 ÔÇö Eliminar** (`DELETE /api/producto/{codigo}`)
- **RF7 ÔÇö Diagn├│stico** (`GET /`)

## 4. Requisitos no funcionales
- **RNF1 ÔÇö Capas estrictas**
- **RNF2 ÔÇö SQL a la vista**
- **RNF3 ÔÇö SQL siempre parametrizado**
- **RNF4 ÔÇö As├¡ncrona (async/await)**
- **RNF5 ÔÇö Errores uniformes**
- **RNF6 ÔÇö Sin anticipaci├│n**

## 5. Criterios de aceptaci├│n
1. `docker compose up -d --build` deja corriendo la BD y la API en `http://localhost:8032/`.
2. Listado y l├¡mites funcionan seg├║n contrato.
3. Ciclo completo de los 5 verbos funcional.
4. Validaci├│n 422 en peticiones de entrada.
5. Frontend Blazor en `http://localhost:8096/productos` funcional de punta a punta.
