# Proyecto de Aula — Módulo: Mapa de Conocimiento

Sistema integral de información para gestionar proyectos de investigación, docentes, productos académicos, alianzas externas y taxonomías del módulo **Mapa de Conocimiento** (**21 tablas**: 18 del módulo + 3 de gestión de usuarios). Desarrollado **100% en Python** bajo la **metodología SDD incremental acumulativa** de 3 versiones ($v1 \subset v2 \subset v3$), con API REST (**FastAPI + PyMySQL + PyJWT + BCrypt**), Frontend Web (**Flask + Jinja2 + httpx + Bootstrap local**) y Base de Datos (**MariaDB 11.4 + phpMyAdmin**) orquestados con **Docker Compose**.

---

## 1. Mapa y Estado de Versiones ($v1 \subset v2 \subset v3$)

Cada versión crece sobre la anterior sin romperla, con su API REST, su interfaz web en Flask y su especificación SDD:

| Versión | Alcance Acumulativo | Tablas (21 en total) | Documentación |
| :--- | :--- | :--- | :---: |
| **v1** | 8 tablas **sin clave foránea** + borrado lógico (`activo = 1/0`), validación HTTP 422/404/409 y cliente `CLIENT.FOUND_ROWS` | `area_conocimiento`, `objetivo_desarrollo_sostenible`, `area_aplicacion`, `termino_clave`, `linea_investigacion`, `aliado`, `tipo_producto`, `proyecto` | [Docs v1](docs/v1_sin_fk/README.md) |
| **v2** | 10 tablas **con clave foránea**: validación de padres activos (`409 Conflict`), carga paralela (`ThreadPoolExecutor`) y maestro-detalle M:N | `docente`, `producto`, `desarrolla`, `aliado_proyecto`, `palabras_clave`, `ac_proyecto`, `proyecto_linea`, `ods_proyecto`, `aa_proyecto`, `docente_producto` | [Docs v2](docs/v2_con_fk/README.md) |
| **v3** | Autenticación **JWT**, contraseñas con **BCrypt**, semilla automática y control **RBAC** (`Administrador`, `Investigador`, `Consulta`) con HTTP `401` / `403` | `rol`, `usuario`, `rol_usuario` | [Docs v3](docs/v3_seguridad_rbac/README.md) |

---

## 2. Documentación y Guías del Curso

- [Diagrama Entidad-Relación (DER) de las 21 Tablas (`docs/DER.md`)](docs/DER.md): Modelo entidad-relación en Mermaid con las 21 tablas, atributos, tipos y cardinalidades.
- [Documento Maestro de Arquitectura (`docs/README.md`)](docs/README.md): Mapa de las 21 tablas, puertos y matriz de roles.
- [Guía de Retroalimentación y Sustentación (`docs/RETROALIMENTACION_Y_SUSTENTACION.md`)](docs/RETROALIMENTACION_Y_SUSTENTACION.md): Decisiones de diseño y respuestas técnicas para la sustentación.
- [Paradigma P.O.O. en Python (`docs/PARADIGMA_POO.md`)](docs/PARADIGMA_POO.md): Abstracción (`ABC`), encapsulamiento, composición sobre herencia y polimorfismo en memoria vs MariaDB.
- [Flujo de una Petición (`docs/FLUJO_DE_UNA_PETICION.md`)](docs/FLUJO_DE_UNA_PETICION.md): Trazabilidad completa desde Flask (`8116`) hasta FastAPI (`8117`) y MariaDB (`13333`).
- [SOLID, Capas y Patrones (`docs/SOLID_CAPAS_PATRONES.md`)](docs/SOLID_CAPAS_PATRONES.md): Principios aplicados y separación estricta de responsabilidades.
- [Spec Kit del Proyecto (`docs/spec_kit/1_constitution.md`)](docs/spec_kit/1_constitution.md): Constitución permanente, mapa de versiones y contratos HTTP por versión.

---

## 3. Estructura del Repositorio

