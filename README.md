# Proyecto — Aplicación y Servicios Web

Sistema integral de gestión comercial y facturación desarrollado bajo la **metodología incremental acumulativa** de 4 versiones ($v1 \subset v2 \subset v3 \subset v4$), con API REST (.NET 10 + Dapper), Frontend (Blazor Server .NET 10 + Bootstrap) y Base de Datos (SQL Server 2022) orquestados mediante Docker Compose.

---

## 🗺️ Mapa y Estado de Versiones

Cada versión incluye a la anterior de forma acumulativa. Ninguna versión se cierra si no cuenta con su **API REST** y su **Interfaz Gráfica** funcionando en paralelo y con sus pruebas de regresión al 100%.

| Versión | Alcance (Acumulativo) | Tablas / Recursos | Estado | Documentación |
| :--- | :--- | :--- | :---: | :---: |
| **v1** | CRUD completo de las 6 tablas sin clave foránea — API e interfaz gráfica | `producto`, `empresa`, `persona`, `rol`, `ruta`, `usuario` | **Cerrada (100%)** | [Ver Docs v1](docs/v1_sin_fk/README.md) |
| **v2** | Las 12 tablas completas: claves foráneas, maestro-detalle por Stored Procedures, trigger contable y carga paralela en Blazor | `cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, `usuario_con_roles` | **Cerrada (100%)** | [Ver Docs v2](docs/v2_con_fk/README.md) |
| **v3** | Control de acceso: contraseñas con hash, sesión con tokens, control RBAC y protección de rutas con 401 y 403 | Procedimiento `verificar_acceso_ruta`, login y guards de navegación | **Cerrada (100%)** | [Ver Docs v3](docs/v3_seguridad_rbac/README.md) |
| **v4** | 10 consultas multitabla complejas (4+ tablas), tablero de métricas (Dashboard) con gráficos y marca corporativa | `/api/consultas` y `/api/dashboard` cruzando las 12 tablas | **Cerrada (100%)** | [Ver Docs v4](docs/v4_reportes_dashboard/README.md) |
| **v5** | Patrón repositorio con múltiples motores de base de datos (PostgreSQL, MariaDB, etc.) | Arquitectura desacoplada multi-motor | Futura | — |

---

## 📚 Documentación Detallada y Diferencias por Versión

Para consultar qué contiene cada versión, cómo se diferencian y preparar la sustentación técnica, revisa:

- 🎓 **[Guía Maestra de Retroalimentación y Sustentación (`docs/RETROALIMENTACION_Y_SUSTENTACION.md`)](docs/RETROALIMENTACION_Y_SUSTENTACION.md)**: Análisis técnico integral y respuestas a las 10 preguntas clave de examen.
- 📖 **[Documento Maestro de Arquitectura y Diferencias (`docs/README.md`)](docs/README.md)**: Matriz comparativa lado a lado, mapa relacional de las 12 tablas y preguntas frecuentes de examen.
- 📁 **[Versión 1 — Recursos Sin Clave Foránea (`docs/v1_sin_fk/`)](docs/v1_sin_fk/README.md)**: Fundamentos de arquitectura en 3 capas (Controlador-Servicio-Repositorio) con Dapper.
- 📁 **[Versión 2 — Tablas con Clave Foránea y Lógica en BD (`docs/v2_con_fk/`)](docs/v2_con_fk/README.md)**: Integridad referencial, código 409 Conflict, facturación maestro-detalle con Stored Procedures y disparadores.
- 📁 **[Versión 3 — Seguridad y Control de Acceso RBAC (`docs/v3_seguridad_rbac/`)](docs/v3_seguridad_rbac/README.md)**: Autenticación, JWT / Sesiones, middlewares de autorización y códigos 401/403.
- 📁 **[Versión 4 — Reportes, Consultas Multitabla y Dashboard (`docs/v4_reportes_dashboard/`)](docs/v4_reportes_dashboard/README.md)**: 10 consultas complejas cruzando 4+ tablas, KPIs en vivo, tablero Blazor y manual de marca.

---

## 🏗️ Estructura del Repositorio

```text
├── api_facturas/       # Backend API REST en ASP.NET Core .NET 10 + Dapper
│   ├── Controllers/    # Controladores HTTP (traducción de códigos de estado)
│   ├── Servicios/      # Capa de dominio puro (excepciones de negocio)
│   ├── Repositorios/   # Acceso a datos con SQL Server mediante Dapper y SPs
│   ├── Modelos/        # Entidades del dominio
│   └── Peticiones/     # DTOs segregados (Crear, Reemplazo, Actualizar)
├── front_blazor/       # Frontend interactivo en Blazor Server .NET 10
│   ├── Components/     # Vistas Razor interactivas y componentes UI
│   ├── Servicios/      # Clientes HTTP tipados hacia la API
│   └── Modelos/        # Modelos independientes de presentación
├── db/                 # Scripts DDL, DML, Stored Procedures y Triggers de SQL Server
├── docs/               # Paquetes de documentación organizados por versión (v1, v2, v3)
└── docker-compose.yml  # Orquestación con un solo comando para todo el sistema
```

---

## 🚀 Puesta en Marcha Rápida

Para iniciar todos los servicios del proyecto simultáneamente:

```bash
docker compose up -d
```

- **API REST (.NET 10):** [http://localhost:8035](http://localhost:8035) (y puerto alterno `8032`)
  - Diagnóstico: `curl http://localhost:8035/`
- **Frontend Blazor Server:** [http://localhost:8099](http://localhost:8099) (y puerto alterno `8096`)
- **SQL Server 2022:** `localhost:1433` (Base de datos: `bdfacturas`, Usuario: `sa`)
