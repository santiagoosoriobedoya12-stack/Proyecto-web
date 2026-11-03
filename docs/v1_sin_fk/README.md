# Versión 1 — Recursos Independientes (Sin Clave Foránea)

> **Módulo:** Fundamentos y Arquitectura en Capas  
> **Alcance:** Las 6 tablas que no dependen de ninguna otra entidad en la base de datos `bdfacturas`.  
> **Patrón:** Vertical Slice Architecture (Controlador → Servicio → Repositorio → SQL Server).

---

## 1. Resumen Ejecutivo de la Versión 1

En la **Versión 1**, el objetivo fue construir los cimientos arquitectónicos del sistema sin la complejidad de las dependencias foráneas. Se implementó el ciclo de vida CRUD completo de los recursos independientes:

1. **`producto`**: Catálogo de artículos con código alfanumérico, nombre, stock y valor unitario.
2. **`persona`**: Registro base de personas naturales con documento de identidad y datos de contacto.
3. **`empresa`**: Entidades jurídicas con código empresarial y razón social.
4. **`rol`**: Roles de seguridad del sistema (ej. Administrador, Vendedor, Auditor).
5. **`ruta`**: Endpoints y vistas protegibles del sistema.
6. **`usuario`**: Credenciales de acceso (email y contraseña).

---

## 2. Decisiones Clave de Arquitectura

- **Separación Estricta de Responsabilidades:**
  - **Controlador (`Controllers/`):** Solo conoce HTTP (códigos de estado 200, 201, 204, 400, 404, 422, 500), serialización JSON y rutas.
  - **Servicio de Dominio (`Servicios/`):** Contiene las reglas del negocio puro (validaciones, cálculos). **No conoce HTTP** ni `IActionResult`. Si ocurre un fallo de negocio, lanza excepciones del dominio (`NoEncontradoExcepcion`, `ReglaNegocioExcepcion`).
  - **Repositorio (`Repositorios/`):** Encapsula el acceso a datos mediante **Dapper** y consultas SQL explícitas. **No conoce reglas ni HTTP**.
- **Tres Clases de Petición por Recurso:**
  - `*Crear.cs`: Todos los campos obligatorios para creación (`POST` → 201 Created).
  - `*Reemplazo.cs`: Reemplazo total idempotente (`PUT` → 200 / 422 si falta algún campo).
  - `*Actualizar.cs`: Actualización parcial (`PATCH` → 200 con solo los campos enviados).
- **Sobre Estándar de Respuesta:**
  ```json
  {
    "tabla": "producto",
    "total": 8,
    "datos": [ ... ]
  }
  ```
- **Front Blazor Server:**
  - Vistas interactivas independientes (`Productos.razor`, `Personas.razor`, `Empresas.razor`, etc.).
  - Consumo tipado mediante `HttpClient` inyectado en `Program.cs`.
  - Resiliencia ante caídas de la API (alerta visual y conteo en 0 sin colapsar la interfaz).

---

## 3. Índice de Documentos de la Versión 1

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación de requerimientos, alcance y criterios de aceptación formales de la v1. |
| [3_plan.md](3_plan.md) | Plan técnico: estructura de carpetas, capas, contratos de DTOs y dependencias. |
| [4_research.md](4_research.md) | Análisis de alternativas: Dapper vs EF Core, excepciones de dominio vs códigos HTTP, diseño de peticiones. |
| [5_data_model.md](5_data_model.md) | Estructura de las 6 tablas independientes en SQL Server. |
| [6_contracts.md](6_contracts.md) | Especificación OpenAPI/REST de todos los endpoints de la v1 con ejemplos JSON exactos. |
| [7_quickstart.md](7_quickstart.md) | Guía de arranque rápido con Docker Compose y pruebas con `curl`. |
| [8_tasks.md](8_tasks.md) | Plan de tareas por fases verificables de construcción de la v1. |

