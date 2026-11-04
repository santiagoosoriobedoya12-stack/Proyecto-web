# Guía Maestra de Retroalimentación y Sustentación del Proyecto

> **Asignatura:** Aplicación y Servicios Web  
> **Proyecto:** Sistema Integral de Facturación Empresarial (`bdfacturas`)  
> **Integrantes:** Santiago Osorio Bedoya · Samuel López Jiménez  
> **Arquitectura:** C# .NET 10 (ASP.NET Core Web API + Dapper) · Blazor Server .NET 10 · SQL Server 2022 · Docker Compose  
> **Metodología:** Desarrollo Incremental Acumulativo ($v1 \subset v2 \subset v3 \subset v4$)

---

## 1. La Metodología Incremental Acumulativa

### El Principio Central
En el desarrollo de software empresarial moderno no se construyen "capas horizontales" desconectadas donde nada funciona hasta el final. Se construyen **cortes verticales acumulativos**:

$$v1 \subset v2 \subset v3 \subset v4$$

- **Inmutabilidad de Contratos:** Cuando una versión se cierra, sus endpoints, modelos y contratos quedan **congelados**. Una nueva versión agrega capacidades, pero **nunca rompe lo anterior**.
- **Regresión Continua:** Para dar por cerrada la Versión 2 o 3, todos los criterios de la Versión 1 deben continuar pasando al 100%.

---

