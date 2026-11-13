# Especificación — Versión 1: Las 8 Tablas Sin Clave Foránea

> **Versión 1** del desarrollo incremental del módulo **Mapa de Conocimiento**.
> En v1 se construye el corte vertical completo en **Python** para las 8 tablas independientes con borrado lógico y frontend en Flask.

---

## 1. Propósito de la v1
Construir el primer corte vertical completo en **Python 3.12 (FastAPI + Flask + MariaDB)** para las 8 entidades sin claves foráneas del módulo **Mapa de Conocimiento**, estableciendo la arquitectura de capas definitiva:
`Controladores FastAPI` → `Servicios de Dominio` → `Repositorios Abstractos (ABC)` → `MariaDB 11.4`.

---

## 2. Alcance (8 Tablas Independientes)
1. `area_conocimiento` (id, gran_area, area, disciplina)
2. `objetivo_desarrollo_sostenible` (id, nombre, categoria — 17 ODS precargados)
3. `area_aplicacion` (id, nombre)
4. `termino_clave` (termino PK, termino_ingles)
5. `linea_investigacion` (id AUTO_INCREMENT, nombre, descripcion)
6. `aliado` (nit PK, razon_social, nombre_contacto, correo, telefono, ciudad)
7. `tipo_producto` (id, categoria, clase, nombre, tipologia)
8. `proyecto` (id, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin)

Todas las tablas implementan **borrado lógico obligatorio**: columna `activo TINYINT(1) NOT NULL DEFAULT 1` ([docs/spec_kit/1_constitution.md](../spec_kit/1_constitution.md)).

---

## 3. Requisitos Funcionales
- **RF1 — Listado General:** `GET /api/{recurso}?limite=N` retorna el sobre estándar `{ "tabla", "limite", "total", "datos" }` (HTTP 200) o HTTP 204 No Content si está vacío. Solo retorna registros con `activo = 1`.
- **RF2 — Consulta Individual:** `GET /api/{recurso}/{clave}` retorna la entidad activa (HTTP 200) o HTTP 404 Not Found si no existe o fue retirada.
- **RF3 — Creación:** `POST /api/{recurso}` valida el DTO `*Crear` e inserta con `activo = 1` (HTTP 201 Created). Si la clave primaria ya existe, responde HTTP 409 Conflict.
- **RF4 — Reemplazo Idempotente:** `PUT /api/{recurso}/{clave}` reemplaza todos los campos de negocio (HTTP 200 OK). Gracias a `CLIENT.FOUND_ROWS`, si se envían los mismos datos responde 200 y no un falso 404.
- **RF5 — Actualización Parcial:** `PATCH /api/{recurso}/{clave}` actualiza solo los campos enviados (HTTP 200 OK). Si el cuerpo llega vacío responde HTTP 400 Bad Request.
- **RF6 — Retiro Lógico:** `DELETE /api/{recurso}/{clave}` ejecuta `UPDATE ... SET activo = 0 WHERE clave = %s AND activo = 1` (HTTP 204 No Content). Si se repite, responde HTTP 404 Not Found.
- **RF7 — Interfaz Gráfica en Flask:** Pantallas en `http://localhost:8116/{recurso}` conectadas a la API vía HTTP con Bootstrap local y botones no técnicos ("Guardar la ficha completa", "Guardar solo lo que cambié", "Retirar").

---

## 4. Requisitos No Funcionales
- **RNF1 — Stack 100% Python:** Backend FastAPI y Frontend Flask desacoplados sin mezclar dependencias.
- **RNF2 — Capas Estrictas y POO:** Uso de `abc.ABC`, composición sobre herencia e inyección en `ensamblador.py`.
- **RNF3 — SQL Parametrizado:** Sentencias SQL parametrizadas con `%s` en MariaDB (cero concatenación).
- **RNF4 — Aislamiento del Frontend:** `front_flask` no importa `pymysql` ni recibe credenciales de base de datos.
- **RNF5 — DTOs Segregados:** Esquemas Pydantic `*Crear`, `*Reemplazo` y `*Actualizar` con validación de tipos (HTTP 422).
