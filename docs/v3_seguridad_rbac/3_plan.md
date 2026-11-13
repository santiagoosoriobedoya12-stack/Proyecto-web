# Plan de Arquitectura — Versión 3

Plan de diseño del subsistema de seguridad transversal:

1. **Capa Criptográfica y Dependencias:**
   - Librería `bcrypt` para hash y verificación de contraseñas.
   - Librería `PyJWT` para codificación y decodificación de tokens `HS256`.
2. **Dependencias de Autorización en FastAPI:**
   - `exige_usuario_autenticado`: extrae cabecera Bearer, valida firma con `JWT_SECRET` y consulta roles vigentes en MariaDB (401 si falla).
   - `exige_roles(*roles)`: comprueba intersección entre roles del usuario y roles permitidos (403 si falla).
3. **Módulo de Semilla Automática:**
   - `api_mapa/semilla.py` conectado al `lifespan` de FastAPI en `main.py`.
4. **Almacenamiento de Sesión en Flask:**
   - Módulo `front_flask/servicios/cliente_api.py` administra `SESIONES[sid]` y emite la cookie `mapa_sid` (`HttpOnly=True, SameSite="Lax"`).
