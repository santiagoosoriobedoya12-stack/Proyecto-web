# Especificación — Versión 2: Las 10 Tablas Con Claves Foráneas y Maestro-Detalle

> **Versión 2** del desarrollo incremental: Crecimiento sobre la v1 ($v1 \subset v2$) incorporando integridad referencial, validación de padres activos (`409 Conflict`), tablas intermedias M:N y concurrencia en Flask.

---

## 1. Propósito de la v2
Extender la solución de la versión 1 sin romper sus contratos existentes, integrando las 10 tablas que dependen de claves foráneas:
- `docente` (FK opcional a `linea_investigacion`).
- `producto` (FK opcional a `proyecto`, FK obligatoria a `tipo_producto`).
- `desarrolla` (Participación de docentes en proyectos con atributos `rol` y `descripcion`).
- Las 7 tablas puente muchos a muchos (aliados, palabras clave, áreas de conocimiento, líneas, ODS, áreas de aplicación, docentes por producto).

---

## 2. Requisitos Funcionales
- **RF1 — Validación de Integridad Referencial:** Antes de insertar o actualizar cualquier FK, el servicio verifica que el registro padre exista y tenga `activo = 1`. Si no cumple, lanza `ConflictoExcepcion` (HTTP 409 Conflict).
- **RF2 — Prevención de Duplicados en Vínculos M:N:** La combinación de claves en tablas puente es PK compuesta. Intentar asociar un par existente responde HTTP 409 Conflict.
- **RF3 — Maestro-Detalle en `desarrolla`:** Permite asociar docentes y proyectos con su rol y descripción, consultar participaciones por docente o por proyecto, actualizar atributos y retirar lógicamente.
- **RF4 — Factoría de Rutas para Tablas Puente:** 5 endpoints REST explícitos por cada relación intermedia (`GET ""` lista todos, `GET "/{izq}/{val}"`, `GET "/{der}/{val}"`, `POST ""` asocia, `DELETE "/{izq}/{der}"` retira).
- **RF5 — Frontend Concurrente en Flask:** Vistas en Flask con `ThreadPoolExecutor` para cargar en paralelo los catálogos foráneos necesarios para los `<select>` de docentes, proyectos y tipos de producto.

---

## 3. Requisitos No Funcionales
- **RNF1 — Crecimiento Acumulativo:** Ninguna ruta de la v1 sufre cambios destructivos ni de contrato.
- **RNF2 — Captura de Códigos MariaDB:** Interceptar errores nativos (1062 duplicado, 1452 FK inválida, 1451 dependientes) traduciéndolos a HTTP 409.
- **RNF3 — Desacoplamiento del Frontend:** Carga de selects mediante peticiones HTTP asíncronas / concurrentes hacia la API sin acceso directo a MariaDB.
