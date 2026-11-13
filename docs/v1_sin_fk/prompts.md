# Bitácora de Prompts — Versión 1: Las 8 Tablas Sin Clave Foránea

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar la arquitectura en 3 capas, los DTOs segregados en Pydantic, los repositorios PyMySQL y las interfaces web en Flask + Jinja2 para las 8 tablas independientes del módulo **Mapa de Conocimiento** en **Python puro**.

---

## 1. Prompt Arquitectónico Base (Vertical Slice en Python)

```text
Actúa como un arquitecto de software senior en Python 3.12, FastAPI, Flask y MariaDB.
Diseña una arquitectura en capas estricta para el módulo Mapa de Conocimiento (mapa_conocimiento):
1. Capa de Controladores (FastAPI):
   - Expone rutas REST explícitas bajo el prefijo /api/....
   - Maneja códigos de estado HTTP (200, 201, 204, 400, 404, 409, 422, 500).
   - Retorna el sobre estándar { "tabla": ..., "limite": ..., "total": ..., "datos": [...] } o 204 No Content si está vacío.
   - Aplica inyección de dependencias para seguridad.
2. Capa de Servicios (Lógica de Dominio Pura):
   - Encapsula las reglas de negocio en clases POO.
   - Recibe las interfaces abstractas (IRepositorio*) por constructor (Inversión de dependencias).
   - Lanza excepciones de dominio (NoEncontradoExcepcion, ValidacionNegocioExcepcion) sin conocer FastAPI ni HTTP.
3. Capa de Repositorios (PyMySQL sobre MariaDB 11.4):
   - Define interfaces abstractas mediante abc.ABC y @abstractmethod en interfaces.py.
   - Implementa repositorios MariaDB con consultas SQL parametrizadas (%s) previniendo inyección SQL.
   - Aplica la regla obligatoria de borrado lógico (0_METODOLOGIA.md §7.0): todas las lecturas filtran `WHERE activo = 1`.
   - El DELETE ejecuta `UPDATE ... SET activo = 0 WHERE id = %s AND activo = 1`.
   - Conexión configurada con DictCursor y client_flag=CLIENT.FOUND_ROWS para evitar falsos 404 en PUT idempotente.
4. Esquemas Pydantic Segregados por Verbo HTTP (frontera 422):
   - *Crear (POST): todos los campos obligatorios según el DDL.
   - *Reemplazo (PUT): todos los campos obligatorios para reemplazo idempotente (422 si falta alguno).
   - *Actualizar (PATCH): todos los campos Optional (400 si el cuerpo llega vacío).
5. Frontend Web en Flask + Jinja2:
   - Servidor Flask desacoplado en el puerto 8116 (sin pymysql ni credenciales de BD).
   - Consume la API REST mediante httpx con manejo de errores de conexión (.aviso-desconexion).
   - Interfaz con Bootstrap 5 local (sin CDNs externos) y botones no técnicos ("Guardar la ficha completa", "Guardar solo lo que cambié", "Retirar").
```

---

## 2. Prompts de Generación del Spec Kit (Documentos 2 a 8)

### Prompt para `2_spec.md` (Especificación Formal)
```text
Genera la especificación formal 2_spec.md para la Versión 1 de Mapa de Conocimiento:
- Define los requisitos funcionales RF1 (Listar), RF2 (Obtener por clave), RF3 (Crear con 201), RF4 (Reemplazar con 200), RF5 (Actualizar con 200/400), RF6 (Eliminar lógico con 204/404) y RF7 (Frontend Flask).
- Establece la regla inmutable de borrado lógico activo = 1/0.
- Detalla los requisitos no funcionales RNF1 (Capas estrictas), RNF2 (SQL parametrizado %s), RNF3 (Aislamiento de Flask), RNF4 (DTOs segregados).
```

### Prompt para `3_plan.md` y `4_research.md` (Plan Técnico e Investigación)
```text
Genera 3_plan.md y 4_research.md justificando las decisiones de arquitectura:
- Justifica la elección de PyMySQL directo con DictCursor en lugar de ORMs pesados.
- Explica la necesidad de activar client_flag=CLIENT.FOUND_ROWS para evitar falsos 404 en PUT idempotente.
- Detalla la estrategia de DTOs segregados con Pydantic v2 (Crear, Reemplazo, Actualizar).
```

### Prompt para `5_data_model.md` y `6_contracts.md` (Modelo y Contratos REST)
```text
Genera 5_data_model.md y 6_contracts.md para las 8 tablas de la v1:
- Detalla nombres de columna, tipos de datos, claves primarias y la columna activo.
- Define el contrato HTTP exacto: prefijo /api/..., sobre { tabla, limite, total, datos }, códigos HTTP estándar (200, 201, 204, 400, 404, 409, 422, 500) y ejemplos de payload.
```

