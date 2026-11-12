# Guía Maestra de Retroalimentación y Sustentación del Proyecto

> **Asignatura:** Aplicación y Servicios Web  
> **Proyecto:** Sistema Integral de Facturación Empresarial (`bdfacturas`)  
> **Autor:** Santiago Osorio Bedoya  
> **Arquitectura:** Python 3.12 (FastAPI + PyMSSQL + PyJWT + BCrypt) · Frontend Python (Flask + Jinja2 + httpx + Bootstrap local) · SQL Server 2022 · Docker Compose  
> **Metodología:** Desarrollo Incremental Acumulativo ($v1 \subset v2 \subset v3$)

---

## 1. La Metodología Incremental Acumulativa

### El Principio Central
En el desarrollo de software empresarial moderno no se construyen capas horizontales desconectadas donde nada funciona hasta el final. Se construyen **cortes verticales acumulativos**:

$$v1 \subset v2 \subset v3$$

- **Inmutabilidad de Contratos:** Cuando una versión se cierra, sus endpoints y contratos JSON quedan **congelados**. Una nueva versión agrega capacidades, pero **nunca rompe lo anterior**.
- **Regresión Continua:** Para dar por cerrada cualquier versión superior, todos los flujos de versiones anteriores deben seguir respondiendo correctamente.

---

## 2. Retroalimentación Detallada: Versión por Versión

```text
┌──────────────────────────────────────────────────────────────────────────────────────┐
│                              EVOLUCIÓN DEL SISTEMA                                   │
│                                                                                      │
│   VERSIÓN 1: Las 6 tablas independientes (3 capas en FastAPI + PyMSSQL + Jinja2)     │
│   ──────────► producto, persona, empresa, rol, ruta, usuario                         │
│                                                                                      │
│   VERSIÓN 2: Las 6 tablas dependientes (Integridad referencial y lógica en BD)       │
│   ──────────► cliente, vendedor, factura, productosporfactura, rol_usuario, rutarol  │
│               + Stored Procedures, Triggers, HTTP 409 y Maestro-Detalle              │
│                                                                                      │
│   VERSIÓN 3: El Control de Acceso (Seguridad perimetral y RBAC dinámico)             │
│   ──────────► BCrypt costo 12, PyJWT HS256, HTTP 401 / 403, verificar_acceso_ruta    │
│               y sesión en servidor con cookie HttpOnly en front_python               │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

---

### VERSIÓN 1: Los Cimientos y el Corte Vertical

#### 1. ¿Qué problema resuelve la v1?
Construir los cimientos arquitectónicos del sistema operando las **6 tablas independientes** de `bdfacturas` (aquellas que no tienen clave foránea: `producto`, `persona`, `empresa`, `rol`, `ruta`, `usuario`).

#### 2. Principios de Diseño Clave en Python:
- **Separación estricta en 3 Capas (`api_python/`):**
  - **Controlador (`controladores/controlador_v1.py`):** Solo entiende de HTTP (verbos FastAPI `@router.get`, `@router.post`, códigos `200`, `201`, `400`, `404`, `409`, `422`, `500` y respuestas JSON). Recibe servicios ensamblados en `ensamblador.py`.
  - **Modelos (`modelos/entidades.py`):** Clases POO tipadas de dominio (`Producto`, `Cliente`, `Factura`, `Sesion`, etc.).
  - **Servicio (`servicios/servicio_v1.py`):** Contiene las reglas del dominio puro (`ServicioProducto`, `ServicioPersona`, etc.) recibiendo la interfaz `IRepositorio*` por constructor. **No conoce HTTP ni `pymssql`**. Si falla una regla, lanza excepciones de dominio (`NoEncontradoExcepcion`, `ValidacionNegocioExcepcion`, `ConflictoExcepcion`).
  - **Repositorio (`repositorios/repositorio_v1.py`):** Implementa `IRepositorio*` (`ABC`) ejecutando SQL escrito a mano contra SQL Server mediante **PyMSSQL** con consultas parametrizadas (`%s`). No conoce reglas de negocio ni HTTP.
- **Segregación de Peticiones (3 esquemas Pydantic por recurso en `api_python/peticiones/esquemas.py`):**
  - `*Crear`: Campos obligatorios para `POST` $\rightarrow$ `201 Created`.
  - `*Reemplazo`: Reemplazo total idempotente para `PUT` $\rightarrow$ `200 OK` o `422` si falta un campo requerido.
  - `*Actualizar`: Todos los campos opcionales para actualización parcial `PATCH` $\rightarrow$ `200 OK` modificando solo lo enviado.
- **Pruebas unitarias aisladas (`api_python/pruebas/test_capas.py`):**
  - Demuestran que los servicios funcionan contra repositorios falsos en memoria sin requerir SQL Server levantado.
- **Frontend aislado (`front_python/`):**
  - Consume la API exclusivamente vía `httpx` en `front_python/servicios/cliente_api.py`. Sin `pymssql`.
  - Resiliencia ante desconexión: Si `api-facturas` está apagada, las vistas HTML renderizan alerta roja y 0 filas sin colapsar.

---

### VERSIÓN 2: Integridad Referencial y Lógica en la Base

#### 1. ¿Qué problema resuelve la v2?
Completar las **12 tablas** del modelo introduciendo dependencias entre filas (`cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, y el recurso compuesto `usuario-con-roles`).

