# Bitácora de Prompts — Versión 3: El Control de Acceso (Seguridad, JWT y RBAC en Python)

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para implementar en **Python (FastAPI + Jinja2)** la autenticación con JWT, el hash de contraseñas con BCrypt, las dependencias de autorización RBAC con Stored Procedures y la gestión de sesiones en servidor.

---

## 1. Prompt de Semilla y Criptografía (Fases 1 y 2)
```text
Actúa como un experto en ciberseguridad y backend Python con FastAPI.
1. Revisa el script SQL db/bdfacturas.sql verificando las cuentas de prueba con hash BCrypt ($2a$12$) y garantiza en api_python/servicios/servicio_v1.py y servicio_v2.py que toda creación o actualización de contraseña aplique bcrypt.hashpw(..., bcrypt.gensalt(rounds=12)).
2. Mantén las credenciales documentadas para pruebas en 7_quickstart.md:
   - admin@correo.com: admin123
   - vendedor1@correo.com: vendedor123
   - cliente1@correo.com: cliente123
3. En api_python/requirements.txt agrega:
   - pyjwt
   - bcrypt
4. Configura en api_python/config.py y docker-compose.yml la clave secreta Jwt__Key de 32+ caracteres, Issuer, Audience y ExpiracionMinutos.
```

---

## 2. Prompt del Módulo de Autenticación y Token (Fases 3 y 4)
```text
Implementa el módulo de autenticación de la Versión 3 en FastAPI:
1. En api_python/peticiones/esquemas.py agrega SesionCrear ({ email, contrasena }).
2. Crea api_python/servicios/servicio_sesion.py:
   - Consulta el usuario y sus roles mediante RepositorioAcceso.
   - Comprueba la contraseña contra el hash guardado usando bcrypt.checkpw().
   - Genera el token JWT HS256 con pyjwt incluyendo email (sub), roles y tiempos iat/exp sin incluir rutas de permisos.
3. Crea api_python/controladores/controlador_sesion.py con POST /api/sesion:
   - Endpoint público (sin dependencia de token).
   - Si las credenciales son incorrectas (email inexistente o clave errada), responde exactamente el MISMO HTTP 401:
     { "estado": 401, "mensaje": "El correo o la contrasena no son correctos." }
4. En api_python/autorizacion/dependencias.py implementa exige_usuario_autenticado():
   - Extrae Bearer token, valida firma/issuer/audience con leeway=0.
   - Si falta o es inválido, lanza NoAutenticadoExcepcion mapeada en main.py a HTTP 401:
     { "estado": 401, "mensaje": "No hay una sesion valida. Inicie sesion." }
```

---

## 3. Prompt de Autorización RBAC Dinámica (Fases 5 y 6)
```text
Implementa el control de acceso basado en roles mediante Stored Procedure en FastAPI:
1. Crea api_python/repositorios/repositorio_acceso.py:
   - Ejecuta el procedimiento almacenado 'verificar_acceso_ruta' en SQL Server vía PyMSSQL.
   - Traduce el nombre de ruta ('interfaz.usuarios', etc.) a su ID numérico en la tabla 'ruta'.
   - Si la ruta no existe en BD, falla cerrado (tiene_acceso = false).
2. En api_python/autorizacion/dependencias.py crea la fábrica de dependencias exige_permiso(nombre_ruta: str):
   - Depende de exige_usuario_autenticado y consulta a SQL Server en cada petición (para que revocar un rol aplique al instante sin esperar expiración del JWT).
   - Si no tiene permiso, lanza SinPermisoExcepcion → HTTP 403 Forbidden:
     { "estado": 403, "mensaje": "Su rol no tiene permiso para esta operacion.", "ruta": "interfaz.x" }
3. Aplica Depends(exige_permiso("interfaz.x")) en todos los controladores protegidos (controlador_v1.py, controlador_v2.py).
4. En api_python/controladores/controlador_sesion.py agrega GET /api/permisos/mios:
   - Toma el email del token validado (no de la URL) y devuelve las rutas autorizadas para armar el menú del frontend.
```

---

## 4. Prompt de Frontend Python con Flask + Jinja2 (Fases 7, 8 y 9)
```text
Implementa la gestión de sesiones y menús dinámicos en front_python/ (Flask + Jinja2):
1. En front_python/servicios/cliente_api.py implementa un almacén de sesiones en memoria del servidor (SESIONES) vinculado únicamente mediante cookie HttpOnly ('factucorp_sid') para no exponer el JWT al navegador ni al DOM.
2. Implementa POST /login y POST /cerrar-sesion en front_python/app.py consumiendo POST /api/sesion y GET /api/permisos/mios.
3. En front_python/templates/sesion.html diseña la vista de inicio de sesión con alertas claras y en base.html muestra usuario conectado, roles y botón de cierre.
4. En cliente_api.py, inyecta la cabecera 'Authorization: Bearer <token>' en cada llamada httpx y maneja respuestas 401 (sesión expirada), 403 (sin permiso), 409 (conflicto de BD) y 422 (validación).
5. En front_python/templates/base.html renderiza dinámicamente las secciones de menú usando tiene_permiso(sesion, 'interfaz.x').
```
