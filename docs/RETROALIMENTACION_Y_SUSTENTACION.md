# Guía de Retroalimentación y Sustentación (Interpretabilidad)

> Material de estudio directo sobre el código real del proyecto **Mapa de Conocimiento** para responder preguntas de arquitectura, SQL, seguridad y POO durante la sustentación.

---

## 1. Decisiones Clave del Diseño (Qué responder si el docente pregunta)

### ¿Por qué el proyecto usa `activo` en las 18 tablas del módulo además de `rol` y `usuario`?
La metodología del curso (`0_METODOLOGIA.md` §7.0) establece **borrado lógico obligatorio**: ninguna operación `DELETE` destruye filas físicamente ni rompe auditoría histórica. Por eso [`db/init.sql`](../db/init.sql) añade `activo TINYINT(1) NOT NULL DEFAULT 1` a las 18 tablas de Mapa de Conocimiento. Todas las lecturas filtran `activo = 1` y todo `DELETE` ejecuta `UPDATE ... SET activo = 0 WHERE ... AND activo = 1` (si se repite el `DELETE`, `rowcount` es `0` y la API devuelve `404 Not Found`).

### ¿Por qué se activa `CLIENT.FOUND_ROWS` en `api_mapa/database.py`?
En PyMySQL/MariaDB, por defecto `cursor.rowcount` tras un `UPDATE` devuelve las filas **modificadas** en disco. Si haces un `PUT` idempotente enviando exactamente los mismos valores que ya tenía un registro, MariaDB no cambia bytes y devolvería `rowcount = 0`, provocando un falso `404`. Al pasar `client_flag=CLIENT.FOUND_ROWS` en [`database.py`](../api_mapa/database.py), `rowcount` devuelve las filas **encontradas** (`1`), haciendo que `PUT` sea idempotente como exige REST.

### ¿Por qué `front_flask` NO importa `pymysql` ni tiene variables de BD?
El frontend es una aplicación cliente desacoplada que solo consume la API vía HTTP (`httpx`). En [`docker-compose.yml`](../docker-compose.yml), el servicio `front-flask` únicamente recibe `API_BASE_URL=http://api-mapa:8117`. Si la API se detiene, Flask muestra `.aviso-desconexion` sin caerse con excepción no controlada.

### ¿Cómo funciona la seguridad v3 en API y en el Frontend?
1. El login (`POST /api/login`) valida contraseña contra `password_hash` con `bcrypt.checkpw` y firma un JWT (`HS256`) con `roles` y `exp` configurable.
2. En `front_flask`, el JWT **no se expone al navegador**: se guarda en memoria del servidor Flask (`SESIONES`) y al navegador solo se envía una cookie `HttpOnly` llamada `mapa_sid` ([`cliente_api.py`](../front_flask/servicios/cliente_api.py)).
3. En `api_mapa`, [`autorizacion/dependencias.py`](../api_mapa/autorizacion/dependencias.py) distingue claramente:
   - **401 Unauthorized:** falta el header `Authorization: Bearer ...` o el token es inválido/expirado.
   - **403 Forbidden:** el usuario sí está autenticado, pero su rol no está autorizado (por ejemplo, `Consulta` intentando `POST`/`DELETE`, o `Investigador` intentando entrar a `/api/usuario`).

### ¿Qué pasa si falla una FK en v2?
En [`servicio_v2.py`](../api_mapa/servicios/servicio_v2.py), antes de insertar o reemplazar `docente`, `producto`, `desarrolla` o cualquiera de las 7 tablas puente, el servicio consulta si los padres existen y siguen con `activo = 1`. Si el padre no existe o fue retirado lógicamente, se lanza `ConflictoExcepcion`, que [`main.py`](../api_mapa/main.py) traduce a **HTTP `409 Conflict`** — incluso antes de depender solo de la restricción física de MariaDB.

---

## 2. Recorrido Rápido para Sustentar un Archivo Cualquiera

1. **Si abren [`interfaces.py`](../api_mapa/repositorios/interfaces.py) o [`servicio_v1.py`](../api_mapa/servicios/servicio_v1.py):** explica cómo la interfaz `ABC` desacopla el servicio de `pymysql` y permite correr [`test_capas.py`](../api_mapa/pruebas/test_capas.py) sin MariaDB.
2. **Si abren [`ensamblador.py`](../api_mapa/ensamblador.py):** explica que es el *Composition Root* (patrón Inyección de Dependencias sin frameworks mágicos).
3. **Si abren [`app.py`](../front_flask/app.py):** explica las rutas explícitas de las 21 vistas, el `ThreadPoolExecutor` para catálogos foráneos en paralelo y el manejo de `401`/`403`/error de conexión.
4. **Si abren [`db/init.sql`](../db/init.sql):** señala las 21 tablas originales del caso + la columna `activo` para borrado lógico y los 17 ODS semilla.
