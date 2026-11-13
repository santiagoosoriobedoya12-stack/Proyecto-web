# Plan de Arquitectura — Versión 1

> Plan de diseño y separación de responsabilidades en Python para las 8 tablas de la Versión 1.

---

## 1. Arquitectura de Componentes (Python)

```text
api_mapa/
├── controladores/controlador_v1.py   # Routers FastAPI (/api/proyecto, /api/area-conocimiento, etc.)
├── servicios/servicio_v1.py          # Lógica de dominio y validaciones
├── repositorios/
│   ├── interfaces.py                 # Contratos abstractos ABC (IRepositorioProyecto, etc.)
│   ├── repositorio_v1.py             # Implementaciones MariaDB con PyMySQL y filtro activo = 1
│   └── helpers_sql.py                # Constructor de sentencias UPDATE dinámicas parametrizadas
├── modelos/entidades.py              # Entidades @dataclass
├── peticiones/esquemas.py            # DTOs Pydantic (*Crear, *Reemplazo, *Actualizar)
└── database.py                       # Conexión PyMySQL (DictCursor, CLIENT.FOUND_ROWS)

front_flask/
├── app.py                            # Rutas explícitas /proyectos, /ods, etc.
├── catalogo_vistas.py                # Definición de campos, etiquetas y columnas
├── templates/recurso_crud.html       # Plantilla unificada para CRUDs v1
└── static/css/                       # Bootstrap 5 local y app.css
```

---

## 2. Decisiones Técnicas Fundamentales
1. **Borrado Lógico Obligatorio:** Agregar `activo TINYINT(1) DEFAULT 1` al esquema SQL original para cumplir §7.0 de la metodología.
2. **FOUND_ROWS en MariaDB:** Bandera `CLIENT.FOUND_ROWS` activa en la conexión para evitar falsos 404 al hacer `PUT` con los mismos datos.
3. **Manejo Centralizado de Excepciones:** `main.py` registra handlers para traducir `NoEncontradoExcepcion` -> 404, `ConflictoExcepcion` -> 409 y `ValidacionNegocioExcepcion` -> 400.
