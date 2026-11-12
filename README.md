# Proyecto — Aplicación y Servicios Web

Sistema integral de gestión comercial y facturación desarrollado **100% en Python** bajo la **metodología incremental acumulativa** de 3 versiones ($v1 \subset v2 \subset v3$), con API REST (**Python / FastAPI + PyMSSQL + PyJWT + BCrypt**), Frontend Web (**Python / Flask + Jinja2 + httpx + Bootstrap local**) y Base de Datos (**SQL Server 2022**) orquestados mediante **Docker Compose**.

---

## 🗺️ Mapa y Estado de Versiones

Cada versión incluye a la anterior de forma acumulativa. Ninguna versión se cierra si no cuenta con su **API REST** y su **Interfaz Gráfica en Flask** funcionando en paralelo y con sus pruebas de regresión al 100%.

| Versión | Alcance (Acumulativo) | Tablas / Recursos | Estado | Documentación |
| :--- | :--- | :--- | :---: | :---: |
| **v1** | CRUD completo de las 6 tablas sin clave foránea — API FastAPI e interfaz gráfica en Flask | `producto`, `empresa`, `persona`, `rol`, `ruta`, `usuario` | **Cerrada (100%)** | [Ver Docs v1](docs/v1_sin_fk/README.md) |
| **v2** | Las 12 tablas completas: claves foráneas, maestro-detalle por Stored Procedures, trigger contable y carga paralela (`ThreadPoolExecutor`) | `cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, `usuario_con_roles` | **Cerrada (100%)** | [Ver Docs v2](docs/v2_con_fk/README.md) |
| **v3** | Control de acceso: contraseñas con hash BCrypt, sesión con JWT, control RBAC vía `verificar_acceso_ruta` y protección con 401 y 403 | Procedimiento `verificar_acceso_ruta`, login y guards de navegación | **Cerrada (100%)** | [Ver Docs v3](docs/v3_seguridad_rbac/README.md) |

---

## 📚 Documentación Detallada y Guías de Sustentación

- 🎓 **[Guía Maestra de Retroalimentación y Sustentación (`docs/RETROALIMENTACION_Y_SUSTENTACION.md`)](docs/RETROALIMENTACION_Y_SUSTENTACION.md)**: Análisis técnico integral y preguntas clave de examen.
- 🧱 **[Paradigma P.O.O. en Python (`docs/PARADIGMA_POO.md`)](docs/PARADIGMA_POO.md)**: Abstracción (`ABC`), encapsulamiento, composición sobre herencia y polimorfismo en repositorios.
- 🔀 **[Flujo de una Petición (`docs/FLUJO_DE_UNA_PETICION.md`)](docs/FLUJO_DE_UNA_PETICION.md)**: Trazabilidad completa desde Flask hasta SQL Server.
- 🏛️ **[SOLID, Capas y Patrones (`docs/SOLID_CAPAS_PATRONES.md`)](docs/SOLID_CAPAS_PATRONES.md)**: Principios aplicados y separación estricta de procesos.
- 📖 **[Documento Maestro de Arquitectura (`docs/README.md`)](docs/README.md)**: Matriz comparativa y mapa relacional de las 12 tablas.
- 📁 **[Versión 1 (`docs/v1_sin_fk/`)](docs/v1_sin_fk/README.md)** · **[Versión 2 (`docs/v2_con_fk/`)](docs/v2_con_fk/README.md)** · **[Versión 3 (`docs/v3_seguridad_rbac/`)](docs/v3_seguridad_rbac/README.md)**

---

## 🏗️ Estructura del Repositorio (100% Python: FastAPI + Flask + SQL Server)

```text
├── api_python/            # Backend API REST en Python 3.12 (FastAPI + PyMSSQL + PyJWT + BCrypt)
│   ├── main.py            # Punto de entrada FastAPI, Swagger (/swagger) y manejadores 400..500
│   ├── ensamblador.py     # Único archivo que instancia repositorios concretos e inyecta servicios
│   ├── config.py          # Lectura de variables de entorno (BD y JWT)
│   ├── database.py        # Conexión PyMSSQL, helpers parametrizados y códigos de error SQL
│   ├── excepciones.py     # Excepciones tipadas de dominio desacopladas de HTTP
│   ├── modelos/           # Entidades de dominio POO (Producto, Cliente, Factura, Sesion, etc.)
│   ├── peticiones/        # Esquemas Pydantic segregados (Crear, Reemplazo, Actualizar) -> HTTP 422
│   ├── repositorios/      # Interfaces abstractas ABC (interfaces.py) + implementaciones SQL Server
│   ├── servicios/         # Lógica de dominio pura compuesta contra IRepositorio* (ABC)
│   ├── controladores/     # Controladores HTTP REST de v1, v2 y v3
│   ├── autorizacion/      # Dependencias JWT (401) y RBAC dinámico ('verificar_acceso_ruta' -> 403)
│   └── pruebas/           # Pruebas unitarias con repositorios polimórficos en memoria (sin BD)
├── front_python/          # Frontend Web en Python (Flask + Jinja2 + httpx + Bootstrap local)
│   ├── app.py             # Servidor Flask con rutas explícitas por recurso y ThreadPoolExecutor
│   ├── servicios/         # Transporte HTTP, sesiones en servidor (HttpOnly) y funciones por operación
│   ├── templates/         # Plantillas HTML Jinja2 (Home, Sesión, CRUDs, Facturación, Roles)
│   └── static/css/        # Bootstrap 5 local y hoja de estilos complementaria
├── db/                    # Scripts DDL, DML, Stored Procedures, Triggers y semilla BCrypt (SQL Server)
├── postman/               # Colección Postman para probar v1, v2 y v3
├── docs/                  # Documentación por versión (v1..v3), guías POO/SOLID/Flujo y spec_kit
├── .env.example           # Plantilla de variables de entorno sin secretos reales
└── docker-compose.yml     # Orquestación con un solo comando para BD, API FastAPI y Frontend Flask
```

---

## 🚀 Puesta en Marcha Rápida

Opcionalmente copia `.env.example` a `.env` para personalizar claves locales y ejecuta:

```bash
docker compose up -d --build
```

- **API REST (Python / FastAPI):** [http://localhost:8035](http://localhost:8035) (y puerto alterno `8032`)
  - Swagger OpenAPI: [http://localhost:8035/swagger](http://localhost:8035/swagger)
  - Diagnóstico: `curl.exe http://localhost:8035/`
- **Frontend Web (Python / Flask + Jinja2):** [http://localhost:8099](http://localhost:8099) (y puerto alterno `8096`)
- **SQL Server 2022:** `localhost,11463` (Base de datos: `bdfacturas_sqlserver_local`, Usuario: `sa`)
