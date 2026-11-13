# Versión 2 — Recursos Con Clave Foránea y Puentes M:N

> **Módulo:** Integridad Referencial y Relaciones Complejas en Python  
> **Alcance:** Las 10 tablas que dependen de claves foráneas o modelan relaciones muchos a muchos en `mapa_conocimiento`.  
> **Patrón:** Validación de padres activos en la capa de servicios, manejo de 409 Conflict y carga paralela con `ThreadPoolExecutor`.

---

## 1. Resumen Ejecutivo de la Versión 2

En la **Versión 2**, el sistema crece acumulativamente ($v1 \subset v2$) incorporando:

1. **`docente`**: Investigadores con clave foránea opcional a su línea de investigación principal.
2. **`producto`**: Productos de investigación vinculados obligatoriamente a un tipo de producto y opcionalmente a un proyecto.
3. **`desarrolla`**: Participación de docentes en proyectos con atributos de rol y descripción.
4. **7 Tablas Puente M:N**:
   - `aliado_proyecto`: Alianzas por proyecto.
   - `palabras_clave`: Palabras clave indexadas por proyecto.
   - `ac_proyecto`: Áreas de conocimiento por proyecto.
   - `proyecto_linea`: Líneas de investigación adscritas al proyecto.
   - `ods_proyecto`: ODS impactados por el proyecto.
   - `aa_proyecto`: Sectores económicos de aplicación por proyecto.
   - `docente_producto`: Autorías docentes por producto.

---

## 2. Índice Completo del Spec Kit — Versión 2

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación formal: alcance, requisitos funcionales de FKs, RF de vínculos M:N y criterios de aceptación. |
| [3_plan.md](3_plan.md) | Plan técnico: repositorio puente parametrizado, validaciones en servicios y concurrencia. |
| [4_research.md](4_research.md) | Investigación técnica: `ThreadPoolExecutor` en Flask y preservación de tipos y nombres del DDL. |
| [5_data_model.md](5_data_model.md) | Modelo relacional de las 10 tablas de la v2, PKs compuestas y borrado lógico. |
| [6_contracts.md](6_contracts.md) | Especificación de contratos REST OpenAPI de los endpoints de la v2 y matriz de 409 Conflict. |
| [7_quickstart.md](7_quickstart.md) | Guía de prueba rápida en interfaz web y Swagger para validar FKs y puentes. |
| [8_tasks.md](8_tasks.md) | Checklist de tareas de construcción de v2 completadas al 100%. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA alineados con cada documento del Spec Kit y corte vertical de v2. |

---

## 3. Archivos del Código Fuente
- Repositorios: [`api_mapa/repositorios/repositorio_v2.py`](../../api_mapa/repositorios/repositorio_v2.py)
- Servicios: [`api_mapa/servicios/servicio_v2.py`](../../api_mapa/servicios/servicio_v2.py)
- Controladores: [`api_mapa/controladores/controlador_v2.py`](../../api_mapa/controladores/controlador_v2.py)
- Frontend Flask: [`front_flask/app.py`](../../front_flask/app.py) + [`templates/desarrolla.html`](../../front_flask/templates/desarrolla.html) + [`templates/puente.html`](../../front_flask/templates/puente.html)
