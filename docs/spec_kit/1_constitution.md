# Constitución Permanente — Módulo Mapa de Conocimiento

Reglas inmutables que rigen las versiones `v1`, `v2` y `v3`:

1. **Stack único 100% Python:** Backend **FastAPI + PyMySQL + PyJWT + BCrypt** (`api_mapa/`), Frontend **Flask + Jinja2 + httpx + Bootstrap local** (`front_flask/`) y Base de Datos **MariaDB 11.4** (`db/init.sql`).
2. **Separación estricta de capas:** `controladores/` → `servicios/` → `repositorios/interfaces.py` (`ABC`) → `repositorios/*_mariadb.py`. Ningún controlador escribe SQL y ningún servicio importa `FastAPI` o `pymysql`.
3. **Aislamiento del Frontend:** `front_flask/` nunca importa `pymysql` ni recibe credenciales de BD en [`docker-compose.yml`](../../docker-compose.yml).
4. **Retiro lógico obligatorio (`0_METODOLOGIA.md` §7.0):** Las 18 tablas del módulo más `rol` y `usuario` implementan `activo TINYINT(1) NOT NULL DEFAULT 1`. Ningún `SELECT` lista filas con `activo = 0` y ningún `DELETE` ejecuta borrado físico sobre ellas.
5. **Sin secretos en el repositorio:** Credenciales y `JWT_SECRET` se inyectan por variables de entorno (`.env.example` versionado; `.env` ignorado).
6. **Versionado acumulativo ($v1 \subset v2 \subset v3$):** Ninguna versión rompe rutas ni contratos de la versión anterior.
