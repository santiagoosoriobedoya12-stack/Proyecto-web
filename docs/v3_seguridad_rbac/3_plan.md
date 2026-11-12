# Plan técnico — Versión 3: el control de acceso (Python / FastAPI + Jinja2)

> | | |
> |---|---|
> | **Qué hay que construir** | [2_spec.md](2_spec.md) |
> | **Los formatos exactos** | [6_contracts.md](6_contracts.md) |
> | **El orden** | [8_tasks.md](8_tasks.md) |
> | **Los conceptos** | [4_research.md](4_research.md) |

---

## 1. Lo que se agrega a la pila Python

| Paquete | Función |
|---|---|
| **`pyjwt`** | Firma y verifica tokens JWT HS256 con `leeway=0` |
| **`bcrypt`** | Genera y valida hashes costo 12 (`$2a$12$` / `$2b$12$`) |

---

## 2. Las tres piezas, y por qué son tres

```text
   credenciales               token                   permiso
        │                       │                        │
   AUTENTICA ────────►       SESIÓN     ────────►    AUTORIZA
   ¿quién es?              (PyJWT HS256)             ¿puede esto?
        │                       │                        │
  ServicioSesion     exige_usuario_autenticado    exige_permiso("...")
    + BCrypt                (Depends)            + verificar_acceso_ruta
```

| | Archivo en `api_python/` | Qué resuelve |
|---|---|---|
| **1** | `servicios/servicio_sesion.py` | Compara el hash BCrypt y firma el JWT |
| **2** | `autorizacion/dependencias.py` → `exige_usuario_autenticado` | Valida el Bearer token (`401` si falta, expiró o fue alterado) |
| **3** | `autorizacion/dependencias.py` → `exige_permiso` | Consulta `verificar_acceso_ruta` en SQL Server (`403` si el rol no puede) |

---

## 3. Los archivos de seguridad

### 3.1 La sesión

```text
api_python/config.py                      variables JWT (Jwt__Key >= 32 chars, Issuer, Audience, ExpiracionMinutos)
api_python/peticiones/esquemas.py         SesionCrear { email, contrasena } en cuerpo JSON
api_python/servicios/servicio_sesion.py   valida BCrypt + construye JWT HS256 + devuelve rutas permitidas
api_python/controladores/controlador_sesion.py   POST /api/sesion (público) y GET /api/permisos/mios
```

| Decisión | Razón |
|---|---|
| **Credenciales en el cuerpo JSON** | Evita que correos o contraseñas queden en URLs, historial del navegador o logs de proxy |
| **Mismo mensaje 401 en fallo** | Correo inexistente y clave errada devuelven exactamente `"El correo o la contrasena no son correctos."` para evitar enumeración |

### 3.2 El permiso en FastAPI

```text
api_python/repositorios/repositorio_acceso.py   invoca verificar_acceso_ruta vía PyMSSQL
api_python/autorizacion/dependencias.py         dependencias exige_usuario_autenticado y exige_permiso
```

Ejemplo en router FastAPI:
```python
@router.get("/api/usuario", dependencies=[Depends(exige_permiso("interfaz.usuarios"))])
def listar_usuarios(): ...
```

| | Por qué así |
|---|---|
| **Dependencia declarativa `Depends`** | Visible en la firma del endpoint o router; difícil de olvidar al añadir rutas |
| **Se consulta en cada petición** | Garantiza revocación instantánea si se quita el permiso en SQL Server |
| **Fallo cerrado** | Si una ruta no está registrada en tabla `ruta`, responde `403` por defecto |

### 3.3 La interfaz gráfica en `front_python/`

```text
front_python/servicios/cliente_api.py      almacén SESIONES en memoria + cookie HttpOnly 'factucorp_sid'
front_python/templates/sesion.html         formulario de inicio de sesión
front_python/templates/base.html           barra de usuario activo + menú filtrado dinámicamente
front_python/app.py                        GET /sesion, POST /login, POST /cerrar-sesion
```

---

## 4. Decisiones clave del frontend Python

### 4.1 El token vive en el servidor, no en el navegador
En `front_python/servicios/cliente_api.py`, el diccionario `SESIONES` almacena el JWT en memoria del servidor backend web y solo entrega al navegador una cookie aleatoria `HttpOnly` (`factucorp_sid`), impidiendo que scripts JS lean el token vía XSS.

### 4.2 Inyección automática en `llamar_api()`
El helper `llamar_api(..., sesion=sesion)` adjunta `Authorization: Bearer <token>` a las llamadas `httpx` hacia `api-facturas` únicamente usando el token propio de la sesión correspondiente al navegador que hace la solicitud.

### 4.3 El menú no es la protección
`base.html` oculta enlaces no autorizados mediante `tiene_permiso(sesion, "interfaz.x")`, pero si el usuario escribe manualmente `/usuarios` en la barra de direcciones, `api-facturas` ejecuta `exige_permiso`, responde **403**, y `front_python` muestra el mensaje de permiso denegado con 0 filas.
