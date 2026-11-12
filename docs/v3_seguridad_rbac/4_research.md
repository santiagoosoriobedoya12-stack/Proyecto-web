# Investigación y decisiones — Versión 3: el control de acceso

> Las decisiones que se tomaron **y las alternativas que se descartaron, con su
> razón**. En esta versión importa más que en las otras, porque casi todas las
> alternativas «también funcionan» — y fallan de formas que no se ven.

---

## D1 — El hash: ¿SHA-256 o bcrypt?

**bcrypt**, y la razón es contraintuitiva.

| Opción | Argumento |
|---|---|
| **SHA-256** | Está en la librería estándar, no hace falta ningún paquete, y es criptográficamente sólido |
| **bcrypt** ✅ | **Es LENTO a propósito** |

**Lo que decide:** SHA-256 es **demasiado rápido**. Una tarjeta gráfica calcula
miles de millones por segundo, así que probar todas las contraseñas de ocho
caracteres es cuestión de horas. bcrypt se diseñó para ser lento **y con costo
ajustable**: el día que las máquinas sean más rápidas se sube el costo, sin
cambiar de función.

**Y bcrypt trae *salt* incorporado**, que es la otra mitad. El *salt* es un
valor aleatorio que se mezcla con la contraseña, y hace que **dos personas con
la misma clave tengan hashes distintos**:

```
los dos usuarios de carlos.castro comparten la contrasena, y sus hash no se parecen:
  $2a$12$f1UjnYhuaQUrCS8w/EARw...
  $2a$12$F7CLooKrzi/ec4U0iI9.le...
```

Sin *salt*, ver dos hashes iguales en la tabla delataría que esas dos personas
usan la misma contraseña.

> **¿Y Argon2?** Es técnicamente más resistente en memoria RAM, pero se mantuvo **BCrypt (costo 12)** para preservar compatibilidad exacta con los hashes `$2a$12$` ya sembrados en `db/bdfacturas.sql` y la librería estándar `bcrypt` en Python.

## D2 — El costo 12: ¿por qué ese número?

| | |
|---|---|
| **Qué significa** | `$2a$12$` — el `12` es el **costo**, y es un exponente: cada punto **duplica** el tiempo |
| **Por qué 12** | Tarda del orden de **un cuarto de segundo** en una máquina de escritorio. Imperceptible para quien inicia sesión una vez; carísimo para quien quiera probar millones |
| **Por qué no 4** | Sería rápido y **gratis de atacar** |
| **Por qué no 20** | Cada inicio de sesión tardaría **minutos** |

> **Y es ajustable sin cambiar de función, que es la gracia de bcrypt:** el día
> que 12 sea poco, se sube a 13 y los hashes viejos **siguen verificando** —el
> costo va escrito dentro del hash—.

## D3 — La sesión: ¿cookie de servidor o JWT?

| Opción | Argumento |
|---|---|
| **Cookie con sesión en el servidor** | **Se puede revocar**: se borra la sesión y listo. Es más seguro |
| **JWT** ✅ | No necesita que el servidor recuerde nada. **Y es lo que el curso enseña** |

**Lo que decide, y conviene ser honesto:** la cookie con estado en el servidor
es **mejor** para una aplicación como esta. El JWT se elige porque es el
mecanismo que el estudiante va a encontrar en todas partes, y porque **no poder
revocarlo es en sí una lección**: obliga a entender por qué la duración es
corta.

> **Lo que se pierde está escrito en el plan:** un token no se puede apagar. Si
> alguien se lo roba, sirve hasta que venza.

## D4 — Los permisos: ¿dentro del token, o se consultan?

**Se consultan.** Es la decisión más importante de la versión.

| Opción | Argumento |
|---|---|
| **En el token** | **Una consulta menos por petición.** El token ya trae todo: se lee y se decide |
| **Consultados** ✅ | **Quitar un permiso surte efecto de inmediato** |

**Lo que decide:**

```
09:00  Ana recibe un token que dice: puede entrar a interfaz.usuarios
09:30  se le quita ese permiso a su rol, en la base
09:31  Ana sigue entrando: su token todavia dice que puede
       ...hasta que venza, una hora despues
```

**Una hora de permiso que ya se le quitó.** Y el caso que importa no es Ana
olvidadiza: es alguien a quien se le retiró el acceso por una razón.

> **Es el criterio 7, y está escrito para forzar esta decisión:** *«quitarle un
> permiso a un rol surte efecto sin volver a identificarse»*. Con los permisos
> en el token, ese criterio **no se puede cumplir**.
>
> **Lo que cuesta:** una consulta a la base por operación. Es el precio, y es
> barato: el procedimiento son tres `JOIN` sobre tablas con índice.

## D5 — El permiso: ¿duplicar el `JOIN` en Python, o invocar el procedimiento?

**El procedimiento**, `verificar_acceso_ruta`, que **ya existía en la base
desde el primer día**.

| Opción | Argumento |
|---|---|
| **El `JOIN` en Python** | Se lee directo en el repositorio |
| **El procedimiento** ✅ | **La regla vive en un solo sitio** |

**Lo que decide:** repetir la decisión en código Python dejaría la lógica en dos
lugares. Para el listado visual del menú (`GET /api/permisos/mios`), `RepositorioAcceso` sí consulta las rutas del usuario, mientras que la **decisión real de autorización** la toma siempre `verificar_acceso_ruta`.

## D6 — El 403: ¿una dependencia `Depends`, o código repetido en cada función?

**Una dependencia declarativa**, `Depends(exige_permiso("..."))` en FastAPI.

| Opción | Argumento |
|---|---|
| **Copiar verificación en cada función** | Propenso a omisiones |
| **Dependencia FastAPI** ✅ | **Declarativa, reutilizable y visible en el contrato del router** |

## D7 — `leeway=0` al decodificar el JWT: ¿por qué?

Al verificar expiración en `pyjwt` (`jwt.decode(..., leeway=0)`), exigir tolerancia cero asegura que un token vencido sea rechazado inmediatamente con `401`, facilitando probar la caducidad exacta en sustentación sin márgenes artificiales.

## D8 — El token en el navegador: ¿`localStorage` o sesión en el servidor `front_python`?

**Sesión en servidor Python + cookie `HttpOnly`.** En `front_python/servicios/cliente_api.py`, el token JWT permanece en memoria del servidor (`SESIONES`) asociado al `factucorp_sid`.

| Opción | Argumento |
|---|---|
| **`localStorage`** | Expuesto a lectura por JavaScript (XSS) |
| **Sesión en servidor (`HttpOnly`)** ✅ | **El JWT nunca baja al navegador.** |

## D9 — Lo que NO se investigó, y por qué

| | |
|---|---|
| **Refrescar el token** | Trae su propio problema sin resolver: cómo se revoca el *refresh token*. Con una hora, volver a identificarse alcanza |
| **OAuth / OpenID Connect** | Delegar la identidad a Google o Microsoft es lo que se hace en producción, y **esconde exactamente lo que esta versión existe para enseñar** |
| **Permisos por operación** | La tabla `ruta` trae `permiso.crear` y `permiso.eliminar`, así que la base lo soportaría. Los diez criterios piden protección **por interfaz** |
| **Auditoría** (quién hizo qué) | Es un requisito real que el curso no plantea |
