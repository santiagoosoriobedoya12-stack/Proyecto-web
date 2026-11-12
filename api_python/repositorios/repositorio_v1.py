# ==============================================================
# REPOSITORIOS VERSIÓN 1: LAS 6 TABLAS INDEPENDIENTES (SIN FK)
# - Cada clase implementa explícitamente su interfaz ABC (IRepositorio*).
# - Todo el SQL está explícito y parametrizado con '%s' en cada repositorio.
# - Las actualizaciones parciales (PATCH) validan cada columna contra una lista blanca.
# - Errores 2627/2601 (duplicado) y 547 (padre con hijos) -> ConflictoExcepcion (409).
# - RepositorioUsuarioSqlServer jamás incluye la columna 'contrasena' en consultas CRUD.
# ==============================================================

from database import (
    consultar_filas,
    consultar_fila,
    ejecutar_dml,
    ejecutar_insert_identity,
    extraer_codigo_sql,
)
from excepciones import ConflictoExcepcion
from repositorios.interfaces import (
    IRepositorioProducto,
    IRepositorioEmpresa,
    IRepositorioPersona,
    IRepositorioRol,
    IRepositorioRuta,
    IRepositorioUsuario,
)


def ejecutar_update_whitelist(
    tabla: str,
    columna_pk: str,
    clave,
    datos: dict,
    columnas_permitidas: set[str],
) -> int:
    """
    Construye una sentencia UPDATE parametrizada verificando cada nombre de columna
    contra una lista blanca fija para impedir inyección SQL en nombres de campo.
    """
    if not datos:
        return 0
    clausulas_set = []
    valores = []
    for col, val in datos.items():
        col_lower = col.lower()
        if col_lower in columnas_permitidas:
            clausulas_set.append(f"{col_lower} = %s")
            valores.append(val)
    if not clausulas_set:
        return 0
    valores.append(clave)
    sql = f"UPDATE {tabla} SET {', '.join(clausulas_set)} WHERE {columna_pk} = %s"
    try:
        return ejecutar_dml(sql, tuple(valores))
    except Exception as ex:
        codigo = extraer_codigo_sql(ex)
        if codigo in (2627, 2601):
            raise ConflictoExcepcion(f"Ya existe un registro duplicado en '{tabla}'.")
        if codigo == 547:
            raise ConflictoExcepcion(f"Conflicto de integridad referencial en '{tabla}'.")
        raise


def ejecutar_delete_protegido(sql: str, clave, tabla: str) -> int:
    """Ejecuta DELETE parametrizado y traduce el error SQL 547 a ConflictoExcepcion (409)."""
    try:
        return ejecutar_dml(sql, (clave,))
    except Exception as ex:
        if extraer_codigo_sql(ex) == 547:
            raise ConflictoExcepcion(
                f"No se puede eliminar de '{tabla}' porque tiene registros asociados (clave foránea)."
            )
        raise


class RepositorioProductoSqlServer(IRepositorioProducto):
    """Implementación SQL Server explícita de IRepositorioProducto."""

    _PATCH_PERMITIDOS = {"nombre", "stock", "valorunitario"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) codigo, nombre, stock, valorunitario FROM producto ORDER BY codigo"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, codigo: str) -> dict | None:
        sql = "SELECT codigo, nombre, stock, valorunitario FROM producto WHERE codigo = %s"
        return consultar_fila(sql, (codigo,))

    def crear(self, codigo: str, nombre: str, stock: int, valorunitario: float) -> None:
        sql = "INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES (%s, %s, %s, %s)"
        try:
            ejecutar_dml(sql, (codigo, nombre, stock, valorunitario))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe un producto con codigo '{codigo}'.")
            raise

    def actualizar_parcial(self, codigo: str, datos: dict) -> int:
        return ejecutar_update_whitelist("producto", "codigo", codigo, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, codigo: str) -> int:
        sql = "DELETE FROM producto WHERE codigo = %s"
        return ejecutar_delete_protegido(sql, codigo, "producto")


class RepositorioEmpresaSqlServer(IRepositorioEmpresa):
    """Implementación SQL Server explícita de IRepositorioEmpresa."""

    _PATCH_PERMITIDOS = {"nombre"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) codigo, nombre FROM empresa ORDER BY codigo"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, codigo: str) -> dict | None:
        sql = "SELECT codigo, nombre FROM empresa WHERE codigo = %s"
        return consultar_fila(sql, (codigo,))

    def crear(self, codigo: str, nombre: str) -> None:
        sql = "INSERT INTO empresa (codigo, nombre) VALUES (%s, %s)"
        try:
            ejecutar_dml(sql, (codigo, nombre))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una empresa con codigo '{codigo}'.")
            raise

    def actualizar_parcial(self, codigo: str, datos: dict) -> int:
        return ejecutar_update_whitelist("empresa", "codigo", codigo, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, codigo: str) -> int:
        sql = "DELETE FROM empresa WHERE codigo = %s"
        return ejecutar_delete_protegido(sql, codigo, "empresa")