#### 2. Principios de Diseño Clave en Python:
- **Nuevo Código de Estado: `409 Conflict`:**
  - `api_python/database.py` (`extraer_codigo_sql`) inspecciona el código numérico de SQL Server: `547` (violación de clave foránea), `2627`/`2601` (duplicado único) o `50000` (`RAISERROR` de negocio) y el repositorio los traduce a `ConflictoExcepcion` $\rightarrow$ HTTP `409`.
  - **Diferencia fundamental con `422`:** El `422` ocurre en Pydantic por error de forma o tipo en el JSON enviado; el `409` ocurre cuando el JSON es válido sintácticamente pero choca contra la integridad actual de SQL Server.
- **Facturación Maestro-Detalle delegada a SQL Server (`RepositorioFactura`):**
  - Python **nunca calcula subtotales, totales ni precios unitarios**. El cliente solo envía cliente, vendedor y pares `{ codigo, cantidad }`.
  - `sp_insertar_factura_y_productosporfactura` y el disparador `trg_actualizar_totales_y_stock` validan stock, calculan precios reales y descuentan inventario atómicamente.
  - Borrado lógico contable: `DELETE /api/factura/{numero}` ejecuta `sp_anular_factura` estableciendo `estado = 'anulada'` y restituyendo inventario.
- **Usuario con Roles mediante `OPENJSON`:**
  - `ServicioUsuarioConRoles` transforma `[1, 2]` en JSON `[{"fkidrol": 1}, {"fkidrol": 2}]`, hashea la contraseña nueva con BCrypt costo 12 y llama los SPs `crear_usuario_con_roles` (`@p_roles_json`) y `actualizar_usuario_con_roles` (`@p_roles`).
- **Concurrencia en `front_python/` (Flask):**
  - Carga paralela con `ThreadPoolExecutor` en `front_python/app.py` para poblar simultáneamente desplegables de personas, empresas, clientes y vendedores sin sumar latencias.

---

### VERSIÓN 3: El Control de Acceso (Seguridad, JWT y RBAC)

#### 1. ¿Qué problema resuelve la v3?
Proteger los 12 recursos de las versiones anteriores exigiendo autenticación e impidiendo acceso sin rol autorizado, sin romper ningún contrato JSON previo.

#### 2. Las Tres Piezas en Orden Estricto:
1. **Contraseña con Hash (`bcrypt` costo 12):**
   - Salt aleatoria por usuario y costo computacional 12 (`$2a$12$` / `$2b$12$`), impidiendo ataques por tablas arcoíris y fuerza bruta.
2. **Sesión sin Estado (`PyJWT` HS256):**
   - Endpoint público `POST /api/sesion` (`controlador_sesion.py`).
   - Devuelve el **mismo error 401** tanto para correo inexistente como para contraseña errónea, evitando enumeración de usuarios.
   - El token solo transporta `sub` (email) y `roles`, **nunca guarda permisos de rutas**.
3. **Autorización Dinámica RBAC (`verificar_acceso_ruta`):**
   - Dependencias FastAPI `exige_usuario_autenticado` (`401` con `leeway=0`) y `exige_permiso("interfaz.x")` (`403`) definidas en `api_python/autorizacion/dependencias.py`.
   - En cada petición consulta a SQL Server a través de `RepositorioAcceso`: **quitar un permiso a un rol en la BD bloquea al usuario de inmediato sin esperar que expire su JWT**.
- **Seguridad en `front_python/`:**
  - Almacén `SESIONES` en memoria del servidor vinculado al navegador mediante cookie `HttpOnly` (`factucorp_sid`). El JWT nunca viaja a `localStorage` ni se imprime en HTML.
  - Menú filtrado dinámicamente vía `GET /api/permisos/mios`. Si un usuario digita manualmente una URL prohibida (ej. `/usuarios` como vendedor), FastAPI en la API rechaza con `403` real y el front muestra aviso de permiso denegado y cero filas.

---

## 3. Cuestionario de Examen y Preguntas de Sustentación

