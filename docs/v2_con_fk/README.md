# Versión 2 — Tablas con Clave Foránea (FK) y Lógica en Base de Datos

> **Módulo:** Integridad Referencial, Transacciones y Procedimientos Almacenados  
> **Alcance:** Las 6 tablas dependientes de `bdfacturas`, cerrando el modelo total de **12 tablas**.  
> **Enfoque Acumulativo:** $v1 \subset v2$ (La v2 incluye intacta la v1 y agrega sus propias reglas en Python).

---

## 1. Resumen Ejecutivo de la Versión 2

La **Versión 2** completa el modelo relacional de la empresa introduciendo tres conceptos esenciales que no existían en la v1:

1. **Dependencia entre filas e Integridad Referencial:**
   - Un cliente o un vendedor no pueden existir sin su persona asociada.
   - SQL Server defiende las claves foráneas; la API Python traduce el error del motor (código `547` o `2627`/`2601` en `database.py`) a **`HTTP 409 Conflict`**.
2. **Lógica pesada delegada a la Base de Datos:**
   - La entidad `factura` opera mediante una arquitectura **Maestro-Detalle**.
   - Los cálculos de subtotal, total acumulado y descuento de inventario los ejecuta el disparador `trg_actualizar_totales_y_stock` y los procedimientos almacenados transaccionales.
   - Python **nunca** calcula ni envía precios unitarios, subtotales ni totales al crear una factura: la base es la única fuente de verdad.
3. **Selección Dinámica en Frontend Python (`front_python/`):**
   - En lugar de campos de texto planos para escribir códigos foráneos a ciegas, las plantillas Jinja2 ofrecen menús desplegables (`<select>`) cargados desde la API.

---

## 2. Los Nuevos Recursos de la Versión 2

| Recurso | Tipo | Características Principales |
|---|---|---|
| `/api/cliente` | 1:N con FK | Clave autoincremental (`IDENTITY`). Requiere `fkcodpersona` obligatoria y `fkcodempresa` opcional (`None`/nullable para persona natural). |
| `/api/vendedor` | 1:N con FK | Clave autoincremental (`IDENTITY`). Requiere `fkcodpersona` obligatoria, carnet y dirección. |
| `/api/factura` | Maestro-Detalle | 100% operado por Stored Procedures. No realiza borrado físico: `DELETE /api/factura/{num}` ejecuta anulación lógica (`estado = 'anulada'`) devolviendo stock. |
| `/api/rol-usuario` | Tabla Puente M:N | Clave compuesta (`fkemail`, `fkidrol`). Soporta listados por ambos extremos y eliminación por clave compuesta. Sin PUT ni PATCH. |
| `/api/rutarol` | Tabla Puente M:N | Clave compuesta (`fkidruta`, `fkidrol`). Asigna permisos de rutas a roles. Sin PUT ni PATCH. |
| `/api/usuario-con-roles` | Recurso Compuesto | Administra un usuario y su colección de roles atómicamente con 5 Stored Procedures (`OPENJSON`). |

---

## 3. Puntos Clave para la Sustentación Académica

1. **¿Por qué la API devuelve `409` en vez de `422` o `500` al violar una FK?**
   - El código **422 Unprocessable Entity** se reserva para errores en la **forma o tipo** del dato enviado en la petición Pydantic (ej. texto donde iba un entero, o campo obligatorio ausente).
   - El código **409 Conflict** indica que la petición estaba bien formada sintácticamente, pero choca con el **estado actual de la base de datos** (la FK referenciada no existe, o se intentó borrar un registro padre con hijos asociados).
2. **¿Por qué el repositorio de Factura no tiene `INSERT INTO factura`?**
   - Para garantizar la atomicidad contable. `sp_insertar_factura_y_productosporfactura` recibe encabezado y detalle en JSON, abre transacción, calcula subtotales y activa el disparador que descuenta inventario.
3. **Optimización de carga paralela en `front_python/` (`asyncio.gather`):**
   - En pantallas con múltiples catálogos foráneos (como `/clientes` que requiere personas y empresas, o `/emitir-factura` que requiere clientes, vendedores y productos), las llamadas secuenciales sumarían latencias.
   - En `front_python/main.py`, `asyncio.gather(...)` dispara las peticiones HTTP vía `httpx` concurrentemente en paralelo.

---

## 4. Índice de Documentos de la Versión 2

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación de requerimientos funcionales (RF1 a RF6), alcance y 18 criterios de aceptación de la v2. |
| [3_plan.md](3_plan.md) | Diseño técnico detallado en Python: esquemas Pydantic, `ConflictoExcepcion`, repositorios y SPs. |
| [4_research.md](4_research.md) | Decisiones de diseño y alternativas: SPs vs SQL manual, triggers para totales, formato JSON para `OPENJSON`. |
| [5_data_model.md](5_data_model.md) | Modelo relacional de las 6 tablas con FK, diagrama de dependencias, triggers y código de los SPs. |
| [6_contracts.md](6_contracts.md) | Contratos OpenAPI / REST con cuerpos JSON de petición y respuesta para todos los nuevos endpoints. |
| [7_quickstart.md](7_quickstart.md) | Guía de arranque rápido, comandos `curl` de prueba y protocolo de regresión de la v1. |
| [8_tasks.md](8_tasks.md) | Desglose por fases de construcción (Fase 0 a Fase 6) con verificación de cada componente. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA utilizados para diseñar y construir las 6 tablas dependientes en Python. |
