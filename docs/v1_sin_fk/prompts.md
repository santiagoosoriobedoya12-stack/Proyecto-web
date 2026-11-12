# Bitácora de Prompts — Versión 1: Las 6 Tablas Sin Clave Foránea (Python)

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar la arquitectura en 3 capas en Python (**FastAPI + PyMSSQL**), los esquemas Pydantic segregados por verbo y las vistas HTML en **Jinja2** (`front_python/`) para cada una de las 6 tablas independientes de `bdfacturas`.

---

## 1. Prompt Arquitectónico Base (Vertical Slice en Python)
```text
Actúa como un arquitecto de software senior en Python con FastAPI.
Diseña una arquitectura en capas estricta para la API de facturación bdfacturas:
- Router (api_python/routers/): solo conoce verbos HTTP, códigos de estado (200, 201, 204, 400, 404, 409, 422, 500) y el sobre de respuesta { tabla, total, datos }.
- Servicio (api_python/servicios/): encapsula reglas de negocio puras, lanza excepciones de dominio (NoEncontradoExcepcion, ValidacionNegocioExcepcion, ConflictoExcepcion) y NO conoce HTTP ni PyMSSQL.
- Repositorio (api_python/repositorios/): usa PyMSSQL con SQL parametrizado (%s) contra SQL Server sin ningún ORM pesado.
- Separación de esquemas Pydantic (api_python/peticiones/esquemas.py): 3 esquemas por recurso (*Crear para POST 201, *Reemplazo para PUT 422, *Actualizar con todo opcional para PATCH 200).
```

---

## 2. Prompts Específicos por Tabla

### Tabla 1: `producto`
```text
Genera el corte vertical completo en Python para la entidad 'producto':
1. Esquemas Pydantic: ProductoCrear (POST), ProductoReemplazo (PUT), ProductoActualizar (PATCH).
2. RepositorioProducto en api_python/repositorios/repositorio_v1.py heredando de RepositorioTablaBase con PyMSSQL.
3. ServicioProducto en api_python/servicios/servicio_v1.py con validaciones de negocio.
4. Rutas /api/producto en api_python/routers/router_v1.py con sobre estándar { tabla, total, datos }.
5. Vista Jinja2 en front_python/templates/crud_generico.html para /productos consumiendo la API por httpx desde cliente_api.py, con manejo resiliente ante caída de la API.
```

### Tabla 2: `persona`
```text
Genera el corte vertical completo en Python para la entidad 'persona':
1. Esquemas Pydantic: PersonaCrear, PersonaReemplazo, PersonaActualizar (codigo PK, nombre, email, telefono).
2. RepositorioPersona con SQL parametrizado en PyMSSQL.
3. ServicioPersona y rutas /api/persona en router_v1.py.
4. Integración en /personas dentro de front_python/main.py con edición total (PUT) y parcial (PATCH).
```

### Tabla 3: `empresa`
```text
Genera el corte vertical completo en Python para la entidad 'empresa':
1. Esquemas EmpresaCrear, EmpresaReemplazo, EmpresaActualizar (codigo PK, nombre).
2. RepositorioEmpresa, ServicioEmpresa y endpoints /api/empresa.
3. Vista /empresas en front_python/ conectada vía httpx.
```

### Tabla 4: `rol`
```text
Genera el corte vertical en Python para el catálogo de 'rol':
1. Esquemas RolCrear, RolReemplazo, RolActualizar (id PK autoincremental IDENTITY, nombre).
2. RepositorioRol, ServicioRol y endpoints /api/rol.
3. Vista /roles en front_python/ con manejo de error de duplicado (409).
```

### Tabla 5: `ruta`
```text
Genera el corte vertical en Python para el catálogo de 'ruta':
1. Esquemas RutaCrear, RutaReemplazo, RutaActualizar (id PK IDENTITY, nombre, descripcion).
2. RepositorioRuta, ServicioRuta y endpoints /api/ruta.
3. Vista /rutas en front_python/ para administrar rutas protegibles.
```

### Tabla 6: `usuario`
```text
Genera el corte vertical en Python para la entidad 'usuario':
1. Esquemas UsuarioCrear, UsuarioReemplazo, UsuarioActualizar (email PK, contrasena).
2. RepositorioUsuario, ServicioUsuario (aplicando hash BCrypt costo 12 al guardar) y endpoints /api/usuario.
3. Vista /usuarios en front_python/ conectada al CRUD /api/usuario.
```
