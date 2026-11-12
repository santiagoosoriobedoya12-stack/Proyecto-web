# Tareas — Versión 3: el control de acceso, por fases (Python / FastAPI + Jinja2)

> | | |
> |---|---|
> | **Qué** | [2_spec.md](2_spec.md) |
> | **Cómo** | [3_plan.md](3_plan.md) · [6_contracts.md](6_contracts.md) |
> | **La verificación** | [7_quickstart.md](7_quickstart.md) |

**El orden de esta versión no se puede cambiar**, y es lo primero que hay que
entender:

```text
   1. la CONTRASEÑA     sin esto, lo demás es decoración
         ↓
   2. la SESIÓN         sin esto, no hay a quién preguntarle nada
         ↓
   3. el PERMISO        sin esto, el sistema sabe quién entra y le deja todo
         ↓
   4. la INTERFAZ       sin esto, nadie puede usarlo
```

---

## Fase 0 — El punto de partida, verificado

| | |
|---|---|
| **Qué** | Que la v2 esté **funcionando**: los 12 recursos y sus vistas en `front_python/` |
| **Verificación** | `docker compose up -d --build` y las pantallas de v1 y v2 operan sin errores |

## Fase 1 — La semilla: las contraseñas con hash

| | |
|---|---|
| **Archivos** | `db/bdfacturas.sql` · `api_python/servicios/servicio_v1.py` · `api_python/servicios/servicio_v2.py` |
| **Lo que importa** | Hashes BCrypt costo 12 (`$2a$12$` / `$2b$12$`), más hash automático en Python al crear o editar usuarios, y **contraseñas en claro documentadas en `7_quickstart.md`** |
| **Verificación** | `SELECT email, contrasena FROM usuario;` muestra hashes BCrypt costo 12; crear usuario nuevo guarda hash y nunca texto plano |

## Fase 2 — La configuración del token JWT

| | |
|---|---|
| **Archivos** | `api_python/requirements.txt` (`pyjwt`, `bcrypt`) · `api_python/config.py` · `docker-compose.yml` |
| **Lo que importa** | `JWT_KEY` con **32 caracteres o más**; validar configuración en `config.py` al arrancar (`Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiracionMinutos`) |
| **Verificación** | `docker compose up` arranca con clave válida; sin clave suficiente falla al inicio y no con 500 en runtime |

## Fase 3 — Iniciar sesión (`POST /api/sesion`)

| | |
|---|---|
| **Archivos** | `api_python/peticiones/esquemas.py` (`SesionCrear`) · `api_python/servicios/servicio_sesion.py` · `api_python/routers/router_sesion.py` |
| **Lo que importa** | Las credenciales van **en el cuerpo JSON**, nunca en la URL. Y responde **el mismo mensaje 401** para correo inexistente y contraseña incorrecta |
| **Verificación** | Credenciales correctas → **200** con `{ token, expiraEn, email, roles }` · correo inexistente o clave mala → **401 con el mismo mensaje** |

## Fase 4 — El token exigido: el 401

| | |
|---|---|
| **Archivos** | `api_python/autorizacion/dependencias.py` (`exige_usuario_autenticado`) · `api_python/main.py` (`manejar_401`) |
| **Lo que importa** | Validación HS256 con `pyjwt` (`leeway=0`, verificación de firma, emisor, audiencia y expiración); `GET /` y `POST /api/sesion` **abiertos** |
| **Verificación** | Sin token **401** · con token **200** · token alterado → **401** · `GET /` → **200** |

## Fase 5 — El permiso por base de datos: el 403

| | |
|---|---|
| **Archivos** | `api_python/repositorios/repositorio_acceso.py` · `api_python/autorizacion/dependencias.py` (`exige_permiso`) · `routers/router_v1.py`, `router_v2.py` |
| **Lo que importa** | Ejecutar **`verificar_acceso_ruta`** en SQL Server mediante PyMSSQL en cada petición: el `JOIN` de permisos **no se duplica en Python**. Si la ruta no existe en tabla `ruta`, **falla cerrado** (`403`) |
| **Verificación** | Matriz de roles: `admin` todo `200` · `vendedor1` `403` en usuarios y `200` en clientes · `cliente1` al revés. Quitar permiso en BD afecta inmediatamente **sin volver a loguearse** |

## Fase 6 — Los permisos propios, para el menú

| | |
|---|---|
| **Archivos** | `api_python/routers/router_sesion.py` (`GET /api/permisos/mios`) |
| **Lo que importa** | El correo se extrae exclusivamente del token validado (`exige_usuario_autenticado`), nunca de un parámetro en la URL |
| **Verificación** | `admin` recibe las rutas administrativas · `cliente1` recibe solo `["interfaz.inicio", "interfaz.productos"]` |

## Fase 7 — La interfaz de identificación en el front Python

| | |
|---|---|
| **Archivos** | `front_python/servicios/cliente_api.py` (`SESIONES`, cookie `HttpOnly` `factucorp_sid`) · `front_python/templates/sesion.html` · `front_python/templates/base.html` · `front_python/main.py` |
| **Lo que importa** | **El token vive en el servidor Python** (diccionario en memoria indexado por `sid` en cookie `HttpOnly`), nunca en `localStorage` ni visible en el DOM |
| **Verificación** | Iniciar sesión como `admin@correo.com` muestra correo, roles y botón de salida; cerrar sesión invalida la cookie y borra la sesión del servidor |

## Fase 8 — El token en cada petición

| | |
|---|---|
| **Archivos** | `front_python/servicios/cliente_api.py` (`llamar_api`) |
| **Lo que importa** | `llamar_api` lee `sesion["token"]` de esa sesión puntual e inyecta `Authorization: Bearer <token>` en cada llamada `httpx` |
| **Verificación** | Dos navegadores/sesiones simultáneas (`admin` y `vendedor1`) operan de forma completamente aislada sin mezclar tokens |

## Fase 9 — El menú dinámico por rol

| | |
|---|---|
| **Archivos** | `front_python/templates/base.html` · `front_python/main.py` |
| **Lo que importa** | Tras autenticar, `front_python` consulta `GET /api/permisos/mios` y filtra los enlaces visibles de navegación usando `tiene_permiso(sesion, ...)` |
| **Verificación** | Entrar como `vendedor1`: el menú muestra Clientes, Vendedores y Facturas, ocultando Usuarios y Productos; entrar como `cliente1` invierte la visibilidad |

## Fase 10 — El cierre de v3

| | |
|---|---|
| **1** | **Regresión**: probar flujos de v1 y v2 enviando token JWT válido |
| **2** | Verificar los criterios de aceptación de [2_spec.md](2_spec.md) |
| **3** | **Seguridad real en URL:** autenticado como `vendedor1`, navegar manualmente a `/usuarios` en el navegador → el backend responde `403`, el front muestra alerta de permiso denegado y cero filas |
| **4** | Confirmar que `front_python` no expone el JWT en HTML ni cookies |
