# Contratos HTTP — Versión 1 (8 Tablas sin Clave Foránea)

Todos los endpoints viven en [`controlador_v1.py`](../../api_mapa/controladores/controlador_v1.py) con esquemas Pydantic segregados en [`esquemas.py`](../../api_mapa/peticiones/esquemas.py).

| Recurso | Prefijo REST | PK (Tipo) | Auto-Inc |
| :--- | :--- | :--- | :---: |
| Área de Conocimiento | `/api/area-conocimiento` | `id` (`int`) | No |
| ODS | `/api/objetivo-desarrollo-sostenible` | `id` (`int`) | No |
| Área de Aplicación | `/api/area-aplicacion` | `id` (`int`) | No |
| Término Clave | `/api/termino-clave` | `termino` (`str`) | No |
| Línea de Investigación | `/api/linea-investigacion` | `id` (`int`) | Sí |
| Aliado | `/api/aliado` | `nit` (`int`) | No |
| Tipo de Producto | `/api/tipo-producto` | `id` (`int`) | No |
| Proyecto | `/api/proyecto` | `id` (`int`) | No |

## Verbos Estándar por Recurso
- `GET ""` -> `200 OK` (`{"tabla", "limite", "total", "datos"}`) o `204 No Content` si vacío (solo filas con `activo = 1`).
- `GET "/{clave}"` -> `200 OK` o `404 Not Found`.
- `POST ""` -> `201 Created` o `409 Conflict` si la PK ya existe, o `422` por esquema inválido.
- `PUT "/{clave}"` -> `200 OK` (idempotente gracias a `CLIENT.FOUND_ROWS`) o `404 Not Found`.
- `PATCH "/{clave}"` -> `200 OK` (`{"mensaje", "filas_afectadas"}`), `400 Bad Request` si el cuerpo llega vacío, `404` si no existe.
- `DELETE "/{clave}"` -> `204 No Content` (retiro lógico `activo = 0`) o `404 Not Found`.