Estas son las 10 preguntas clave que un docente evaluador suele hacer en la defensa:

### 1. ¿Por qué usamos PyMSSQL con SQL directo en lugar de SQLAlchemy?
> **Respuesta:** Porque mantiene cada sentencia SQL explícita y auditable dentro de la capa de repositorio, facilita ejecutar Stored Procedures con parámetros de salida o tablas JSON (`OPENJSON`) sin fricciones de mapeo y evita la sobrecarga oculta de un ORM pesado.

### 2. ¿Por qué la capa de servicio no devuelve códigos HTTP (`404`, `409`) directamente?
> **Respuesta:** Por separación de responsabilidades e inversión de dependencias. Los servicios (`servicio_v1.py`, `servicio_v2.py`, etc.) encapsulan lógica de dominio pura y pueden probarse unitariamente (`test_capas.py`) sin servidor HTTP. FastAPI traduce nuestras excepciones de dominio (`NoEncontradoExcepcion`, `ConflictoExcepcion`) a códigos HTTP en `api_python/main.py`.

### 3. ¿Por qué tenemos tres esquemas Pydantic (`*Crear`, `*Reemplazo`, `*Actualizar`) por tabla?
> **Respuesta:** Porque `POST`, `PUT` y `PATCH` tienen semánticas distintas: `POST` exige todos los campos iniciales, `PUT` exige reemplazo completo (`422` si falta algún campo) y `PATCH` permite enviar únicamente los atributos que cambian.

### 4. ¿Cuál es la diferencia técnica entre `409 Conflict` y `422 Unprocessable Entity`?
> **Respuesta:** `422` indica que el cuerpo JSON incumple la estructura o tipos definidos en Pydantic (error de forma). `409` indica que el JSON está bien formado, pero la operación choca contra el estado de SQL Server (FK inexistente `547`, clave duplicada `2627`, o stock insuficiente `50000`).

### 5. ¿Por qué al emitir facturas no enviamos precios ni totales desde el front ni desde Python?
> **Respuesta:** Para garantizar integridad contable y evitar manipulación de precios o condiciones de carrera en inventario. El procedimiento almacenado y el trigger `trg_actualizar_totales_y_stock` leen el precio oficial desde `producto`, validan inventario y actualizan totales dentro de una transacción atómica en SQL Server.

### 6. ¿Por qué en `front_python/` usamos `ThreadPoolExecutor` con `httpx`?
> **Respuesta:** Porque cargar múltiples catálogos de forma secuencial suma las latencias de cada petición HTTP. Con `ThreadPoolExecutor`, las consultas viajan concurrentemente en paralelo mientras Flask renderiza la vista HTML.

### 7. ¿Por qué usamos BCrypt (costo 12) en vez de SHA-256 para las contraseñas?
> **Respuesta:** SHA-256 está diseñado para máxima velocidad, lo que facilita ataques por fuerza bruta en GPU. BCrypt añade una *salt* aleatoria automática y un factor de costo configurable (`rounds=12`) que hace deliberadamente costoso probar contraseñas.

### 8. ¿Por qué el JWT no almacena la lista de permisos (`interfaz.usuarios`, etc.)?
> **Respuesta:** Porque un JWT es válido hasta que expira. Si los permisos viajaran dentro del token, quitarle acceso a un usuario no tendría efecto inmediato. Al consultar `verificar_acceso_ruta` en SQL Server en cada petición (`exige_permiso`), cualquier cambio en roles se refleja al instante.

### 9. ¿Cuál es la diferencia entre `401 Unauthorized` y `403 Forbidden`?
> **Respuesta:** `401` significa que no hay identidad válida (credenciales erróneas, falta cabecera Bearer, token expirado o alterado). `403` significa que el token JWT es válido, pero el rol del usuario no tiene asignada esa ruta en `rutarol`.

### 10. ¿Por qué ocultar opciones en el menú HTML de Jinja2 no basta como seguridad?
> **Respuesta:** Ocultar enlaces en el menú mejora la experiencia de usuario, pero cualquiera puede visitar `/usuarios` directamente en el navegador o enviar una petición con `curl` a la API. La seguridad real vive en la API Python mediante `Depends(exige_permiso(...))`, respaldada por SQL Server.

---

## 4. El Rol de la Bitácora de Prompts en el Repositorio

Cada versión documenta su archivo `prompts.md`:
- `docs/v1_sin_fk/prompts.md`
- `docs/v2_con_fk/prompts.md`
- `docs/v3_seguridad_rbac/prompts.md`

Estos archivos registran los prompts diseñados para construir paso a paso la arquitectura en 3 capas en Python, la integridad referencial y la seguridad JWT/RBAC.
