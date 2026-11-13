# Lista de Tareas (Tasks) — Versión 2

- [x] **T2.1:** Extender `db/init.sql` con las 10 tablas de v2 con claves foráneas, PKs compuestas y borrado lógico.
- [x] **T2.2:** Definir modelos `Docente`, `Producto`, `Desarrolla` en `api_mapa/modelos/entidades.py`.
- [x] **T2.3:** Crear DTOs Pydantic para `Docente`, `Producto`, `Desarrolla` y las 7 tablas puente en `api_mapa/peticiones/esquemas.py`.
- [x] **T2.4:** Crear interfaces `IRepositorioDocente`, `IRepositorioProducto`, `IRepositorioDesarrolla` e `IRepositorioPuente` en `api_mapa/repositorios/interfaces.py`.
- [x] **T2.5:** Implementar repositorios MariaDB en `api_mapa/repositorios/repositorio_v2.py`.
- [x] **T2.6:** Implementar servicios `ServicioDocente`, `ServicioProducto`, `ServicioDesarrolla` y `ServicioPuente` con validación de padres activos (`ConflictoExcepcion` -> 409).
- [x] **T2.7:** Inyectar nuevos servicios en `api_mapa/ensamblador.py`.
- [x] **T2.8:** Crear routers FastAPI explícitos y factoría `_crear_router_puente` en `api_mapa/controladores/controlador_v2.py`.
- [x] **T2.9:** Implementar `ThreadPoolExecutor` en `front_flask/app.py` y crear plantillas `desarrolla.html` y `puente.html`.
- [x] **T2.10:** Escribir pruebas unitarias de FKs y polimorfismo en `api_mapa/pruebas/test_capas.py`.
