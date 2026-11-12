# ==============================================================
# REPOSITORIOS VERSIÓN 2: LAS 6 TABLAS DEPENDIENTES + USUARIO CON ROLES
# Reglas clave:
# - Error SQL 547 (FK rota o padre con hijos) -> ConflictoExcepcion (HTTP 409)
# - Error SQL 2627/2601 (pareja/clave duplicada) -> ConflictoExcepcion (HTTP 409)
# - 'factura' opera 100% mediante sus Stored Procedures oficiales.
# - 'rutarol' y 'usuario-con-roles' operan mediante sus Stored Procedures con OPENJSON.
# ==============================================================

import json
from database import obtener_conexion, extraer_codigo_sql, normalizar_fila, normalizar_filas
from excepciones import ConflictoExcepcion, NoEncontradoExcepcion, ValidacionNegocioExcepcion
from repositorios.repositorio_v1 import RepositorioTablaBase


class RepositorioCliente(RepositorioTablaBase):
    """Repositorio SQL Server para 'cliente' (PK IDENTITY 'id', FKs a persona y empresa)."""
    tabla = "cliente"
    columna_pk = "id"
    columnas_select = "id, credito, fkcodpersona, fkcodempresa"
    columnas_patch = {"credito", "fkcodpersona", "fkcodempresa"}

    def crear(self, credito: float, fkcodpersona: str, fkcodempresa: str | None) -> int:
        """Inserta cliente; si fkcodpersona o fkcodempresa no existen (SQL 547), lanza 409."""
        sql = (
            "INSERT INTO cliente (credito, fkcodpersona, fkcodempresa) "
            "VALUES (%s, %s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        )
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (credito, fkcodpersona, fkcodempresa))
                    fila = cur.fetchone()
                    return int(fila["nuevo_id"])
        except Exception as ex:
            if extraer_codigo_sql(ex) == 547:
                raise ConflictoExcepcion("La persona o empresa referenciada no existe en la base de datos.")
            raise

    def actualizar_parcial(self, clave, datos: dict) -> int:
        """Sobrescribe PATCH para traducir error 547 si se intenta cambiar a una FK inexistente."""
        try:
            return super().actualizar_parcial(clave, datos)
        except ConflictoExcepcion:
            raise ConflictoExcepcion("La persona o empresa referenciada no existe en la base de datos.")


class RepositorioVendedor(RepositorioTablaBase):
    """Repositorio SQL Server para 'vendedor' (PK IDENTITY 'id', FK obligatoria a persona)."""
    tabla = "vendedor"
    columna_pk = "id"
    columnas_select = "id, carnet, direccion, fkcodpersona"
    columnas_patch = {"carnet", "direccion", "fkcodpersona"}

    def crear(self, carnet: int, direccion: str, fkcodpersona: str) -> int:
        """Inserta vendedor; si fkcodpersona no existe (SQL 547), lanza 409 Conflict."""
        sql = (
            "INSERT INTO vendedor (carnet, direccion, fkcodpersona) "
            "VALUES (%s, %s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        )
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (carnet, direccion, fkcodpersona))
                    fila = cur.fetchone()
                    return int(fila["nuevo_id"])
        except Exception as ex:
            if extraer_codigo_sql(ex) == 547:
                raise ConflictoExcepcion(f"La persona '{fkcodpersona}' no existe en la base de datos.")
            raise


