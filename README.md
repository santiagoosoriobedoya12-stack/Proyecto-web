# Proyecto — Aplicación y Servicios Web

Sistema integral de gestión y facturación desarrollado según la metodología incremental acumulativa de 4 versiones (+ v5).

## 🗺️ Mapa de Versiones

Cada versión incluye a la anterior de forma acumulativa ($v1 \subset v2 \subset v3 \subset v4$). Ninguna versión se cierra si no cuenta con su **API REST** y su **Interfaz Gráfica** funcionando en paralelo.

| Versión | Alcance (Acumulativo) | Tablas / Recursos | Estado |
| :--- | :--- | :--- | :--- |
| **v1** | CRUD completo de las 6 tablas sin clave foránea — API e interfaz gráfica | `producto`, `empresa`, `persona`, `rol`, `ruta`, `usuario` | En curso |
| **v2** | CRUD de todas las tablas con clave foránea, puente y facturación maestro-detalle | `cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, `usuario_con_roles` | Pendiente |
| **v3** | Control de acceso: contraseñas con hash, sesión con tokens y permisos de ruta | Recursos `sesion` y `permisos` (`verificar_acceso_ruta`) | Pendiente |
| **v4** | 10 consultas multitabla (4+ tablas), tablero con métricas/gráficos, marca corporativa, PWA/responsive y despliegue | `/api/consultas` cruzando las 12 tablas | Pendiente |
| **v5** | Patrón repositorio con múltiples motores de base de datos (PostgreSQL, MariaDB, etc.) | Arquitectura desacoplada | Futura |

---

## 📌 Reglas del Proyecto
1. **Back y Front en Paralelo**: Cada recurso debe contar con sus endpoints y sus vistas asociadas.
2. **Cierre formal**: Cada versión se cierra con commit y tag de versión (ej. `v1.0.0`).
3. **Regresión**: Al avanzar a una nueva versión, los criterios de aceptación de las versiones anteriores siguen corriendo y deben continuar pasando.

---

## 📂 Estructura del Proyecto

```text
├── database/          # Scripts DDL, DML y procedimientos almacenados
├── backend/           # API REST (Modelos, Repositorios, Servicios, Controladores)
├── frontend/          # Interfaces Gráficas de usuario
├── docs/              # Documentación, especificaciones y actas de entrega
└── README.md
```
