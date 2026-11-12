# Modelo de Datos — Versión 2 (10 Tablas con Clave Foránea y Puentes)

> Todas las tablas de `v2` incluyen `activo TINYINT(1) NOT NULL DEFAULT 1` en [`db/init.sql`](../../db/init.sql) conforme a `0_METODOLOGIA.md` §7.0. Además de la integridad referencial física de MariaDB (`InnoDB`), [`servicio_v2.py`](../../api_mapa/servicios/servicio_v2.py) valida que los registros referenciados existan con `activo = 1` antes de insertar o actualizar, retornando `409 Conflict` si un padre está retirado lógicamente.

## Tablas con Atributos y FK
1. `docente` (`cedula` PK; `linea_investigacion_principal` FK nullable -> `linea_investigacion.id`; conserva los nombres de columna exactos del DDL oficial, incluyendo `nacionalidaad`, `cat_minciencia`, `conv_minciencia`, `url_cvlac`).
2. `producto` (`id` PK; `proyecto` FK nullable -> `proyecto.id`; `tipo_producto` FK obligatoria -> `tipo_producto.id`).
3. `desarrolla` (PK compuesta `(docente, proyecto)` con atributos `rol` y `descripcion`).

## Las 7 Tablas Puente M:N (PK Compuesta)
4. `aliado_proyecto` (`aliado`, `proyecto`)
5. `palabras_clave` (`proyecto`, `termino_clave`)
6. `ac_proyecto` (`proyecto`, `area_conocimiento`)
7. `proyecto_linea` (`proyecto`, `linea_investigacion`)
8. `ods_proyecto` (`proyecto`, `ods`)
9. `aa_proyecto` (`proyecto`, `area_aplicacion`)
10. `docente_producto` (`docente`, `producto`)
