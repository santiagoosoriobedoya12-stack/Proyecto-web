# Plan de Arquitectura — Versión 2

Plan de integración para las 10 tablas dependientes y relacionales sobre la v1:

1. **Modelo de Claves Foráneas y Validación Previa:**
   - La regla se aplica en la capa de servicios (`ServicioDocente`, `ServicioProducto`, `ServicioDesarrolla`, `ServicioPuente`) antes de disparar la consulta SQL.
   - Si la validación falla -> `ConflictoExcepcion` -> HTTP 409 Conflict.
2. **Abstracción de Tablas Puente:**
   - Para las 7 tablas intermedias puras (`aliado_proyecto`, `palabras_clave`, etc.), se implementa `IRepositorioPuente` y `RepositorioPuenteMariaDB`, parametrizado con los nombres de columna y tabla para evitar duplicación de código SQL repetitivo.
3. **Módulo Especial `desarrolla`:**
   - Al contener atributos propios (`rol` y `descripcion`), cuenta con su propio repositorio dedicado (`IRepositorioDesarrolla`), DTOs segregados y plantilla `desarrolla.html`.
4. **Carga Paralela en Frontend:**
   - Función `_cargar_selects_paralelo()` en `front_flask/app.py` utilizando `ThreadPoolExecutor(max_workers=N)` para lanzar consultas HTTP en simultáneo y combinarlas en memoria antes de renderizar Jinja2.
