# Versión 1 — 8 Tablas sin Clave Foránea

Implementa el vertical slice completo (MariaDB → Repositorio → Servicio → Controlador FastAPI → Vista Flask) para las 8 tablas independientes del módulo **Mapa de Conocimiento**.

- **Modelo de datos:** [5_data_model.md](5_data_model.md)
- **Contratos HTTP:** [6_contracts.md](../spec_kit/versiones/v1_sin_fk/6_contracts.md)
- **Guía rápida de prueba:** [7_quickstart.md](7_quickstart.md)

## Archivos Principales
- [`api_mapa/repositorios/repositorio_v1.py`](../../api_mapa/repositorios/repositorio_v1.py)
- [`api_mapa/servicios/servicio_v1.py`](../../api_mapa/servicios/servicio_v1.py)
- [`api_mapa/controladores/controlador_v1.py`](../../api_mapa/controladores/controlador_v1.py)
- [`front_flask/catalogo_vistas.py`](../../front_flask/catalogo_vistas.py) + [`front_flask/templates/recurso_crud.html`](../../front_flask/templates/recurso_crud.html)
