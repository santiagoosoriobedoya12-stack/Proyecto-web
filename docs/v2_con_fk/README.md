# Versión 2 — 10 Tablas con Claves Foráneas y Maestro-Detalle

Crece sobre `v1` incorporando `docente`, `producto`, `desarrolla` y las 7 tablas puente M:N con validación de padres activos (`409 Conflict`) y carga paralela de catálogos foráneos en Flask mediante `ThreadPoolExecutor`.

- **Modelo de datos:** [5_data_model.md](5_data_model.md)
- **Contratos HTTP:** [6_contracts.md](../spec_kit/versiones/v2_con_fk/6_contracts.md)
- **Guía rápida de prueba:** [7_quickstart.md](7_quickstart.md)

## Archivos Principales
- [`api_mapa/repositorios/repositorio_v2.py`](../../api_mapa/repositorios/repositorio_v2.py)
- [`api_mapa/servicios/servicio_v2.py`](../../api_mapa/servicios/servicio_v2.py)
- [`api_mapa/controladores/controlador_v2.py`](../../api_mapa/controladores/controlador_v2.py)
- [`front_flask/templates/desarrolla.html`](../../front_flask/templates/desarrolla.html) + [`front_flask/templates/puente.html`](../../front_flask/templates/puente.html)