## 2. Retroalimentación Detallada: Versión por Versión

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│                            EVOLUCIÓN DEL SISTEMA                                 │
│                                                                                  │
│   VERSIÓN 1: Las 6 tablas independientes (Arquitectura 3 capas limpia y Dapper)  │
│   ──────────► producto, persona, empresa, rol, ruta, usuario                     │
│                                                                                  │
│   VERSIÓN 2: Las 6 tablas dependientes (Integridad referencial y lógica en BD)   │
│   ──────────► cliente, vendedor, factura, productos, rol_usuario, rutarol        │
│               + Stored Procedures, Triggers, HTTP 409 y Maestro-Detalle          │
│                                                                                  │
│   VERSIÓN 3: El Control de Acceso (Seguridad perimetral y RBAC dinámico)        │
│   ──────────► BCrypt costo 12, JWT Bearer, HTTP 401 / 403, verificar_acceso_ruta │
│               y EstadoSesion scoped en Blazor                                    │
└──────────────────────────────────────────────────────────────────────────────────┘
```

---

### 🟢 VERSIÓN 1: Los Cimientos y el Corte Vertical

#### 1. ¿Qué problema resuelve la v1?
Construir los cimientos arquitectónicos del sistema operando las **6 tablas independientes** de `bdfacturas` (aquellas que no tienen ninguna clave foránea: `producto`, `persona`, `empresa`, `rol`, `ruta`, `usuario`).

#### 2. Principios de Diseño Clave:
- **Separación de Responsabilidades en 3 Capas:**
  - **Controlador:** Solo entiende de HTTP (verbos, códigos 200, 201, 204, 400, 404, 422, 500 y serialización JSON).
  - **Servicio:** Contiene las reglas del dominio puro. **No conoce HTTP**. Si una regla falla, lanza excepciones de negocio (`NoEncontradoExcepcion`, `ArgumentException`).
  - **Repositorio:** Ejecuta SQL escrito a mano contra SQL Server mediante **Dapper**. No conoce reglas de negocio ni HTTP.
- **Segregación de Peticiones (3 DTOs por Recurso):**
  - `*Crear.cs`: Todos los campos obligatorios para el `POST` $\rightarrow$ `201 Created`.
  - `*Reemplazo.cs`: Reemplazo total idempotente (`PUT` $\rightarrow$ `200 OK` o `422` si se omite un campo requerido).
  - `*Actualizar.cs`: Actualización parcial (`PATCH` $\rightarrow$ `200 OK` modificando solo los campos enviados).
- **Front Blazor Server:**
  - Consumo tipado con `HttpClient` inyectado en `Program.cs`.
  - Resiliencia ante desconexión: Si la API se apaga, la pantalla muestra alerta roja y 0 filas, sin colapsar la interfaz.

---

### 🟡 VERSIÓN 2: Integridad Referencial y Lógica en la Base

#### 1. ¿Qué problema resuelve la v2?
Completar las **12 tablas** del modelo introduciendo dependencias entre filas (`cliente`, `vendedor`, `factura`, `productosporfactura`, `rol_usuario`, `rutarol`, y `usuario_con_roles`).

#### 2. Principios de Diseño Clave:
- **Nuevo Código de Estado: `409 Conflict`:**
  - Captura el error de motor `SqlException 547` (violación de clave foránea) y `2627/2601` (duplicado en tabla puente) y lo traduce a `409 Conflict` a través de la excepción de dominio `ConflictoExcepcion`.
  - **Diferencia fundamental con 422:** El `422` es error de forma o sintaxis en el JSON enviado; el `409` ocurre cuando el JSON está perfecto pero choca contra el estado actual de la base de datos (ej: la persona asociada al cliente no existe).
- **Facturación Maestro-Detalle Delegada a SQL Server:**
  - La API **nunca calcula subtotales, totales ni precios unitarios**. El disparador `trg_prodfact_insert` y los procedimientos almacenados son la única fuente de verdad contable y de descuento de inventario.
  - No existe `DELETE` físico en la API: Se ejecuta `sp_anular_factura` (borrado lógico `estado = 0`) para restituir el stock.
- **Optimización de Front (Blazor Server):**
  - Desplegables dinámicos (`<select>`): Las claves foráneas se seleccionan de listas traídas de la API (muestra nombres, envía códigos).
  - **Carga Concurrente con `Task.WhenAll`:** En pantallas complejas (`Clientes.razor`, `Facturas.razor`), las peticiones a personas, empresas o catálogos se disparan en paralelo, evitando timeouts de 50 segundos.

---

### 🔴 VERSIÓN 3: El Control de Acceso (Seguridad, JWT y RBAC)

#### 1. ¿Qué problema resuelve la v3?
**«Ponerle la puerta a lo que ya existe»**. En la v1 y v2 cualquier cliente HTTP podía operar libremente; la v3 exige identificación y verifica permisos sin alterar los CRUDs ya construidos.

#### 2. Las Tres Piezas en Orden Estricto:
1. **Contraseña con Hash (`BCrypt` costo 12):**
   - Nunca texto plano. `BCrypt` incluye *salt* criptográfica automática: dos usuarios con la misma contraseña (`carlos123`) generan hashes completamente distintos, previniendo ataques de tablas arcoíris.
   - El factor de costo 12 hace el cálculo computacionalmente lento a propósito (un cuarto de segundo), imposibilitando ataques de fuerza bruta con GPU.
2. **Sesión sin Estado (Tokens JWT):**
   - Endpoint único de identificación: `POST /api/sesion` (`[AllowAnonymous]`).
   - Mismo mensaje de error 401 si el correo no existe o la contraseña está mal (previene enumeración de correos).
   - El token solo transporta el correo y los nombres de los roles; **nunca almacena los permisos**.
3. **Autorización Dinámica RBAC (`verificar_acceso_ruta`):**
   - Filtro `[ExigePermiso("interfaz.x")]` decorando los 12 controladores.
   - Consulta el Stored Procedure `verificar_acceso_ruta` en cada petición HTTP: **quitarle un permiso a un rol en la base surte efecto de inmediato sin re-loguearse (Criterio 7)**.
- **La Gran Diferencia: 401 vs 403:**
  - **`401 Unauthorized`:** *«No sé quién es usted»* (sin token, token alterado o expirado).
  - **`403 Forbidden`:** *«Sé quién es, pero su rol no tiene permiso para esta operación»*.
- **Blazor Server:**
  - `EstadoSesion` registrado como `scoped` (uno por circuito; el token vive en memoria del servidor y no baja al navegador).
  - Renderizado interactivo en `App.razor` sin dibujo previo (`prerender: false`).
  - Menú dinámico que oculta opciones según el rol (`GET /api/permisos/mios`).
  - **Criterio 9 Cumplido:** Escribir la URL a mano sin permiso (ej: `/usuarios`) responde 403 en la API y la vista muestra *"Su rol no tiene permiso para esta operacion."* con 0 filas.

---

## 3. Cuestionario de Examen y Preguntas de Sustentación

Estas son las 10 preguntas clásicas que un docente evaluador formula en la defensa del proyecto:

### 1. ¿Por qué usamos Dapper en lugar de Entity Framework Core?
> **Respuesta:** Dapper es un micro-ORM de alto rendimiento que deja el SQL a la vista. Permite escribir consultas optimizadas y mapear directamente los resultados de Procedimientos Almacenados sin la sobrecarga, migraciones mágicas ni el seguimiento de estado oculto de un ORM pesado.

### 2. ¿Por qué el servicio de negocio no devuelve `IActionResult` ni códigos HTTP?
> **Respuesta:** Por el principio de inversión de dependencias y separación de capas. El servicio representa la lógica de dominio puro y debe ser reutilizable (por ejemplo, en una aplicación de consola, un worker o una cola RabbitMQ). Si el servicio supiera de `IActionResult` o códigos HTTP como 404, estaría acoplado al protocolo web.

### 3. ¿Por qué tenemos tres clases de petición (`*Crear`, `*Reemplazo`, `*Actualizar`) por cada recurso?
> **Respuesta:** Porque los verbos HTTP tienen semánticas distintas:
> - `POST` crea un recurso y exige todos los datos obligatorios.
> - `PUT` es un reemplazo total idempotente; si falta un campo debe responder `422 Unprocessable Entity`.
> - `PATCH` es una actualización parcial; todos sus campos deben ser opcionales y solo modifica los que el cliente envíe. Una sola clase haría imposible validar estas reglas.

### 4. ¿Cuál es la diferencia técnica entre el error 409 y el error 422?
> **Respuesta:**
> - **422 Unprocessable Entity:** Error de **forma o sintaxis** en el cuerpo de la petición (ej: texto donde iba un entero, o campo obligatorio omitido).
> - **409 Conflict:** La petición está perfectamente construida sintácticamente, pero entra en **conflicto con el estado actual del almacén de datos** (ej: violar una clave foránea porque el padre no existe, o intentar insertar un duplicado en una tabla puente).

### 5. ¿Por qué la creación de facturas no envía precios, subtotales ni totales desde la API?
> **Respuesta:** Para preservar la integridad contable y prevenir fraudes o condiciones de carrera (*race conditions*). El cliente solo envía el cliente, el vendedor y la lista de códigos con sus cantidades. El disparador en SQL Server consulta el precio oficial del producto, valida el inventario disponible, calcula el subtotal y acumula el total en una sola transacción atómica.

### 6. ¿Por qué usamos `Task.WhenAll` en Blazor Server?
> **Respuesta:** Si una pantalla necesita cargar personas, empresas y clientes de la API secuencialmente con `await`, los tiempos de latencia de red se suman (ej: 3 llamadas de 200 ms tardarían 600 ms). Con `Task.WhenAll`, las tres peticiones se disparan concurrentemente en paralelo, tardando solo lo que tarde la más lenta.

### 7. ¿Por qué usamos BCrypt en lugar de SHA-256 para las contraseñas?
> **Respuesta:** SHA-256 es un algoritmo de hashing diseñado para ser extremadamente rápido, lo que permite a un atacante con una GPU probar miles de millones de combinaciones por segundo. BCrypt está diseñado para ser **lento a propósito con factor de costo ajustable (costo 12)** y genera una **salt criptográfica automática**, evitando ataques por fuerza bruta y tablas arcoíris.

### 8. ¿Por qué no guardamos los permisos de ruta dentro del token JWT?
> **Respuesta:** Porque un token JWT es inmutable una vez firmado. Si los permisos estuvieran dentro del token, revocarle un permiso a un empleado no surtiría efecto hasta que su token expire (ej: 1 hora después). Al consultar el permiso en la base de datos con `verificar_acceso_ruta` en cada petición, la revocación de acceso tiene efecto instantáneo (Criterio 7).

### 9. ¿Cuál es la diferencia entre el error 401 y el error 403?
> **Respuesta:**
> - **401 Unauthorized:** Significa *«No sé quién es usted»* (credenciales incorrectas, token ausente, alterado o expirado).
> - **403 Forbidden:** Significa *«Sé quién es usted (su token es válido), pero sus roles no tienen autorización para acceder a este recurso»*.

### 10. ¿Por qué ocultar botones en el menú de Blazor no es suficiente seguridad?
> **Respuesta:** Porque ocultar un botón en la interfaz gráfica es solo comodidad visual (UX). Cualquier usuario puede escribir la URL en la barra de direcciones o enviar una petición con `curl` o Postman. La verdadera seguridad reside en el backend: si un usuario sin rol autorizado intenta acceder al endpoint, la API lo rechaza con un **403 Forbidden** real (Criterio 9).

---

## 4. El Rol de la Bitácora de Prompts en el Repositorio

En cada una de las carpetas de versión se incluyó el archivo `prompts.md`:
- `docs/v1_sin_fk/prompts.md`
- `docs/v2_con_fk/prompts.md`
- `docs/v3_seguridad_rbac/prompts.md`

Este archivo documenta la ingeniería de prompts asistida por Inteligencia Artificial que estructuró:
1. El diseño arquitectónico y desacoplamiento en 3 capas.
2. El corte vertical individual de cada tabla del modelo.
3. Las reglas de integridad relacional y transaccionalidad.
4. Los esquemas de autenticación JWT y autorización RBAC.