### Prompt para `7_quickstart.md` y `8_tasks.md` (Puesta en Marcha y Tareas)
```text
Genera 7_quickstart.md y 8_tasks.md:
- Guía de ejecución con Docker Compose y pruebas automáticas con pytest pruebas/test_capas.py.
- Checklist de tareas desglosadas de T1.1 a T1.11 marcadas como completadas.
```

---

## 3. Prompts Específicos por Tabla de Versión 1

### Tabla 1: `area_conocimiento`
```text
Genera el corte vertical completo en Python para 'area_conocimiento':
1. Modelo de dominio AreaConocimiento (@dataclass: id, gran_area, area, disciplina).
2. DTOs Pydantic: AreaConocimientoCrear, AreaConocimientoReemplazo, AreaConocimientoActualizar.
3. Interfaz IRepositorioAreaConocimiento e implementación RepositorioAreaConocimientoMariaDB con filtro activo = 1.
4. ServicioAreaConocimiento con validación de límite y existencia.
5. Router FastAPI en /api/area-conocimiento.
6. Vista Flask y configuración en catalogo_vistas.py para renderizado en recurso_crud.html.
```

### Tabla 2: `objetivo_desarrollo_sostenible` (ODS)
```text
Genera el corte vertical completo para 'objetivo_desarrollo_sostenible':
1. Modelo ObjetivoDesarrolloSostenible (@dataclass: id, nombre, categoria).
2. DTOs segregados: OdsCrear, OdsReemplazo, OdsActualizar.
3. Repositorio con PyMySQL y 17 registros ODS oficiales precargados en init.sql.
4. ServicioOds y router en /api/objetivo-desarrollo-sostenible.
5. Vista en Flask con catálogo de los 17 ODS.
```

### Tabla 3: `area_aplicacion`
```text
Genera el corte vertical completo para 'area_aplicacion':
1. Modelo AreaAplicacion (@dataclass: id, nombre).
2. Peticiones segregadas para POST, PUT y PATCH.
3. Repositorio MariaDB con borrado lógico activo = 1/0.
4. Servicio y router /api/area-aplicacion.
5. Vista Flask en recurso_crud.html.
```

### Tabla 4: `termino_clave`
```text
Genera el corte vertical para 'termino_clave':
1. Modelo TerminoClave (@dataclass: termino VARCHAR(30) PK, termino_ingles).
2. DTOs: TerminoClaveCrear, TerminoClaveReemplazo, TerminoClaveActualizar.
3. Repositorio MariaDB con clave primaria alfanumérica en consultas SQL.
4. Servicio y router /api/termino-clave.
5. Vista Flask con soporte de edición y retiro por clave alfanumérica.
```

### Tabla 5: `linea_investigacion`
```text
Genera el corte vertical para 'linea_investigacion':
1. Modelo LineaInvestigacion (@dataclass: id AUTO_INCREMENT, nombre, descripcion).
2. DTOs segregados: LineaInvestigacionCrear (sin ID), LineaInvestigacionReemplazo, LineaInvestigacionActualizar.
3. Repositorio MariaDB capturando LAST_INSERT_ID() al insertar.
4. ServicioLineaInvestigacion y router /api/linea-investigacion.
5. Vista Flask con soporte para clave primaria autogenerada (pk_auto = True).
```

### Tabla 6: `aliado`
```text
Genera el corte vertical para 'aliado':
1. Modelo Aliado (@dataclass: nit PK, razon_social, nombre_contacto, correo, telefono, ciudad).
2. DTOs con validación de correo electrónico mediante EmailStr de Pydantic.
3. Repositorio MariaDB con verificación de NIT duplicado (código 1062 -> ConflictoExcepcion 409).
4. ServicioAliado y router /api/aliado.
5. Vista Flask en /aliados con formulario y tabla de entidades externas.
```

### Tabla 7: `tipo_producto`
```text
Genera el corte vertical para 'tipo_producto':
1. Modelo TipoProducto (@dataclass: id, categoria, clase, nombre, tipologia).
2. DTOs segregados según la tipología oficial de Minciencias.
3. Repositorio MariaDB, ServicioTipoProducto y router /api/tipo-producto.
4. Vista Flask en /tipos-producto.
```

### Tabla 8: `proyecto`
```text
Genera el corte vertical completo para la entidad 'proyecto':
1. Modelo Proyecto (@dataclass: id PK, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin).
2. DTOs: ProyectoCrear, ProyectoReemplazo, ProyectoActualizar con validación de fechas (date) y presupuesto >= 0.
3. RepositorioProyectoMariaDB con filtro activo = 1 y normalización de tipos Decimal y date para serialización JSON.
4. ServicioProyecto con validaciones de negocio.
5. Router FastAPI en /api/proyecto.
6. Vista Flask en /proyectos con listado, formulario de creación y opciones de edición completa (PUT), parcial (PATCH) y retiro lógico.
```
