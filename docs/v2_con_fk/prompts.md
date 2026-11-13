# Bitácora de Prompts — Versión 2: Las 10 Tablas Con Clave Foránea y Puentes M:N

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar la integridad referencial en capas, el manejo de excepciones HTTP 409 Conflict, las relaciones muchos a muchos, la carga paralela con `ThreadPoolExecutor` en Flask y las plantillas de vinculación.

---

## 1. Prompt Arquitectónico Base (Integridad Referencial y 409 Conflict en Python)

```text
Actúa como un arquitecto de software senior en Python, FastAPI, MariaDB y Flask.
Implementa el crecimiento sobre la versión 1 (v1 ⊂ v2) para las 10 tablas con claves foráneas:
1. Regla de Negocio de Padres Activos:
   - Antes de insertar o actualizar un registro con clave foránea, el servicio debe verificar que el padre referenciado exista y tenga `activo = 1`.
   - Si el padre no existe o fue retirado lógicamente, lanza `ConflictoExcepcion`, mapeada a HTTP 409 Conflict.
2. Captura de Errores Nativos de MariaDB:
   - En database.py, implementa extraer_codigo_mysql(ex) para interceptar:
     * 1062 (Duplicate entry): clave primaria o única duplicada -> ConflictoExcepcion 409.
     * 1452 (Cannot add or update child row): FK no válida -> ConflictoExcepcion 409.
     * 1451 (Cannot delete or update parent row): registro tiene dependientes -> ConflictoExcepcion 409.
3. Factoría de Routers para Tablas Puente M:N:
   - En controlador_v2.py, implementa una función factoría `_crear_router_puente` que genere los 5 endpoints REST explícitos por cada tabla intermedia:
     * GET "" (listar todos activos)
     * GET "/{clave_izq}/{valor}" (listar asociados por izquierda)
     * GET "/{clave_der}/{valor}" (listar asociados por derecha)
     * POST "" (vincular dos registros -> 201 Created o 409 Conflict si ya existe)
     * DELETE "/{valor_izq}/{valor_der}" (retiro lógico -> 204 No Content o 404)
4. Frontend Flask con Concurrencia (ThreadPoolExecutor):
   - En app.py, para vistas que requieren múltiples catálogos foráneos (docentes, productos, puentes), usa concurrent.futures.ThreadPoolExecutor para pedir los catálogos a la API en paralelo y poblar los <select> sin demoras.
```

---

## 2. Prompts Específicos por Tabla de Versión 2

### Tabla 9: `docente` (FK a `linea_investigacion`)
```text
Genera el corte vertical completo para 'docente':
1. Modelo Docente (@dataclass con los campos exactos del DDL oficial: cedula PK, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, telefono, url_cvlac, fecha_actualizacion, escalafon, perfil, cat_minciencia, conv_minciencia, nacionalidaad [con doble a], linea_investigacion_principal FK nullable).
2. DTOs Pydantic: DocenteCrear, DocenteReemplazo, DocenteActualizar.
3. ServicioDocente: valida que si linea_investigacion_principal viene definida, la línea exista y tenga activo = 1; si no, lanza ConflictoExcepcion (409).
4. Router FastAPI en /api/docente.
5. Vista Flask en /docentes con selector desplegable de líneas de investigación cargado dinámicamente desde la API.
```

### Tabla 10: `producto` (FK a `proyecto` y `tipo_producto`)
```text
Genera el corte vertical para 'producto':
1. Modelo Producto (@dataclass: id PK, nombre, categoria, fecha_entrega, proyecto FK nullable, tipo_producto FK obligatoria).
2. DTOs: ProductoCrear, ProductoReemplazo, ProductoActualizar.
3. ServicioProducto: valida mediante composición con IRepositorioTipoProducto e IRepositorioProyecto que ambos padres existan y estén activos (409 si falla alguno).
4. Router FastAPI en /api/producto.
5. Vista Flask en /productos con dos selectores desplegables (Proyecto y Tipo de Producto) cargados en paralelo.
```

### Tabla 11: `desarrolla` (Participación Docente-Proyecto con Atributos)
```text
Genera el módulo de vinculación docente-proyecto con atributos propios:
1. Modelo Desarrolla (@dataclass: docente FK, proyecto FK, rol, descripcion).
2. DTOs: DesarrollaCrear, DesarrollaReemplazo, DesarrollaActualizar.
3. ServicioDesarrolla con métodos:
   - listar()
   - obtener_por_docente(cedula)
   - obtener_por_proyecto(id_proyecto)
   - crear(pet) -> 409 si ya existe la participación o si docente/proyecto están inactivos
   - reemplazar(cedula, id_proyecto, pet)
   - actualizar_parcial(cedula, id_proyecto, pet)
   - eliminar(cedula, id_proyecto) -> borrado lógico
4. Router FastAPI en /api/desarrolla.
5. Plantilla Jinja2 desarrolla.html con tabla de participaciones, formulario de asignación y edición.
```

### Tablas 12 a 18: Las 7 Tablas Puente M:N
```text
Genera los cortes verticales para las 7 relaciones muchos a muchos del módulo:
1. 'aliado_proyecto' (aliado, proyecto) en /api/aliado-proyecto y /aliados-proyecto.
2. 'palabras_clave' (proyecto, termino_clave) en /api/palabras-clave y /palabras-clave.
3. 'ac_proyecto' (proyecto, area_conocimiento) en /api/ac-proyecto y /ac-proyecto.
4. 'proyecto_linea' (proyecto, linea_investigacion) en /api/proyecto-linea y /proyecto-linea.
5. 'ods_proyecto' (proyecto, ods) en /api/ods-proyecto y /ods-proyecto.
6. 'aa_proyecto' (proyecto, area_aplicacion) en /api/aa-proyecto y /aa-proyecto.
7. 'docente_producto' (docente, producto) en /api/docente-producto y /docente-producto.
Cada una con:
- Clave primaria compuesta en MariaDB.
- DTO Pydantic de vinculación.
- ServicioPuente genérico parametrizado.
- Router FastAPI explícito.
- Plantilla Jinja2 puente.html con selección bilateral de registros activos y botón Retirar.
```
