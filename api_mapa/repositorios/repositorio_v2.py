# ==============================================================
# REPOSITORIOS VERSIÓN 2: LAS 10 TABLAS CON CLAVE FORÁNEA (MariaDB)
# - Validan que las claves foráneas existan con activo = 1 (-> HTTP 409 si no).
# - Traducen códigos MySQL 1062 (duplicado) y 1451/1452 (FK) a ConflictoExcepcion.
# - Todas las consultas y listados filtran activo = 1.
# ==============================================================

from database import consultar_fila, consultar_filas, ejecutar_dml, extraer_codigo_mysql
from excepciones import ConflictoExcepcion
from repositorios.helpers_sql import (
    actualizar_whitelist_logico,
    eliminar_logico,
    verificar_padre_activo,
)
from repositorios.interfaces import (
    IRepositorioDocente,
    IRepositorioProducto,
    IRepositorioDesarrolla,
    IRepositorioPuente,
)


class RepositorioDocenteMariaDB(IRepositorioDocente):
    _COLS = (
        "cedula, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, telefono, "
        "url_cvlac, fecha_actualizacion, escalafon, perfil, cat_minciencia, conv_minciencia, "
        "nacionalidaad, linea_investigacion_principal"
    )
    _PATCH = {
        "nombres", "apellidos", "genero", "cargo", "fecha_nacimiento", "correo", "telefono",
        "url_cvlac", "fecha_actualizacion", "escalafon", "perfil", "cat_minciencia",
        "conv_minciencia", "nacionalidaad", "linea_investigacion_principal",
    }

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = f"SELECT {self._COLS} FROM docente WHERE activo = 1 ORDER BY cedula LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, cedula: int) -> dict | None:
        sql = f"SELECT {self._COLS} FROM docente WHERE cedula = %s AND activo = 1"
        return consultar_fila(sql, (cedula,))

    def crear(self, d: dict) -> None:
        if d.get("linea_investigacion_principal") is not None:
            verificar_padre_activo("linea_investigacion", "id", d["linea_investigacion_principal"], "línea de investigación")
        sql = (
            "INSERT INTO docente (cedula, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, "
            "telefono, url_cvlac, fecha_actualizacion, escalafon, perfil, cat_minciencia, conv_minciencia, "
            "nacionalidaad, linea_investigacion_principal, activo) "
            "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, 1)"
        )
        try:
            ejecutar_dml(sql, (
                d["cedula"], d["nombres"], d["apellidos"], d["genero"], d["cargo"],
                d["fecha_nacimiento"], d["correo"], d["telefono"], d["url_cvlac"],
                d["fecha_actualizacion"], d["escalafon"], d["perfil"], d.get("cat_minciencia"),
                d["conv_minciencia"], d["nacionalidaad"], d.get("linea_investigacion_principal"),
            ))
        except Exception as ex:
            cod = extraer_codigo_mysql(ex)
            if cod == 1062:
                raise ConflictoExcepcion(f"Ya existe un docente con cédula {d['cedula']}.")
            if cod in (1451, 1452):
                raise ConflictoExcepcion("La línea de investigación principal referenciada no existe.")
            raise

    def actualizar_parcial(self, cedula: int, datos: dict) -> int:
        if "linea_investigacion_principal" in datos and datos["linea_investigacion_principal"] is not None:
            verificar_padre_activo("linea_investigacion", "id", datos["linea_investigacion_principal"], "línea de investigación")
        return actualizar_whitelist_logico("docente", "cedula", cedula, datos, self._PATCH)

    def eliminar(self, cedula: int) -> int:
        return eliminar_logico("docente", "cedula", cedula)


