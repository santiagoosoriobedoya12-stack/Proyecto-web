# ==============================================================
# REPOSITORIO VERSIÓN 3: SEGURIDAD, HASH DE LOGIN Y RBAC
# Reglas críticas de la v3:
# - Para iniciar sesión, consulta 'contrasena' SOLO aquí (nunca en GET /api/usuario).
# - Para validar permisos, invoca el Stored Procedure 'verificar_acceso_ruta'
#   (Criterio 6: NO se arma el JOIN de permisos en código Python).
# ==============================================================

import json
from database import obtener_conexion


class RepositorioAcceso:
    """Repositorio especializado en autenticación y verificación dinámica RBAC en SQL Server."""

    def obtener_credenciales_por_email(self, email: str) -> dict | None:
        """
        Obtiene email y hash BCrypt exclusivamente durante el proceso interno de login.
        Retorna None si el correo no existe.
        """
        sql = "SELECT email, contrasena FROM usuario WHERE email = %s"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (email,))
                return cur.fetchone()

    def obtener_roles_por_email(self, email: str) -> list[str]:
        """Obtiene los nombres de roles asignados a un usuario para incluirlos como claims en el JWT."""
        sql = """
            SELECT r.nombre
            FROM rol r
            INNER JOIN rol_usuario ru ON r.id = ru.fkidrol
            WHERE ru.fkemail = %s
            ORDER BY r.id;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (email,))
                return [fila["nombre"] for fila in cur.fetchall()]

    def obtener_id_ruta_por_nombre(self, nombre_ruta: str) -> int | None:
        """Traduce el nombre lógico (ej. 'interfaz.usuarios') a su ID numérico en la tabla 'ruta'."""
        sql = "SELECT id FROM ruta WHERE ruta = %s"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (nombre_ruta,))
                fila = cur.fetchone()
                return int(fila["id"]) if fila else None

    def verificar_acceso_ruta(self, email: str, id_ruta: int) -> bool:
        """
        Criterio 6 y 7 de la v3:
        Invoca el Stored Procedure 'verificar_acceso_ruta' en cada petición.
        Cruza usuario -> rol_usuario -> rutarol dentro de SQL Server y devuelve True/False.
        """
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC verificar_acceso_ruta @p_email = %s, @p_fkidruta = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (email, id_ruta))
                fila = cur.fetchone()
                raw = fila["resultado"] if fila else ""
                if not raw:
                    return False
                try:
                    data = json.loads(raw)
                    return bool(data.get("tiene_acceso", False))
                except Exception:
                    return False

    def obtener_rutas_permitidas_por_email(self, email: str) -> list[str]:
        """
        Consulta las rutas autorizadas del usuario para GET /api/permisos/mios.
        Se usa exclusivamente en el frontend para armar el menú visual (Criterio 8).
        """
        sql = """
            SELECT DISTINCT rt.ruta
            FROM ruta rt
            INNER JOIN rutarol rr ON rt.id = rr.fkidruta
            INNER JOIN rol_usuario ru ON rr.fkidrol = ru.fkidrol
            WHERE ru.fkemail = %s
            ORDER BY rt.ruta;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (email,))
                return [fila["ruta"] for fila in cur.fetchall()]
