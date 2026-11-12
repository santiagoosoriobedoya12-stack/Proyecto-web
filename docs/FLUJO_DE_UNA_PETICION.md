# Flujo de una Petición (Flask → FastAPI → Servicio → Repositorio → MariaDB)

Trazabilidad real de extremo a extremo cuando un usuario crea o actualiza un registro en **Mapa de Conocimiento**.

---

## Caso 1: Crear un Producto de Investigación (`POST /productos/accion` → `POST /api/producto`)

1. **Navegador → Frontend Flask (`8116`)**
   - El usuario abre `/productos` en [`front_flask/app.py`](../front_flask/app.py).
   - `_ver_crud("productos")` usa `ThreadPoolExecutor` para pedir en paralelo la lista de productos (`GET /api/producto`) y los catálogos foráneos `proyectos` (`GET /api/proyecto`) y `tipos_producto` (`GET /api/tipo-producto`).
   - Al enviar el formulario, el navegador hace `POST /productos/accion` con `modo="crear"`.
2. **Cliente HTTP y Sesión en Servidor (`front_flask/servicios/`)**
   - [`cliente_api.py`](../front_flask/servicios/cliente_api.py) recupera el JWT asociado a la cookie `HttpOnly` `mapa_sid`.
   - [`operaciones_api.py`](../front_flask/servicios/operaciones_api.py) ejecuta `crear_producto(sesion, body)` enviando `POST http://api-mapa:8117/api/producto` con cabecera `Authorization: Bearer <token>`.
3. **Frontera HTTP y Seguridad en FastAPI (`8117`)**
   - [`controlador_v2.py`](../api_mapa/controladores/controlador_v2.py) ejecuta primero la dependencia `exige_roles("Administrador", "Investigador")` en [`autorizacion/dependencias.py`](../api_mapa/autorizacion/dependencias.py):
     - Sin token o token expirado → `NoAutenticadoExcepcion` (**HTTP `401`**).
     - Rol `Consulta` → `SinPermisoExcepcion` (**HTTP `403`**).
   - FastAPI valida el JSON contra `ProductoCrear` en [`peticiones/esquemas.py`](../api_mapa/peticiones/esquemas.py): si falta un campo o el tipo no coincide → **HTTP `422`**.
4. **Capa de Servicio (`servicios/servicio_v2.py`)**
   - `ServicioProducto.crear(pet)` verifica mediante composición que:
     - `tipo_producto` exista y esté activo en `IRepositorioTipoProducto` (si no → `ConflictoExcepcion` **HTTP `409`**).
     - `proyecto` (si no es `None`) exista y esté activo en `IRepositorioProyecto` (si no → **HTTP `409`**).
5. **Capa de Repositorio (`repositorios/repositorio_v2.py` + `database.py`)**
   - `RepositorioProductoMariaDB.crear(pet)` abre conexión con `DictCursor` y `CLIENT.FOUND_ROWS` en [`database.py`](../api_mapa/database.py).
   - Ejecuta `INSERT INTO producto (..., activo) VALUES (..., 1)` con parámetros `%s` (sin interpolación de texto).
   - Si el `id` ya existía → `extraer_codigo_mysql` detecta código `1062` y el repositorio lanza `ConflictoExcepcion` (**HTTP `409`**).
6. **Respuesta de Retorno**
   - El repositorio devuelve la entidad `Producto`; el controlador responde **HTTP `201 Created`** con el objeto creado; Flask redirige `303 See Other` a `/productos` mostrando el mensaje de éxito.

---

## Caso 2: Retiro Lógico (`DELETE /api/proyecto/{id}`)

1. Desde la tabla HTML en Flask, el usuario pulsa **Retirar** (`POST /proyectos/retirar`).
2. Flask llama `DELETE /api/proyecto/{id}` con el JWT del usuario.
3. `RepositorioProyectoMariaDB.eliminar(id)` ejecuta:
   ```sql
   UPDATE proyecto SET activo = 0 WHERE id = %s AND activo = 1
   ```
4. Si `cursor.rowcount == 0` (el proyecto no existe o ya había sido retirado) → `NoEncontradoExcepcion` (**HTTP `404`**).
5. Si afectó 1 fila → **HTTP `204 No Content`**. Ninguna consulta `SELECT` volverá a incluirlo porque todas filtran `activo = 1`.
