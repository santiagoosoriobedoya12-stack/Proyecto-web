# Versión 1 — Recursos Independientes (Sin Clave Foránea)

> **Módulo:** Fundamentos y Arquitectura en Capas  
> **Alcance:** Las 6 tablas que no dependen de ninguna otra entidad en la base de datos `bdfacturas`.  
> **Patrón:** Vertical Slice Architecture en Python (Router → Servicio → Repositorio → SQL Server).

---

## 1. Resumen Ejecutivo de la Versión 1

En la **Versión 1**, el objetivo fue construir los cimientos arquitectónicos del sistema sin la complejidad de las dependencias foráneas. Se implementó el ciclo de vida CRUD completo de los recursos independientes:

1. **`producto`**: Catálogo de artículos con código alfanumérico, nombre, stock y valor unitario.
2. **`persona`**: Registro base de personas naturales con código único, nombre, correo electrónico y teléfono.
3. **`empresa`**: Entidades jurídicas con código empresarial y razón social.
4. **`rol`**: Roles de seguridad del sistema (ej. Administrador, Vendedor, Cliente).
5. **`ruta`**: Endpoints y vistas protegibles del sistema.
6. **`usuario`**: Credenciales de acceso (`email` y `contrasena`).

---

## 2. Decisiones Clave de Arquitectura en Python

- **Separación Estricta de Responsabilidades (`api_python/`):**
  - **Router (`routers/router_v1.py`):** Solo conoce HTTP (verbos FastAPI, códigos de estado `200`, `201`, `204`, `400`, `404`, `409`, `422`, `500`), serialización JSON y rutas.
  - **Servicio de Dominio (`servicios/servicio_v1.py`):** Contiene las reglas del negocio puro (validaciones de clave, hash de contraseña). **No conoce HTTP**. Si ocurre un fallo de negocio, lanza excepciones de dominio (`NoEncontradoExcepcion`, `ValidacionNegocioExcepcion`, `ConflictoExcepcion`).
  - **Repositorio (`repositorios/repositorio_v1.py`):** Encapsula el acceso a datos mediante **PyMSSQL** y consultas SQL parametrizadas explícitas (`%s`). **No conoce reglas ni HTTP**.
- **Tres Esquemas Pydantic por Recurso (`peticiones/esquemas.py`):**
  - `*Crear`: Campos obligatorios para creación (`POST` → `201 Created`).
  - `*Reemplazo`: Reemplazo total idempotente (`PUT` → `200` / `422` si falta algún campo).
  - `*Actualizar`: Todos los campos opcionales para actualización parcial (`PATCH` → `200` con solo los campos enviados).
- **Sobre Estándar de Respuesta:**
  ```json
  {
    "tabla": "producto",
    "total": 8,
    "datos": [ ... ]
  }
  ```
- **Frontend Python Independiente (`front_python/`):**
  - Vistas Jinja2 servidas por FastAPI (`templates/crud_generico.html`) sin `pymssql`.
  - Consumo HTTP asíncrono centralizado en `front_python/servicios/cliente_api.py` mediante `httpx`.
  - Resiliencia ante caídas de la API (alerta visual y conteo en 0 sin colapsar la interfaz).

---

## 3. Índice de Documentos de la Versión 1

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación de requerimientos, alcance y criterios de aceptación formales de la v1. |
| [3_plan.md](3_plan.md) | Plan técnico: estructura de carpetas, capas Python, esquemas Pydantic y dependencias. |
| [4_research.md](4_research.md) | Análisis de alternativas: PyMSSQL vs ORM pesado, excepciones de dominio vs HTTP, validación por verbo. |
| [5_data_model.md](5_data_model.md) | Estructura de las 6 tablas independientes en SQL Server. |
| [6_contracts.md](6_contracts.md) | Especificación OpenAPI/REST de todos los endpoints de la v1 con ejemplos JSON exactos. |
| [7_quickstart.md](7_quickstart.md) | Guía de arranque rápido con Docker Compose y pruebas con `curl`. |
| [8_tasks.md](8_tasks.md) | Plan de tareas por fases verificables de construcción de la v1. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA utilizados para diseñar y construir las 6 tablas en Python. |