```text
├── api_mapa/                  # Backend API REST (Python 3.12 + FastAPI + PyMySQL + PyJWT + BCrypt)
│   ├── main.py                # Punto de entrada FastAPI, Swagger (/swagger) y manejadores 400..500
│   ├── ensamblador.py         # Único punto donde se instancian repositorios MariaDB y servicios
│   ├── config.py              # Lectura de variables de entorno (BD y JWT) sin secretos en código
│   ├── database.py            # Conexión PyMySQL (CLIENT.FOUND_ROWS, DictCursor) y código de error SQL
│   ├── excepciones.py         # Excepciones de dominio desacopladas de HTTP
│   ├── semilla.py             # Semilla idempotente de usuarios demo con hash BCrypt
│   ├── modelos/               # Entidades de dominio POO (21 tablas + Sesion)
│   ├── peticiones/            # Esquemas Pydantic segregados (*Crear, *Reemplazo, *Actualizar) -> HTTP 422
│   ├── repositorios/          # Interfaces ABC (interfaces.py) + implementaciones MariaDB (v1..v3)
│   ├── servicios/             # Lógica de negocio pura compuesta contra IRepositorio* (ABC)
│   ├── controladores/         # Routers REST explícitos (/api/...) de v1, v2 y v3
│   ├── autorizacion/          # Dependencias JWT (401) y verificación de roles (403)
│   └── pruebas/               # Pruebas unitarias con repositorios polimórficos en memoria (test_capas.py)
├── front_flask/               # Frontend Web aislado (Flask + Jinja2 + httpx + Bootstrap local)
│   ├── app.py                 # Rutas explícitas para las 21 vistas + ThreadPoolExecutor
│   ├── catalogo_vistas.py     # Metadatos de campos, FK selects y columnas para cada vista
│   ├── servicios/             # Cliente httpx, sesiones en servidor (HttpOnly mapa_sid) y ops por recurso
│   ├── templates/             # Plantillas Jinja2 (base, home, sesion, recurso_crud, desarrolla, puente)
│   └── static/css/            # Bootstrap 5 local y app.css (sin dependencias de CDN externo)
├── db/
│   ├── init.sql               # DDL completo (21 tablas con borrado lógico activo=1/0) + ODS y datos semilla
│   └── cargar_excel.py        # Importador opcional desde el Excel oficial de catálogos a MariaDB
├── postman/                   # Colección Postman MapaConocimiento_v3.postman_collection.json
├── docs/                      # Documentación conceptual, guías v1..v3 y spec_kit SDD
├── .env.example               # Plantilla de variables de entorno sin credenciales reales
└── docker-compose.yml         # Orquestación de MariaDB (13333), phpMyAdmin (8108), api-mapa (8117) y front-flask (8116)
```

---

## 4. Puesta en Marcha Rápida (Docker Compose)

1. Copia la plantilla de variables de entorno:
   ```powershell
   Copy-Item .env.example .env
   ```
2. Levanta los 4 contenedores:
   ```powershell
   docker compose up -d --build
   ```
3. Accede a los servicios:
   - **Frontend Web (Flask):** <http://localhost:8116>
   - **API REST / Swagger:** <http://localhost:8117/swagger>
   - **phpMyAdmin:** <http://localhost:8108>
   - **MariaDB:** `localhost:13333` (base de datos `mapa_conocimiento`)

### Usuarios Semilla de Prueba (v3)

| Usuario | Contraseña | Rol | Permisos |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | `Administrador` | CRUD completo en las 21 tablas (incluye usuarios y roles) |
| `investigador` | `inv123` | `Investigador` | CRUD completo en las 18 tablas de v1 y v2; HTTP `403` en administración v3 |
| `consulta` | `consulta123` | `Consulta` | Solo lectura (`GET`) en v1 y v2; HTTP `403` en escritura y administración v3 |

### Ejecutar Pruebas de Capas sin Base de Datos

```powershell
docker compose exec api-mapa pytest pruebas/test_capas.py -v
```
