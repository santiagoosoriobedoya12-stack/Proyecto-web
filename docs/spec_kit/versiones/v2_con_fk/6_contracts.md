# Contratos HTTP — Versión 2 (10 Tablas con Clave Foránea)

Definidos en [`controlador_v2.py`](../../../../api_mapa/controladores/controlador_v2.py) sobre las garantías de `v1`:

## 1. Entidades Principales con FK
- `/api/docente` (PK `cedula: int`; FK opcional `linea_investigacion_principal` -> `linea_investigacion.id`)
- `/api/producto` (PK `id: int`; FK opcional `proyecto` -> `proyecto.id`; FK obligatoria `tipo_producto` -> `tipo_producto.id`)

Regla de negocio (`servicio_v2.py`): cualquier FK enviada distinta de `None` debe existir con `activo = 1`; en caso contrario responde **HTTP `409 Conflict`**.

## 2. Participación con Atributos (`desarrolla`)
- `GET /api/desarrolla`, `GET /api/desarrolla/docente/{cedula}`, `GET /api/desarrolla/proyecto/{id_proyecto}`
- `POST /api/desarrolla` -> crea vínculo `(docente, proyecto)` con `rol` y `descripcion` (`201` o `409` si ya existe o si un padre está inactivo)
- `PUT /api/desarrolla/{cedula}/{id_proyecto}` y `PATCH /api/desarrolla/{cedula}/{id_proyecto}`
- `DELETE /api/desarrolla/{cedula}/{id_proyecto}` -> retiro lógico `activo = 0` (`204` o `404`)

## 3. Las 7 Tablas Puente M:N
Cada tabla expone: `GET ""`, `GET "/{col_izq}/{val}"`, `GET "/{col_der}/{val}"`, `POST ""` (`201`/`409`) y `DELETE "/{val_izq}/{val_der}"` (`204`/`404`):
- `/api/aliado-proyecto` (`aliado`, `proyecto`)
- `/api/palabras-clave` (`proyecto`, `termino_clave`)
- `/api/ac-proyecto` (`proyecto`, `area_conocimiento`)
- `/api/proyecto-linea` (`proyecto`, `linea_investigacion`)
- `/api/ods-proyecto` (`proyecto`, `ods`)
- `/api/aa-proyecto` (`proyecto`, `area_aplicacion`)
- `/api/docente-producto` (`docente`, `producto`)
