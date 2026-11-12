# ==============================================================
# REPOSITORIOS VERSIÓN 3: ROLES, USUARIOS Y ACCESO JWT (MariaDB)
# - Rol y Usuario implementan borrado lógico (activo = 1 / 0).
# - RepositorioUsuarioMariaDB jamás devuelve 'password' en consultas CRUD.
# - RepositorioAccesoMariaDB consulta credenciales y roles activos para login/RBAC.
# ==============================================================

from database import (
    consultar_fila,
    consultar_filas,
    ejecutar_dml,
    ejecutar_insert_autoincrement,
    extraer_codigo_mysql,
)
from excepciones import ConflictoExcepcion
from repositorios.helpers_sql import actualizar_whitelist_logico, eliminar_logico, verificar_padre_activo
from repositorios.interfaces import (
    IRepositorioRol,
    IRepositorioUsuario,
    IRepositorioRolUsuario,
    IRepositorioAcceso,
)


class RepositorioRolMariaDB(IRepositorioRol):
    _PATCH = {"nombre", "descripcion"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, nombre, descripcion, fecha_creacion FROM rol WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_rol: int) -> dict | None:
        sql = "SELECT id, nombre, descripcion, fecha_creacion FROM rol WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_rol,))

    def crear(self, nombre: str, descripcion: str | None) -> int:
        sql = "INSERT INTO rol (nombre, descripcion, activo) VALUES (%s, %s, 1)"
        try:
            return ejecutar_insert_autoincrement(sql, (nombre, descripcion))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe un rol con nombre '{nombre}'.")
            raise

    def actualizar_parcial(self, id_rol: int, datos: dict) -> int:
        return actualizar_whitelist_logico("rol", "id", id_rol, datos, self._PATCH)

    def eliminar(self, id_rol: int) -> int:
        return eliminar_logico("rol", "id", id_rol)


class RepositorioUsuarioMariaDB(IRepositorioUsuario):
    _COLS = "id, username, email, nombre_completo, fecha_creacion, fecha_actualizacion"
    _PATCH = {"username", "password", "email", "nombre_completo"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = f"SELECT {self._COLS} FROM usuario WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_usuario: int) -> dict | None:
        sql = f"SELECT {self._COLS} FROM usuario WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_usuario,))

    def crear(self, username: str, password_hash: str, email: str, nombre_completo: str | None) -> int:
        sql = "INSERT INTO usuario (username, password, email, nombre_completo, activo) VALUES (%s, %s, %s, %s, 1)"
        try:
            return ejecutar_insert_autoincrement(sql, (username, password_hash, email, nombre_completo))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion("El username o el email ya se encuentra registrado.")
            raise

    def actualizar_parcial(self, id_usuario: int, datos: dict) -> int:
        return actualizar_whitelist_logico("usuario", "id", id_usuario, datos, self._PATCH)

    def eliminar(self, id_usuario: int) -> int:
        return eliminar_logico("usuario", "id", id_usuario)


class RepositorioRolUsuarioMariaDB(IRepositorioRolUsuario):
    """Puente M:N usuario <-> rol (en v3 no tiene columna 'activo' en esquema SQL)."""

    def obtener_todos(self) -> list[dict]:
        sql = (
            "SELECT ru.usuario_id, ru.rol_id, u.username, r.nombre AS nombre_rol "
            "FROM rol_usuario ru "
            "INNER JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1 "
            "INNER JOIN rol r ON r.id = ru.rol_id AND r.activo = 1 "
            "ORDER BY ru.usuario_id, ru.rol_id"
        )
        return consultar_filas(sql)

    def obtener_por_usuario(self, usuario_id: int) -> list[dict]:
        sql = (
            "SELECT ru.usuario_id, ru.rol_id, r.nombre AS nombre_rol "
            "FROM rol_usuario ru "
            "INNER JOIN rol r ON r.id = ru.rol_id AND r.activo = 1 "
            "WHERE ru.usuario_id = %s ORDER BY ru.rol_id"
        )
        return consultar_filas(sql, (usuario_id,))

    def obtener_por_rol(self, rol_id: int) -> list[dict]:
        sql = (
            "SELECT ru.usuario_id, ru.rol_id, u.username "
            "FROM rol_usuario ru "
            "INNER JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1 "
            "WHERE ru.rol_id = %s ORDER BY ru.usuario_id"
        )
        return consultar_filas(sql, (rol_id,))

    def crear(self, usuario_id: int, rol_id: int) -> None:
        verificar_padre_activo("usuario", "id", usuario_id, "usuario")
        verificar_padre_activo("rol", "id", rol_id, "rol")
        sql = "INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (%s, %s)"
        try:
            ejecutar_dml(sql, (usuario_id, rol_id))
        except Exception as ex:
            cod = extraer_codigo_mysql(ex)
            if cod == 1062:
                raise ConflictoExcepcion(f"El usuario {usuario_id} ya tiene asignado el rol {rol_id}.")
            if cod in (1451, 1452):
                raise ConflictoExcepcion("El usuario o rol especificado no existe.")
            raise

    def eliminar(self, usuario_id: int, rol_id: int) -> int:
        sql = "DELETE FROM rol_usuario WHERE usuario_id = %s AND rol_id = %s"
        return ejecutar_dml(sql, (usuario_id, rol_id))


class RepositorioAccesoMariaDB(IRepositorioAcceso):
    """Consulta usuario activo por username/email y sus roles para emitir/verificar RBAC."""

    def obtener_usuario_login(self, identificador: str) -> dict | None:
        sql = (
            "SELECT id, username, email, password, nombre_completo "
            "FROM usuario WHERE (username = %s OR email = %s) AND activo = 1"
        )
        return consultar_fila(sql, (identificador, identificador))

    def obtener_roles_usuario(self, usuario_id: int) -> list[str]:
        sql = (
            "SELECT r.nombre "
            "FROM rol r "
            "INNER JOIN rol_usuario ru ON r.id = ru.rol_id "
            "WHERE ru.usuario_id = %s AND r.activo = 1 "
            "ORDER BY r.id"
        )
        return [f["nombre"] for f in consultar_filas(sql, (usuario_id,))]
