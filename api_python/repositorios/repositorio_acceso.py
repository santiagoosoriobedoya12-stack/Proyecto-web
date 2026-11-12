# ==============================================================
# REPOSITORIO VERSIÓN 3: SEGURIDAD, LOGIN Y CONTROL DE ACCESO RBAC
# Implementa IRepositorioAcceso:
# - Consulta el hash de 'usuario' exclusivamente durante login.
# - Delega la decisión de permisos al SP 'verificar_acceso_ruta' en SQL Server.
# ==============================================================

import json
from database import consultar_fila, consultar_filas, obtener_conexion
from repositorios.interfaces import IRepositorioAcceso


class RepositorioAccesoSqlServer(IRepositorioAcceso):
    """Implementación SQL Server de IRepositorioAcceso."""

    def obtener_credenciales_por_email(self, email: str) -> dict | None:
        sql = "SELECT email, contrasena FROM usuario WHERE email = %s"
        return consultar_fila(sql, (email,))

    def obtener_roles_por_email(self, email: str) -> list[str]:
        sql = """
            SELECT r.nombre
            FROM rol r
            INNER JOIN rol_usuario ru ON r.id = ru.fkidrol
            WHERE ru.fkemail = %s
            ORDER BY r.id;
        """
        return [fila["nombre"] for fila in consultar_filas(sql, (email,))]

    def obtener_id_ruta_por_nombre(self, nombre_ruta: str) -> int | None:
        sql = "SELECT id FROM ruta WHERE ruta = %s"
        fila = consultar_fila(sql, (nombre_ruta,))
        return int(fila["id"]) if fila else None

    def verificar_acceso_ruta(self, email: str, id_ruta: int) -> bool:
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
                    return bool(json.loads(raw).get("tiene_acceso", False))
                except Exception:
                    return False

    def obtener_rutas_permitidas_por_email(self, email: str) -> list[str]:
        sql = """
            SELECT DISTINCT rt.ruta
            FROM ruta rt
            INNER JOIN rutarol rr ON rt.id = rr.fkidruta
            INNER JOIN rol_usuario ru ON rr.fkidrol = ru.fkidrol
            WHERE ru.fkemail = %s
            ORDER BY rt.ruta;
        """
        return [fila["ruta"] for fila in consultar_filas(sql, (email,))]