class RepositorioFactura:
    """
    Repositorio Maestro-Detalle de 'factura' + 'productosporfactura'.
    Opera exclusivamente invocando los Stored Procedures oficiales de SQL Server.
    Los subtotales, total y descuento/devolución de stock los ejecuta SQL Server vía Triggers y SPs.
    """

    def listar(self) -> list[dict]:
        """Ejecuta 'sp_listar_facturas_y_productosporfactura' y deserializa el JSON retornado en @p_resultado."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_listar_facturas_y_productosporfactura @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql)
                fila = cur.fetchone()
                raw_json = fila["resultado"] if fila else "[]"
                if not raw_json or raw_json == "[]":
                    return []
                return json.loads(raw_json)

    def obtener_por_numero(self, numero: int) -> dict:
        """Ejecuta 'sp_consultar_factura_y_productosporfactura'; captura THROW 50003 -> 404."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_consultar_factura_y_productosporfactura @p_numero = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (numero,))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo == 50003 or "no existe" in msg.lower():
                raise NoEncontradoExcepcion(f"No existe la factura número {numero}.")
            raise

    def crear(self, fkidcliente: int, fkidvendedor: int, productos_json: str) -> dict:
        """
        Ejecuta 'sp_insertar_factura_y_productosporfactura' pasando el detalle en JSON.
        Traduce THROW 50001/50002/50010 y 547 (cliente/vendedor/producto inexistente o stock insuficiente) -> 409.
        """
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
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (fkidcliente, fkidvendedor, productos_json))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo in (547, 50001, 50002, 50010) or "stock" in msg.lower() or "no existe" in msg.lower():
                raise ConflictoExcepcion(msg)
            raise

    def anular(self, numero: int) -> dict:
        """
        Ejecuta 'sp_anular_factura': cambia el estado a 'anulada' y devuelve las unidades al stock.
        Si no existe -> 404; si ya estaba anulada -> 409 Conflict.
        """
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC sp_anular_factura @p_numero = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (numero,))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if "ya está anulada" in msg.lower() or "ya esta anulada" in msg.lower():
                raise ConflictoExcepcion(f"La factura {numero} ya se encuentra anulada.")
            if codigo == 50010 or "no existe" in msg.lower():
                raise NoEncontradoExcepcion(f"No existe la factura número {numero}.")
            raise


class RepositorioRolUsuario:
    """Repositorio SQL Server para la tabla puente M:N 'rol_usuario' (clave compuesta fkemail + fkidrol)."""

    def obtener_todos(self) -> list[dict]:
        """Lista todas las asignaciones usuario-rol haciendo JOIN con 'rol' para mostrar el nombre."""
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            ORDER BY ru.fkemail, ru.fkidrol;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql)
                return normalizar_filas(cur.fetchall())

    def obtener_por_usuario(self, email: str) -> list[dict]:
        """Obtiene los roles asignados a un usuario específico."""
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            WHERE ru.fkemail = %s
            ORDER BY ru.fkidrol;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (email,))
                return normalizar_filas(cur.fetchall())

    def obtener_por_rol(self, idrol: int) -> list[dict]:
        """Obtiene los usuarios que tienen asignado un rol específico."""
        sql = """
            SELECT ru.fkemail, ru.fkidrol, r.nombre AS nombre_rol
            FROM rol_usuario ru
            INNER JOIN rol r ON ru.fkidrol = r.id
            WHERE ru.fkidrol = %s
            ORDER BY ru.fkemail;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (idrol,))
                return normalizar_filas(cur.fetchall())

    def crear(self, fkemail: str, fkidrol: int) -> None:
        """Asocia un usuario y un rol; captura 2627/2601 (pareja duplicada) y 547 (FK inexistente) -> 409."""
        sql = "INSERT INTO rol_usuario (fkemail, fkidrol) VALUES (%s, %s)"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (fkemail, fkidrol))
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo in (2627, 2601):
                raise ConflictoExcepcion(f"El usuario '{fkemail}' ya tiene asignado el rol {fkidrol}.")
            if codigo == 547:
                raise ConflictoExcepcion("El usuario o el rol especificado no existe.")
            raise

    def eliminar(self, fkemail: str, fkidrol: int) -> int:
        """Elimina la relación por su clave primaria compuesta (email + idrol)."""
        sql = "DELETE FROM rol_usuario WHERE fkemail = %s AND fkidrol = %s"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (fkemail, fkidrol))
                return cur.rowcount


class RepositorioRutaRol:
    """
    Repositorio de permisos 'rutarol' ejecutado mediante sus 3 Stored Procedures oficiales:
    listar_rutarol, crear_rutarol y eliminar_rutarol.
    """

    def listar(self) -> list[dict]:
        """Ejecuta 'listar_rutarol' y deserializa la lista de permisos rol-ruta."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC listar_rutarol @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql)
                fila = cur.fetchone()
                raw_json = fila["resultado"] if fila else "[]"
                return json.loads(raw_json or "[]")

    def crear(self, fkidruta: int, fkidrol: int) -> dict:
        """Ejecuta 'crear_rutarol'; si 'success' es false en el JSON de salida, lanza ConflictoExcepcion (409)."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC crear_rutarol @p_fkidruta = %s, @p_fkidrol = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (fkidruta, fkidrol))
                fila = cur.fetchone()
                res = json.loads(fila["resultado"])
                if not res.get("success", False):
                    raise ConflictoExcepcion(res.get("message", "No fue posible crear el permiso."))
                return res

    def eliminar(self, fkidruta: int, fkidrol: int) -> dict:
        """Ejecuta 'eliminar_rutarol'; si el permiso no existe, lanza NoEncontradoExcepcion (404)."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC eliminar_rutarol @p_fkidruta = %s, @p_fkidrol = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (fkidruta, fkidrol))
                fila = cur.fetchone()
                res = json.loads(fila["resultado"])
                if not res.get("success", False):
                    raise NoEncontradoExcepcion(res.get("message", "El permiso no existe."))
                return res


