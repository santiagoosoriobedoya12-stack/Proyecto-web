# ==============================================================
# REPOSITORIOS VERSIÓN 2: LAS 6 TABLAS DEPENDIENTES + USUARIO CON ROLES
# - Cada clase implementa explícitamente su contrato abstracto IRepositorio*.
# - Error SQL 547 / 2627 / 2601 -> ConflictoExcepcion (HTTP 409).
# - 'factura', 'rutarol' y 'usuario-con-roles' operan mediante Stored Procedures oficiales.
# ==============================================================

from database import (
    consultar_filas,
    consultar_fila,
    ejecutar_dml,
    ejecutar_insert_identity,
    ejecutar_sp_json,
    extraer_codigo_sql,
)
from excepciones import ConflictoExcepcion, NoEncontradoExcepcion
from repositorios.interfaces import (
    IRepositorioCliente,
    IRepositorioVendedor,
    IRepositorioFactura,
    IRepositorioRolUsuario,
    IRepositorioRutaRol,
    IRepositorioUsuarioConRoles,
)
from repositorios.repositorio_v1 import ejecutar_update_whitelist, ejecutar_delete_protegido


class RepositorioClienteSqlServer(IRepositorioCliente):
    """Implementación SQL Server de IRepositorioCliente (FKs a persona y empresa)."""

    _PATCH_PERMITIDOS = {"credito", "fkcodpersona", "fkcodempresa"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) id, credito, fkcodpersona, fkcodempresa FROM cliente ORDER BY id"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_cliente: int) -> dict | None:
        sql = "SELECT id, credito, fkcodpersona, fkcodempresa FROM cliente WHERE id = %s"
        return consultar_fila(sql, (id_cliente,))

    def crear(self, credito: float, fkcodpersona: str, fkcodempresa: str | None) -> int:
        sql = (
            "INSERT INTO cliente (credito, fkcodpersona, fkcodempresa) "
            "VALUES (%s, %s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        )
        try:
            return ejecutar_insert_identity(sql, (credito, fkcodpersona, fkcodempresa))
        except Exception as ex:
            if extraer_codigo_sql(ex) == 547:
                raise ConflictoExcepcion("La persona o empresa referenciada no existe en la base de datos.")
            raise

    def actualizar_parcial(self, id_cliente: int, datos: dict) -> int:
        try:
            return ejecutar_update_whitelist("cliente", "id", id_cliente, datos, self._PATCH_PERMITIDOS)
        except ConflictoExcepcion:
            raise ConflictoExcepcion("La persona o empresa referenciada no existe en la base de datos.")

    def eliminar(self, id_cliente: int) -> int:
        sql = "DELETE FROM cliente WHERE id = %s"
        return ejecutar_delete_protegido(sql, id_cliente, "cliente")


class RepositorioVendedorSqlServer(IRepositorioVendedor):
    """Implementación SQL Server de IRepositorioVendedor (FK obligatoria a persona)."""

    _PATCH_PERMITIDOS = {"carnet", "direccion", "fkcodpersona"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT TOP (%s) id, carnet, direccion, fkcodpersona FROM vendedor ORDER BY id"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_vendedor: int) -> dict | None:
        sql = "SELECT id, carnet, direccion, fkcodpersona FROM vendedor WHERE id = %s"
        return consultar_fila(sql, (id_vendedor,))

    def crear(self, carnet: int, direccion: str, fkcodpersona: str) -> int:
        sql = (
            "INSERT INTO vendedor (carnet, direccion, fkcodpersona) "
            "VALUES (%s, %s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        )
        try:
            return ejecutar_insert_identity(sql, (carnet, direccion, fkcodpersona))
        except Exception as ex:
            if extraer_codigo_sql(ex) == 547:
                raise ConflictoExcepcion(f"La persona '{fkcodpersona}' no existe en la base de datos.")
            raise

    def actualizar_parcial(self, id_vendedor: int, datos: dict) -> int:
        try:
            return ejecutar_update_whitelist("vendedor", "id", id_vendedor, datos, self._PATCH_PERMITIDOS)
        except ConflictoExcepcion:
            raise ConflictoExcepcion("La persona referenciada no existe en la base de datos.")

    def eliminar(self, id_vendedor: int) -> int:
        sql = "DELETE FROM vendedor WHERE id = %s"
        return ejecutar_delete_protegido(sql, id_vendedor, "vendedor")


class RepositorioFacturaSqlServer(IRepositorioFactura):
    """
    Implementación SQL Server Maestro-Detalle de IRepositorioFactura.
    Opera 100% por Stored Procedures; SQL Server calcula totales y stock en Triggers.
    """

    def listar(self) -> list[dict]:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_listar_facturas_y_productosporfactura @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        res = ejecutar_sp_json(sql)
        return res if isinstance(res, list) else []

    def obtener_por_numero(self, numero: int) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_consultar_factura_y_productosporfactura @p_numero = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (numero,))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo == 50003 or "no existe" in str(ex).lower():
                raise NoEncontradoExcepcion(f"No existe la factura número {numero}.")
            raise

    def crear(self, fkidcliente: int, fkidvendedor: int, productos_json: str) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_insertar_factura_y_productosporfactura
                @p_fkidcliente = %s,
                @p_fkidvendedor = %s,
                @p_productos = %s,
                @p_minimo_detalle = 1,
                @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (fkidcliente, fkidvendedor, productos_json))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo in (547, 50001, 50002, 50010) or "stock" in msg.lower() or "no existe" in msg.lower():
                raise ConflictoExcepcion(msg)
            raise

    def anular(self, numero: int) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_anular_factura @p_numero = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (numero,))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if "ya está anulada" in msg.lower() or "ya esta anulada" in msg.lower():
                raise ConflictoExcepcion(f"La factura {numero} ya se encuentra anulada.")
            if codigo == 50010 or "no existe" in msg.lower():
                raise NoEncontradoExcepcion(f"No existe la factura número {numero}.")
            raise


