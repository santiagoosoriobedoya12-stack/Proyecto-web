# Lista de Tareas (Tasks) — Versión 1

- [x] **T1.1:** Crear script `db/init.sql` con las 8 tablas de v1 + columna `activo TINYINT(1) DEFAULT 1` y los 17 ODS precargados.
- [x] **T1.2:** Implementar capa de infraestructura `api_mapa/database.py` con `CLIENT.FOUND_ROWS` y `DictCursor`.
- [x] **T1.3:** Crear entidades de dominio `@dataclass` en `api_mapa/modelos/entidades.py`.
- [x] **T1.4:** Definir esquemas Pydantic `*Crear`, `*Reemplazo`, `*Actualizar` en `api_mapa/peticiones/esquemas.py`.
- [x] **T1.5:** Definir interfaces abstractas `abc.ABC` en `api_mapa/repositorios/interfaces.py`.
- [x] **T1.6:** Implementar repositorios MariaDB con consultas SQL parametrizadas en `api_mapa/repositorios/repositorio_v1.py`.
- [x] **T1.7:** Implementar servicios de dominio con lógica de negocio en `api_mapa/servicios/servicio_v1.py`.
- [x] **T1.8:** Conectar capas en `api_mapa/ensamblador.py` (Composition Root).
- [x] **T1.9:** Implementar routers FastAPI en `api_mapa/controladores/controlador_v1.py`.
- [x] **T1.10:** Crear servidor Flask `front_flask/app.py`, `catalogo_vistas.py` y plantilla Jinja2 `recurso_crud.html`.
- [x] **T1.11:** Escribir pruebas unitarias con repositorios en memoria en `api_mapa/pruebas/test_capas.py`.