class RepositorioUsuarioConRoles:
    """
    Repositorio compuesto para 'usuario-con-roles'.
    Ejecuta atómicamente los 5 Stored Procedures oficiales con OPENJSON en SQL Server.
    """

    def listar(self) -> list[dict]:
        """Ejecuta 'listar_usuarios_con_roles' devolviendo cada usuario y su arreglo de roles."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC listar_usuarios_con_roles @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql)
                fila = cur.fetchone()
                raw_json = fila["resultado"] if fila else "[]"
                return json.loads(raw_json or "[]")

    def consultar(self, email: str) -> dict:
        """Ejecuta 'consultar_usuario_con_roles'; captura THROW 50008 -> 404."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC consultar_usuario_con_roles @p_email = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (email,))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo == 50008 or "no existe" in str(ex).lower():
                raise NoEncontradoExcepcion(f"El usuario '{email}' no existe.")
            raise

    def crear(self, email: str, contrasena_hash: str, roles_json: str) -> dict:
        """Ejecuta 'crear_usuario_con_roles' enviando el arreglo JSON a @p_roles_json (OPENJSON)."""
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
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (email, contrasena_hash, roles_json))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo in (2627, 2601) or "ya existe" in msg.lower():
                raise ConflictoExcepcion(f"El usuario '{email}' ya existe en el sistema.")
            if codigo == 547 or "no existe" in msg.lower():
                raise ConflictoExcepcion("Uno de los roles especificados no existe en la base de datos.")
            raise

    def actualizar(self, email: str, contrasena_hash: str | None, roles_json: str) -> dict:
        """Ejecuta 'actualizar_usuario_con_roles' pasando @p_roles (si @p_contrasena es NULL conserva la clave)."""
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
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (email, contrasena_hash, roles_json))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            msg = str(ex)
            if codigo == 50007 or ("usuario" in msg.lower() and "no existe" in msg.lower()):
                raise NoEncontradoExcepcion(f"El usuario '{email}' no existe.")
            if codigo == 547:
                raise ConflictoExcepcion("Uno de los roles especificados no existe.")
            raise

    def eliminar(self, email: str) -> dict:
        """Ejecuta 'eliminar_usuario_con_roles'; captura THROW 50006 -> 404."""
        sql = """
            DECLARE @out NVARCHAR(MAX);
            EXEC eliminar_usuario_con_roles @p_email = %s, @p_resultado = @out OUTPUT;
            SELECT @out AS resultado;
        """
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (email,))
                    fila = cur.fetchone()
                    return json.loads(fila["resultado"])
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo == 50006 or "no existe" in str(ex).lower():
                raise NoEncontradoExcepcion(f"Usuario '{email}' no existe.")
            raise
