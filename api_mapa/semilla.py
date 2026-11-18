# ==============================================================
# SEMILLA SEGURA DE USUARIOS Y ROLES INICIALES AL ARRANCAR LA API
# Garantiza que 'admin', 'investigador' y 'consulta' existan en MariaDB con
# hashes BCrypt costo 12 reales sin almacenar contraseñas planas en SQL.
# ==============================================================

import os
from database import consultar_fila, ejecutar_dml, ejecutar_insert_autoincrement, obtener_conexion
from servicios.servicio_v1 import hashear_password


def asegurar_esquema_inicial() -> None:
    """Si la base de datos es PostgreSQL y está vacía, ejecuta db_init.sql automáticamente."""
    from config import DB_ENGINE
    if DB_ENGINE != "postgres":
        return
    try:
        fila = consultar_fila(
            "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'area_conocimiento'"
        )
        if not fila:
            ruta_sql = os.path.join(os.path.dirname(__file__), "db_init.sql")
            if os.path.exists(ruta_sql):
                with open(ruta_sql, "r", encoding="utf-8") as f:
                    contenido_sql = f.read()
                with obtener_conexion() as conn:
                    with conn.cursor() as cur:
                        cur.execute(contenido_sql)
    except Exception:
        pass


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
            if not consultar_fila("SELECT 1 FROM rol_usuario WHERE usuario_id = %s AND rol_id = %s", (usr_id, rol_id)):
                ejecutar_dml(
                    "INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (%s, %s)",
                    (usr_id, rol_id),
                )
    except Exception:
        # Si la base aún está iniciando, la prueba de salud o siguiente petición continuará
        pass
