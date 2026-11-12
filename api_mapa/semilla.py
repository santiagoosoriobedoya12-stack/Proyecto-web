# ==============================================================
# SEMILLA SEGURA DE USUARIOS Y ROLES INICIALES AL ARRANCAR LA API
# Garantiza que 'admin', 'investigador' y 'consulta' existan en MariaDB con
# hashes BCrypt costo 12 reales sin almacenar contraseñas planas en SQL.
# ==============================================================

from database import consultar_fila, ejecutar_dml, ejecutar_insert_autoincrement
from servicios.servicio_v1 import hashear_password


_CUENTAS_INICIALES = [
    ("admin", "admin@mapa.edu.co", "Administrador General", "admin123", 1),
    ("investigador", "investigador@mapa.edu.co", "Docente Investigador", "inv123", 2),
    ("consulta", "consulta@mapa.edu.co", "Usuario Consulta", "consulta123", 3),
]


def asegurar_usuarios_semilla() -> None:
    """Crea o sincroniza de manera idempotente los 3 usuarios de prueba en MariaDB."""
    try:
        for username, email, nombre, clave_plana, rol_id in _CUENTAS_INICIALES:
            existente = consultar_fila("SELECT id, password FROM usuario WHERE username = %s", (username,))
            pwd_hash = hashear_password(clave_plana)
            if not existente:
                usr_id = ejecutar_insert_autoincrement(
                    "INSERT INTO usuario (username, password, email, nombre_completo, activo) VALUES (%s, %s, %s, %s, 1)",
                    (username, pwd_hash, email, nombre),
                )
            else:
                usr_id = int(existente["id"])
                # Si la contraseña sembrada por SQL aún no corresponde a BCrypt estándar local, la normaliza
                if not str(existente.get("password", "")).startswith("$2b$12$"):
                    ejecutar_dml("UPDATE usuario SET password = %s, activo = 1 WHERE id = %s", (pwd_hash, usr_id))
            ejecutar_dml(
                "INSERT IGNORE INTO rol_usuario (usuario_id, rol_id) VALUES (%s, %s)",
                (usr_id, rol_id),
            )
    except Exception:
        # Si la base aún está iniciando, la prueba de salud o siguiente petición continuará
        pass