class RepositorioProductoMariaDB(IRepositorioProducto):
    _COLS = "id, nombre, categoria, fecha_entrega, proyecto, tipo_producto"
    _PATCH = {"nombre", "categoria", "fecha_entrega", "proyecto", "tipo_producto"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = f"SELECT {self._COLS} FROM producto WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_prod: int) -> dict | None:
        sql = f"SELECT {self._COLS} FROM producto WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_prod,))

    def crear(
        self,
        id_prod: int,
        nombre: str,
        categoria: str,
        fecha_entrega: str,
        proyecto: int | None,
        tipo_producto: int,
    ) -> None:
        if proyecto is not None:
            verificar_padre_activo("proyecto", "id", proyecto, "proyecto")
        verificar_padre_activo("tipo_producto", "id", tipo_producto, "tipo de producto")
        sql = (
            "INSERT INTO producto (id, nombre, categoria, fecha_entrega, proyecto, tipo_producto, activo) "
            "VALUES (%s, %s, %s, %s, %s, %s, 1)"
        )
        try:
            ejecutar_dml(sql, (id_prod, nombre, categoria, fecha_entrega, proyecto, tipo_producto))
        except Exception as ex:
            cod = extraer_codigo_mysql(ex)
            if cod == 1062:
                raise ConflictoExcepcion(f"Ya existe un producto con id {id_prod}.")
            if cod in (1451, 1452):
                raise ConflictoExcepcion("El proyecto o tipo_producto referenciado no existe.")
            raise

    def actualizar_parcial(self, id_prod: int, datos: dict) -> int:
        if "proyecto" in datos and datos["proyecto"] is not None:
            verificar_padre_activo("proyecto", "id", datos["proyecto"], "proyecto")
        if "tipo_producto" in datos and datos["tipo_producto"] is not None:
            verificar_padre_activo("tipo_producto", "id", datos["tipo_producto"], "tipo de producto")
        return actualizar_whitelist_logico("producto", "id", id_prod, datos, self._PATCH)

    def eliminar(self, id_prod: int) -> int:
        return eliminar_logico("producto", "id", id_prod)


class RepositorioDesarrollaMariaDB(IRepositorioDesarrolla):
    """Repositorio de participación docente <-> proyecto con atributos propios (rol y descripcion)."""

    def obtener_todos(self) -> list[dict]:
        sql = (
            "SELECT d.docente, d.proyecto, d.rol, d.descripcion "
            "FROM desarrolla d "
            "INNER JOIN docente doc ON doc.cedula = d.docente AND doc.activo = 1 "
            "INNER JOIN proyecto p ON p.id = d.proyecto AND p.activo = 1 "
            "WHERE d.activo = 1 ORDER BY d.docente, d.proyecto"
        )
        return consultar_filas(sql)

    def obtener_por_docente(self, cedula: int) -> list[dict]:
        sql = "SELECT docente, proyecto, rol, descripcion FROM desarrolla WHERE docente = %s AND activo = 1 ORDER BY proyecto"
        return consultar_filas(sql, (cedula,))

    def obtener_por_proyecto(self, id_proyecto: int) -> list[dict]:
        sql = "SELECT docente, proyecto, rol, descripcion FROM desarrolla WHERE proyecto = %s AND activo = 1 ORDER BY docente"
        return consultar_filas(sql, (id_proyecto,))

    def obtener_par(self, cedula: int, id_proyecto: int) -> dict | None:
        sql = "SELECT docente, proyecto, rol, descripcion FROM desarrolla WHERE docente = %s AND proyecto = %s AND activo = 1"
        return consultar_fila(sql, (cedula, id_proyecto))

    def crear(self, cedula: int, id_proyecto: int, rol: str, descripcion: str) -> None:
        verificar_padre_activo("docente", "cedula", cedula, "docente")
        verificar_padre_activo("proyecto", "id", id_proyecto, "proyecto")
        existente = consultar_fila("SELECT activo FROM desarrolla WHERE docente = %s AND proyecto = %s", (cedula, id_proyecto))
        if existente:
            if existente["activo"] == 1:
                raise ConflictoExcepcion(f"El docente {cedula} ya participa en el proyecto {id_proyecto}.")
            sql_re = "UPDATE desarrolla SET rol = %s, descripcion = %s, activo = 1 WHERE docente = %s AND proyecto = %s"
            ejecutar_dml(sql_re, (rol, descripcion, cedula, id_proyecto))
            return
        sql = "INSERT INTO desarrolla (docente, proyecto, rol, descripcion, activo) VALUES (%s, %s, %s, %s, 1)"
        ejecutar_dml(sql, (cedula, id_proyecto, rol, descripcion))

    def actualizar_parcial(self, cedula: int, id_proyecto: int, datos: dict) -> int:
        clausulas = []
        valores = []
        for col in ("rol", "descripcion"):
            if col in datos:
                clausulas.append(f"{col} = %s")
                valores.append(datos[col])
        if not clausulas:
            return 0
        valores.extend([cedula, id_proyecto])
        sql = f"UPDATE desarrolla SET {', '.join(clausulas)} WHERE docente = %s AND proyecto = %s AND activo = 1"
        return ejecutar_dml(sql, tuple(valores))

    def eliminar(self, cedula: int, id_proyecto: int) -> int:
        sql = "UPDATE desarrolla SET activo = 0 WHERE docente = %s AND proyecto = %s AND activo = 1"
        return ejecutar_dml(sql, (cedula, id_proyecto))


class _RepositorioPuenteBase(IRepositorioPuente):
    """Implementación base parametrizada por configuración fija de tabla puente binaria M:N."""

    def __init__(
        self,
        tabla: str,
        col_izq: str,
        col_der: str,
        tabla_izq: str,
        pk_izq: str,
        tabla_der: str,
        pk_der: str,
    ):
        self.tabla = tabla
        self.col_izq = col_izq
        self.col_der = col_der
        self.tabla_izq = tabla_izq
        self.pk_izq = pk_izq
        self.tabla_der = tabla_der
        self.pk_der = pk_der

    def obtener_todos(self) -> list[dict]:
        sql = (
            f"SELECT p.{self.col_izq}, p.{self.col_der} "
            f"FROM {self.tabla} p "
            f"INNER JOIN {self.tabla_izq} ti ON ti.{self.pk_izq} = p.{self.col_izq} AND ti.activo = 1 "
            f"INNER JOIN {self.tabla_der} td ON td.{self.pk_der} = p.{self.col_der} AND td.activo = 1 "
            f"WHERE p.activo = 1 ORDER BY p.{self.col_izq}, p.{self.col_der}"
        )
        return consultar_filas(sql)

    def obtener_por_izquierda(self, clave_izq) -> list[dict]:
        sql = f"SELECT {self.col_izq}, {self.col_der} FROM {self.tabla} WHERE {self.col_izq} = %s AND activo = 1"
        return consultar_filas(sql, (clave_izq,))

    def obtener_por_derecha(self, clave_der) -> list[dict]:
        sql = f"SELECT {self.col_izq}, {self.col_der} FROM {self.tabla} WHERE {self.col_der} = %s AND activo = 1"
        return consultar_filas(sql, (clave_der,))

    def crear(self, clave_izq, clave_der) -> None:
        verificar_padre_activo(self.tabla_izq, self.pk_izq, clave_izq, self.tabla_izq)
        verificar_padre_activo(self.tabla_der, self.pk_der, clave_der, self.tabla_der)
        existente = consultar_fila(
            f"SELECT activo FROM {self.tabla} WHERE {self.col_izq} = %s AND {self.col_der} = %s",
            (clave_izq, clave_der),
        )
        if existente:
            if existente["activo"] == 1:
                raise ConflictoExcepcion(f"La relación ({clave_izq}, {clave_der}) ya existe en '{self.tabla}'.")
            ejecutar_dml(
                f"UPDATE {self.tabla} SET activo = 1 WHERE {self.col_izq} = %s AND {self.col_der} = %s",
                (clave_izq, clave_der),
            )
            return
        ejecutar_dml(
            f"INSERT INTO {self.tabla} ({self.col_izq}, {self.col_der}, activo) VALUES (%s, %s, 1)",
            (clave_izq, clave_der),
        )

    def eliminar(self, clave_izq, clave_der) -> int:
        sql = f"UPDATE {self.tabla} SET activo = 0 WHERE {self.col_izq} = %s AND {self.col_der} = %s AND activo = 1"
        return ejecutar_dml(sql, (clave_izq, clave_der))


class RepositorioAliadoProyectoMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("aliado_proyecto", "aliado", "proyecto", "aliado", "nit", "proyecto", "id")


class RepositorioPalabrasClaveMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("palabras_clave", "proyecto", "termino_clave", "proyecto", "id", "termino_clave", "termino")


class RepositorioAcProyectoMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("ac_proyecto", "proyecto", "area_conocimiento", "proyecto", "id", "area_conocimiento", "id")


class RepositorioProyectoLineaMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("proyecto_linea", "proyecto", "linea_investigacion", "proyecto", "id", "linea_investigacion", "id")


class RepositorioOdsProyectoMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("ods_proyecto", "proyecto", "ods", "proyecto", "id", "objetivo_desarrollo_sostenible", "id")


class RepositorioAaProyectoMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("aa_proyecto", "proyecto", "area_aplicacion", "proyecto", "id", "area_aplicacion", "id")


class RepositorioDocenteProductoMariaDB(_RepositorioPuenteBase):
    def __init__(self):
        super().__init__("docente_producto", "docente", "producto", "docente", "cedula", "producto", "id")
