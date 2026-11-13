# Documento Maestro de Arquitectura — Mapa de Conocimiento

Este proyecto implementa en **Python puro** el módulo **Mapa de Conocimiento** (`mapa_conocimiento`, **21 tablas**) siguiendo la arquitectura de capas, el paradigma orientado a objetos y el desarrollo incremental por versiones ($v1 \subset v2 \subset v3$). El diagrama entidad-relación detallado se encuentra en [DER.md](DER.md).

---

## 1. Inventario de las 21 Tablas por Versión

### Versión 1 — 8 Tablas sin Clave Foránea
Todas incorporan `activo TINYINT(1) NOT NULL DEFAULT 1` para cumplir el retiro lógico obligatorio:

1. `area_conocimiento` (`id` PK manual, `gran_area`, `area`, `disciplina`)
2. `objetivo_desarrollo_sostenible` (`id` PK manual, `nombre`, `categoria`) — Precargada con los 17 ODS oficiales en [init.sql](../db/init.sql).
3. `area_aplicacion` (`id` PK manual, `nombre`)
4. `termino_clave` (`termino` PK `VARCHAR(30)`, `termino_ingles`)
5. `linea_investigacion` (`id` `AUTO_INCREMENT`, `nombre`, `descripcion`)
6. `aliado` (`nit` PK manual, `razon_social`, `nombre_contacto`, `correo`, `telefono`, `ciudad`)
7. `tipo_producto` (`id` PK manual, `categoria`, `clase`, `nombre`, `tipologia`)
8. `proyecto` (`id` PK manual, `titulo`, `resumen`, `presupuesto`, `tipo_financiacion`, `tipo_fondos`, `fecha_inicio`, `fecha_fin`)

### Versión 2 — 10 Tablas con Clave Foránea y Maestro-Detalle
También incorporan `activo TINYINT(1) NOT NULL DEFAULT 1` y validan que los registros padres referenciados existan y estén activos (`activo = 1`), devolviendo HTTP `409 Conflict` cuando la referencia es inválida o ya existe el vínculo:

9. `docente` (`cedula` PK manual; FK opcional `linea_investigacion_principal` -> `linea_investigacion.id`)
10. `producto` (`id` PK manual; FK opcional `proyecto` -> `proyecto.id`; FK obligatoria `tipo_producto` -> `tipo_producto.id`)
11. `desarrolla` (PK compuesta `(docente, proyecto)` + atributos propios `rol`, `descripcion`)
12. `aliado_proyecto` (PK compuesta `(aliado, proyecto)`)
13. `palabras_clave` (PK compuesta `(proyecto, termino_clave)`)
14. `ac_proyecto` (PK compuesta `(proyecto, area_conocimiento)`)
15. `proyecto_linea` (PK compuesta `(proyecto, linea_investigacion)`)
16. `ods_proyecto` (PK compuesta `(proyecto, ods)`)
17. `aa_proyecto` (PK compuesta `(proyecto, area_aplicacion)`)
18. `docente_producto` (PK compuesta `(docente, producto)`)

### Versión 3 — 3 Tablas de Seguridad, JWT y Control de Acceso (RBAC)
19. `rol` (`id` `AUTO_INCREMENT`, `nombre`, `descripcion`, `activo`)
20. `usuario` (`id` `AUTO_INCREMENT`, `username`, `password_hash` con BCrypt, `email`, `nombre_completo`, `activo`)
21. `rol_usuario` (PK compuesta `(usuario_id, rol_id)`)

---

## 2. Puertos y Contenedores (`docker-compose.yml`)

| Servicio | Tecnología | Puerto Host | Puerto Contenedor | Responsabilidad |
| :--- | :--- | :---: | :---: | :--- |
| `mariadb` | MariaDB 11.4 | `13333` | `3306` | Motor relacional `mapa_conocimiento` |
| `phpmyadmin` | phpMyAdmin | `8108` | `80` | Inspección visual de tablas y datos |
| `api-mapa` | Python 3.12 + FastAPI | `8117` | `8117` | API REST en capas, validación Pydantic y JWT |
| `front-flask` | Python 3.12 + Flask | `8116` | `8116` | Vistas HTML Jinja2 y cliente HTTP (`httpx`) |

---

## 3. Aislamiento del Frontend (`front_flask`)

El contenedor `front-flask`:
- **No recibe** variables de entorno de MariaDB ni instala `pymysql`.
- Toda lectura y escritura viaja por HTTP a `API_BASE_URL` (`http://api-mapa:8117`).
- Guarda el JWT únicamente en el servidor Flask (`SESIONES` en memoria indexado por cookie `HttpOnly` `mapa_sid`).
- Usa `ThreadPoolExecutor` en vistas con múltiples catálogos foráneos para ejecutar las consultas en paralelo.
