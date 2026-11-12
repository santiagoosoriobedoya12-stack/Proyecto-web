# Proyecto — Aplicación y Servicios Web

Sistema integral de gestión comercial y facturación desarrollado **100% en Python** bajo la **metodología incremental acumulativa** de 3 versiones ($v1 \subset v2 \subset v3$), con API REST (**Python / FastAPI + PyMSSQL + PyJWT + BCrypt**), Frontend Web (**Python / FastAPI + Jinja2 + Bootstrap local**) y Base de Datos (**SQL Server 2022**) orquestados mediante **Docker Compose**.

---

## 🗺️ Mapa y Estado de Versiones

Cada versión incluye a la anterior de forma acumulativa. Ninguna versión se cierra si no cuenta con su **API REST** y su **Interfaz Gráfica** funcionando en paralelo y con sus pruebas de regresión al 100%.

| Versión | Alcance (Acumulativo) | Tablas / Recursos | Estado | Documentación |
| :--- | :--- | :--- | :---: | :---: |
| **v1** | CRUD completo de las 6 tablas sin clave foránea — API e interfaz gráfica en Python | `producto`, `empresa`, `persona`, `rol`, `ruta`, `usuario` | **Cerrada (100%)** | [Ver Docs v1](docs/v1_sin_fk/README.md) |
| **v2** | Las 12 tablas completas: claves foráneas, maestro-detalle por Stored Procedures, trigger contable y carga paralela (`asyncio.gather`) | `cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, `usuario_con_roles` | **Cerrada (100%)** | [Ver Docs v2](docs/v2_con_fk/README.md) |
| **v3** | Control de acceso: contraseñas con hash BCrypt, sesión con JWT, control RBAC vía `verificar_acceso_ruta` y protección con 401 y 403 | Procedimiento `verificar_acceso_ruta`, login y guards de navegación | **Cerrada (100%)** | [Ver Docs v3](docs/v3_seguridad_rbac/README.md) |

---

## 📚 Documentación Detallada y Diferencias por Versión

Para consultar qué contiene cada versión, cómo se diferencian y preparar la sustentación técnica, revisa:

- 🎓 **[Guía Maestra de Retroalimentación y Sustentación (`docs/RETROALIMENTACION_Y_SUSTENTACION.md`)](docs/RETROALIMENTACION_Y_SUSTENTACION.md)**: Análisis técnico integral y respuestas a preguntas clave de sustentación.
- 📖 **[Documento Maestro de Arquitectura y Diferencias (`docs/README.md`)](docs/README.md)**: Matriz comparativa lado a lado, mapa relacional de las 12 tablas y guía técnica.
- 📁 **[Versión 1 — Recursos Sin Clave Foránea (`docs/v1_sin_fk/`)](docs/v1_sin_fk/README.md)**: Fundamentos de arquitectura en 3 capas (Router-Servicio-Repositorio) con SQL parametrizado.
- 📁 **[Versión 2 — Tablas con Clave Foránea y Lógica en BD (`docs/v2_con_fk/`)](docs/v2_con_fk/README.md)**: Integridad referencial, código 409 Conflict, facturación maestro-detalle con Stored Procedures y disparadores.
- 📁 **[Versión 3 — Seguridad y Control de Acceso RBAC (`docs/v3_seguridad_rbac/`)](docs/v3_seguridad_rbac/README.md)**: Autenticación, JWT / Sesiones en servidor, dependencias de autorización y códigos 401/403.

---

## 🏗️ Estructura del Repositorio (100% Python + SQL Server)

```text
├── api_python/            # Backend API REST en Python 3.12 (FastAPI + PyMSSQL + PyJWT + BCrypt)
│   ├── main.py            # Ensamblador FastAPI, Swagger (/swagger), manejadores 400/401/403/404/409/422
│   ├── config.py          # Configuración de conexión SQL Server y llaves JWT desde variables de entorno
│   ├── database.py        # Gestor de conexiones PyMSSQL, extracción de errores SQL y normalización JSON
│   ├── excepciones.py     # Excepciones tipadas de dominio desacopladas de HTTP
│   ├── autorizacion/      # Dependencias JWT y validación RBAC dinámica ('verificar_acceso_ruta')
│   ├── peticiones/        # Esquemas Pydantic segregados (Crear, Reemplazo, Actualizar) -> HTTP 422
│   ├── servicios/         # Capa de dominio pura (reglas de negocio, BCrypt costo 12, OPENJSON, JWT)
│   ├── repositorios/      # Acceso a datos SQL Server explícito y parametrizado + SPs
│   ├── routers/           # Controladores HTTP REST de v1, v2 y v3
│   └── pruebas/           # Pruebas unitarias de capas con unittest
├── front_python/          # Frontend Web en Python (FastAPI + Jinja2 + httpx + Bootstrap local)
│   ├── main.py            # Rutas web, carga paralela con asyncio.gather y control de formularios
│   ├── servicios/         # Cliente HTTP asíncrono y sesiones JWT en memoria del servidor
│   ├── templates/         # Plantillas HTML Jinja2 (Home, Login, CRUDs, Facturación, Roles)
│   └── static/css/        # Bootstrap 5 local y hoja de estilos del sistema
├── db/                    # Scripts DDL, DML, Stored Procedures, Triggers y semilla BCrypt (SQL Server)
├── docs/                  # Paquetes de documentación organizados por versión (v1, v2 y v3) y spec_kit
└── docker-compose.yml     # Orquestación con un solo comando para BD, API Python y Frontend Python
```

---

## 🚀 Puesta en Marcha Rápida

Para iniciar todos los servicios del proyecto simultáneamente:

```bash
docker compose up -d --build
```

- **API REST (Python / FastAPI):** [http://localhost:8035](http://localhost:8035) (y puerto alterno `8032`)
  - Swagger OpenAPI: [http://localhost:8035/swagger](http://localhost:8035/swagger)
  - Diagnóstico: `curl.exe http://localhost:8035/`
- **Frontend Web (Python / Jinja2):** [http://localhost:8099](http://localhost:8099) (y puerto alterno `8096`)
- **SQL Server 2022:** `localhost,11463` (Base de datos: `bdfacturas_sqlserver_local`, Usuario: `sa`)
