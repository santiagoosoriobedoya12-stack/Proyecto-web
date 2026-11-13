# Versión 1 — Recursos Independientes (Sin Clave Foránea)

> **Módulo:** Fundamentos y Arquitectura en Capas en Python  
> **Alcance:** Las 8 tablas que no dependen de ninguna otra entidad en la base de datos `mapa_conocimiento`.  
> **Patrón:** Vertical Slice Architecture (Controlador FastAPI → Servicio → Repositorio MariaDB con PyMySQL).

---

## 1. Resumen Ejecutivo de la Versión 1

En la **Versión 1**, el objetivo fue construir los cimientos arquitectónicos del sistema en **Python puro** para las 8 entidades independientes del módulo **Mapa de Conocimiento**:

1. **`area_conocimiento`**: Catálogo jerárquico (gran área, área, disciplina).
2. **`objetivo_desarrollo_sostenible`**: Los 17 ODS oficiales con su categoría social, ambiental o económica.
3. **`area_aplicacion`**: Sectores económicos receptores de investigación.
4. **`termino_clave`**: Palabras clave y términos técnicos con traducción opcional al inglés.
5. **`linea_investigacion`**: Líneas de investigación institucionales (id autoincremental).
6. **`aliado`**: Empresas y entidades externas asociadas (NIT, razón social, contacto, correo, ciudad).
7. **`tipo_producto`**: Tipología de productos según Minciencias.
8. **`proyecto`**: Proyectos de investigación (título, resumen, presupuesto, financiación, fechas).

Todas las entidades incorporan **borrado lógico obligatorio**: columna `activo TINYINT(1) NOT NULL DEFAULT 1` ([docs/spec_kit/1_constitution.md](../spec_kit/1_constitution.md)).

---

## 2. Índice Completo del Spec Kit — Versión 1

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación formal: alcance, requisitos funcionales (RF1..RF7), no funcionales y criterios de aceptación. |
| [3_plan.md](3_plan.md) | Plan técnico de arquitectura en capas, estructura de carpetas y responsabilidades. |
| [4_research.md](4_research.md) | Investigación técnica: PyMySQL con DictCursor, `CLIENT.FOUND_ROWS` y Pydantic v2. |
| [5_data_model.md](5_data_model.md) | Modelo de datos de las 8 tablas en MariaDB y justificación de la columna `activo`. |
| [6_contracts.md](6_contracts.md) | Especificación de contratos REST OpenAPI de los endpoints v1, verbos y códigos HTTP. |
| [7_quickstart.md](7_quickstart.md) | Guía de arranque rápido con Docker Compose y pruebas con `pytest`. |
| [8_tasks.md](8_tasks.md) | Checklist de tareas de construcción completadas al 100%. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA alineados con cada documento del Spec Kit y corte vertical. |

---

## 3. Archivos del Código Fuente
- Repositorios: [`api_mapa/repositorios/repositorio_v1.py`](../../api_mapa/repositorios/repositorio_v1.py)
- Servicios: [`api_mapa/servicios/servicio_v1.py`](../../api_mapa/servicios/servicio_v1.py)
- Controladores: [`api_mapa/controladores/controlador_v1.py`](../../api_mapa/controladores/controlador_v1.py)
- Frontend Flask: [`front_flask/app.py`](../../front_flask/app.py) + [`front_flask/templates/recurso_crud.html`](../../front_flask/templates/recurso_crud.html)