class RepositorioPersonaSqlServer(IRepositorioPersona):
    """Implementación SQL Server explícita de IRepositorioPersona."""

    _PATCH_PERMITIDOS = {"nombre", "email", "telefono"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) codigo, nombre, email, telefono FROM persona ORDER BY codigo"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, codigo: str) -> dict | None:
        sql = "SELECT codigo, nombre, email, telefono FROM persona WHERE codigo = %s"
        return consultar_fila(sql, (codigo,))

    def crear(self, codigo: str, nombre: str, email: str, telefono: str) -> None:
        sql = "INSERT INTO persona (codigo, nombre, email, telefono) VALUES (%s, %s, %s, %s)"
        try:
            ejecutar_dml(sql, (codigo, nombre, email, telefono))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una persona con codigo '{codigo}'.")
            raise

    def actualizar_parcial(self, codigo: str, datos: dict) -> int:
        return ejecutar_update_whitelist("persona", "codigo", codigo, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, codigo: str) -> int:
        sql = "DELETE FROM persona WHERE codigo = %s"
        return ejecutar_delete_protegido(sql, codigo, "persona")


class RepositorioRolSqlServer(IRepositorioRol):
    """Implementación SQL Server explícita de IRepositorioRol."""

    _PATCH_PERMITIDOS = {"nombre"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) id, nombre FROM rol ORDER BY id"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_rol: int) -> dict | None:
        sql = "SELECT id, nombre FROM rol WHERE id = %s"
        return consultar_fila(sql, (id_rol,))

    def crear(self, nombre: str) -> int:
        sql = "INSERT INTO rol (nombre) VALUES (%s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        return ejecutar_insert_identity(sql, (nombre,))

    def actualizar_parcial(self, id_rol: int, datos: dict) -> int:
        return ejecutar_update_whitelist("rol", "id", id_rol, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, id_rol: int) -> int:
        sql = "DELETE FROM rol WHERE id = %s"
        return ejecutar_delete_protegido(sql, id_rol, "rol")


class RepositorioRutaSqlServer(IRepositorioRuta):
    """Implementación SQL Server explícita de IRepositorioRuta."""

    _PATCH_PERMITIDOS = {"ruta", "descripcion"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) id, ruta, descripcion FROM ruta ORDER BY id"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_ruta: int) -> dict | None:
        sql = "SELECT id, ruta, descripcion FROM ruta WHERE id = %s"
        return consultar_fila(sql, (id_ruta,))

    def crear(self, ruta: str, descripcion: str) -> int:
        sql = "INSERT INTO ruta (ruta, descripcion) VALUES (%s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        try:
            return ejecutar_insert_identity(sql, (ruta, descripcion))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una ruta registrada con el patrón '{ruta}'.")
            raise

    def actualizar_parcial(self, id_ruta: int, datos: dict) -> int:
        return ejecutar_update_whitelist("ruta", "id", id_ruta, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, id_ruta: int) -> int:
        sql = "DELETE FROM ruta WHERE id = %s"
        return ejecutar_delete_protegido(sql, id_ruta, "ruta")


class RepositorioUsuarioSqlServer(IRepositorioUsuario):
    """
    Implementación SQL Server explícita de IRepositorioUsuario.
    REGLA v3: Nunca selecciona la columna 'contrasena' en operaciones de lectura CRUD.
    """

    _PATCH_PERMITIDOS = {"contrasena"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) email FROM usuario ORDER BY email"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, email: str) -> dict | None:
        sql = "SELECT email FROM usuario WHERE email = %s"
        return consultar_fila(sql, (email,))

    def crear(self, email: str, contrasena_hash: str) -> None:
        sql = "INSERT INTO usuario (email, contrasena) VALUES (%s, %s)"
        try:
            ejecutar_dml(sql, (email, contrasena_hash))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe un usuario con email '{email}'.")
            raise

    def actualizar_parcial(self, email: str, datos: dict) -> int:
        return ejecutar_update_whitelist("usuario", "email", email, datos, self._PATCH_PERMITIDOS)

    def eliminar(self, email: str) -> int:
        sql = "DELETE FROM usuario WHERE email = %s"
        return ejecutar_delete_protegido(sql, email, "usuario")