class RepositorioRolUsuarioSqlServer(IRepositorioRolUsuario):
    """Implementación SQL Server de la tabla puente M:N 'rol_usuario'."""

    def obtener_todos(self) -> list[dict]:
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            ORDER BY ru.fkemail, ru.fkidrol;
        """
        return consultar_filas(sql)

    def obtener_por_usuario(self, email: str) -> list[dict]:
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            WHERE ru.fkemail = %s
            ORDER BY ru.fkidrol;
        """
        return consultar_filas(sql, (email,))

    def obtener_por_rol(self, idrol: int) -> list[dict]:
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            WHERE ru.fkidrol = %s
            ORDER BY ru.fkemail;
        """
        return consultar_filas(sql, (idrol,))

    def crear(self, fkemail: str, fkidrol: int) -> None:
        sql = "INSERT INTO rol_usuario (fkemail, fkidrol) VALUES (%s, %s)"
        try:
            ejecutar_dml(sql, (fkemail, fkidrol))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo in (2627, 2601):
                raise ConflictoExcepcion(f"El usuario '{fkemail}' ya tiene asignado el rol {fkidrol}.")
            if codigo == 547:
                raise ConflictoExcepcion("El usuario o el rol especificado no existe.")
            raise

    def eliminar(self, fkemail: str, fkidrol: int) -> int:
        sql = "DELETE FROM rol_usuario WHERE fkemail = %s AND fkidrol = %s"
        return ejecutar_dml(sql, (fkemail, fkidrol))


class RepositorioRutaRolSqlServer(IRepositorioRutaRol):
    """Implementación SQL Server de IRepositorioRutaRol usando sus 3 Stored Procedures."""

    def listar(self) -> list[dict]:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC listar_rutarol @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        res = ejecutar_sp_json(sql)
        return res if isinstance(res, list) else []

    def crear(self, fkidruta: int, fkidrol: int) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC crear_rutarol @p_fkidruta = %s, @p_fkidrol = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        res = ejecutar_sp_json(sql, (fkidruta, fkidrol))
        if not res.get("success", False):
            raise ConflictoExcepcion(res.get("message", "No fue posible crear el permiso."))
        return res

    def eliminar(self, fkidruta: int, fkidrol: int) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC eliminar_rutarol @p_fkidruta = %s, @p_fkidrol = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        res = ejecutar_sp_json(sql, (fkidruta, fkidrol))
        if not res.get("success", False):
            raise NoEncontradoExcepcion(res.get("message", "El permiso no existe."))
        return res


class RepositorioUsuarioConRolesSqlServer(IRepositorioUsuarioConRoles):
    """Implementación SQL Server de IRepositorioUsuarioConRoles (5 Stored Procedures con OPENJSON)."""

    def listar(self) -> list[dict]:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC listar_usuarios_con_roles @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        res = ejecutar_sp_json(sql)
        return res if isinstance(res, list) else []

    def consultar(self, email: str) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC consultar_usuario_con_roles @p_email = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (email,))
        except Exception as ex:
            if extraer_codigo_sql(ex) == 50008 or "no existe" in str(ex).lower():
                raise NoEncontradoExcepcion(f"El usuario '{email}' no existe.")
            raise

    def crear(self, email: str, contrasena_hash: str, roles_json: str) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC crear_usuario_con_roles
                @p_email = %s,
                @p_contrasena = %s,
                @p_roles_json = %s,
                @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (email, contrasena_hash, roles_json))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo in (2627, 2601) or "ya existe" in msg.lower():
                raise ConflictoExcepcion(f"El usuario '{email}' ya existe en el sistema.")
            if codigo == 547 or "no existe" in msg.lower():
                raise ConflictoExcepcion("Uno de los roles especificados no existe en la base de datos.")
            raise

    def actualizar(self, email: str, contrasena_hash: str | None, roles_json: str) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC actualizar_usuario_con_roles
                @p_email = %s,
                @p_contrasena = %s,
                @p_roles = %s,
                @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (email, contrasena_hash, roles_json))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo == 50007 or ("usuario" in msg.lower() and "no existe" in msg.lower()):
                raise NoEncontradoExcepcion(f"El usuario '{email}' no existe.")
            if codigo == 547:
                raise ConflictoExcepcion("Uno de los roles especificados no existe.")
            raise

    def eliminar(self, email: str) -> dict:
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC eliminar_usuario_con_roles @p_email = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            return ejecutar_sp_json(sql, (email,))
        except Exception as ex:
            if extraer_codigo_sql(ex) == 50006 or "no existe" in str(ex).lower():
                raise NoEncontradoExcepcion(f"Usuario '{email}' no existe.")
            raise
