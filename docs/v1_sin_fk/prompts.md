# Bitácora de Prompts — Versión 1: Las 6 Tablas Sin Clave Foránea

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar la arquitectura en 3 capas, los DTOs segregados, los repositorios Dapper y las interfaces en Blazor para cada una de las 6 tablas independientes de `bdfacturas`.

---

## 1. Prompt Arquitectónico Base (Vertical Slice)
```text
Actúa como un arquitecto de software senior en C# y .NET 10.
Diseña una arquitectura en capas estricta para la API de facturación bdfacturas:
- Controlador: solo conoce verbos HTTP, códigos de estado (200, 201, 204, 400, 404, 422, 500) y el sobre de respuesta { tabla, total, datos }.
- Servicio: encapsula reglas de negocio puras, lanza excepciones de dominio (NoEncontradoExcepcion, ReglaNegocioExcepcion) y NO conoce HTTP ni IActionResult.
- Repositorio: usa Dapper para ejecutar SQL puro contra SQL Server sin ningún ORM pesado.
- Separación de DTOs: 3 peticiones por recurso (*Crear con todo obligatorio para POST, *Reemplazo con todo obligatorio para PUT 422, *Actualizar con todo opcional para PATCH 200).
```

---

## 2. Prompts Específicos por Tabla

### Tabla 1: `producto`
```text
Genera el corte vertical completo para la entidad 'producto':
1. Modelo Producto.cs (codigo alfanumérico PK, nombre, stock int, valorunitario decimal).
2. DTOs: ProductoCrear (POST), ProductoReemplazo (PUT), ProductoActualizar (PATCH).
3. Repositorio IRepositorioProducto y RepositorioProductoSqlServer con Dapper.
4. Servicio IServicioProducto y ServicioProducto con validaciones de negocio.
5. Controlador ProductoController con sobre estándar de respuesta.
6. Componente Blazor Server Productos.razor con listado interactivo, formularios modales o desplegables, y manejo de desconexión resiliente.
```

### Tabla 2: `persona`
```text
Genera el corte vertical completo para la entidad 'persona':
1. Modelo Persona.cs (codigo PK, nombre, apellido, documento, email, telefono).
2. DTOs segregados: PersonaCrear, PersonaReemplazo, PersonaActualizar.
3. Repositorio con consultas SQL directas en Dapper.
4. Servicio de dominio y Controlador PersonaController.
5. Componente Blazor Personas.razor con botones de edición total (PUT) y parcial (PATCH).
```

### Tabla 3: `empresa`
```text
Genera el corte vertical completo para la entidad 'empresa':
1. Modelo Empresa.cs (codigo PK, razonsocial, nit, direccion, telefono).
2. Peticiones segregadas para POST, PUT y PATCH.
3. Repositorio Dapper, Servicio y Controlador EmpresaController.
4. Vista Blazor Empresas.razor conectada al servicio HTTP tipado.
```

### Tabla 4: `rol`
```text
Genera el corte vertical para el catálogo de 'rol':
1. Modelo Rol.cs (id PK autoincremental IDENTITY, nombre).
2. DTOs de creación y actualización de roles.
3. Repositorio Dapper, Servicio y RolController.
4. Vista Blazor Roles.razor con validaciones de unicidad.
```

### Tabla 5: `ruta`
```text
Genera el corte vertical para el catálogo de 'ruta':
1. Modelo Ruta.cs (id PK autoincremental IDENTITY, ruta/patrón, descripción).
2. DTOs segregados para creación y actualización.
3. Repositorio Dapper, Servicio y RutaController.
4. Vista Blazor Rutas.razor para administrar las rutas protegibles del sistema.
```

### Tabla 6: `usuario`
```text
Genera el corte vertical para la entidad 'usuario':
1. Modelo Usuario.cs (email PK, contrasena).
2. DTOs UsuarioCrear, UsuarioReemplazo, UsuarioActualizar.
3. Repositorio Dapper, Servicio y UsuarioController.
4. Vista Blazor Usuarios.razor con encriptación y listado protegido.
```

