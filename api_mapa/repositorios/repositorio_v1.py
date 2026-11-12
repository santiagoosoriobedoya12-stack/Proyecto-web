# ==============================================================
# REPOSITORIOS VERSIÓN 1: LAS 8 TABLAS SIN CLAVE FORÁNEA (MariaDB)
# - Todos filtran 'activo = 1' y jamás exponen la columna 'activo'.
# - Todos realizan borrado lógico ('activo = 0').
# - Traducen error MySQL 1062 (PK duplicada) a ConflictoExcepcion (409).
# ==============================================================

from database import (
    consultar_fila,
    consultar_filas,
    ejecutar_dml,
    ejecutar_insert_autoincrement,
    extraer_codigo_mysql,
)
from excepciones import ConflictoExcepcion
from repositorios.helpers_sql import actualizar_whitelist_logico, eliminar_logico
from repositorios.interfaces import (
    IRepositorioAreaConocimiento,
    IRepositorioOds,
    IRepositorioAreaAplicacion,
    IRepositorioTerminoClave,
    IRepositorioLineaInvestigacion,
    IRepositorioAliado,
    IRepositorioTipoProducto,
    IRepositorioProyecto,
)


class RepositorioAreaConocimientoMariaDB(IRepositorioAreaConocimiento):
    _PATCH = {"gran_area", "area", "disciplina"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, gran_area, area, disciplina FROM area_conocimiento WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_area: int) -> dict | None:
        sql = "SELECT id, gran_area, area, disciplina FROM area_conocimiento WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_area,))

    def crear(self, id_area: int, gran_area: str, area: str, disciplina: str) -> None:
        sql = "INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo) VALUES (%s, %s, %s, %s, 1)"
        try:
            ejecutar_dml(sql, (id_area, gran_area, area, disciplina))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe area_conocimiento con id {id_area}.")
            raise

    def actualizar_parcial(self, id_area: int, datos: dict) -> int:
        return actualizar_whitelist_logico("area_conocimiento", "id", id_area, datos, self._PATCH)

    def eliminar(self, id_area: int) -> int:
        return eliminar_logico("area_conocimiento", "id", id_area)


class RepositorioOdsMariaDB(IRepositorioOds):
    _PATCH = {"nombre", "categoria"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, nombre, categoria FROM objetivo_desarrollo_sostenible WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_ods: int) -> dict | None:
        sql = "SELECT id, nombre, categoria FROM objetivo_desarrollo_sostenible WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_ods,))

    def crear(self, id_ods: int, nombre: str, categoria: str) -> None:
        sql = "INSERT INTO objetivo_desarrollo_sostenible (id, nombre, categoria, activo) VALUES (%s, %s, %s, 1)"
        try:
            ejecutar_dml(sql, (id_ods, nombre, categoria))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe ODS con id {id_ods}.")
            raise

    def actualizar_parcial(self, id_ods: int, datos: dict) -> int:
        return actualizar_whitelist_logico("objetivo_desarrollo_sostenible", "id", id_ods, datos, self._PATCH)

    def eliminar(self, id_ods: int) -> int:
        return eliminar_logico("objetivo_desarrollo_sostenible", "id", id_ods)


class RepositorioAreaAplicacionMariaDB(IRepositorioAreaAplicacion):
    _PATCH = {"nombre"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, nombre FROM area_aplicacion WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_aa: int) -> dict | None:
        sql = "SELECT id, nombre FROM area_aplicacion WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_aa,))

    def crear(self, id_aa: int, nombre: str) -> None:
        sql = "INSERT INTO area_aplicacion (id, nombre, activo) VALUES (%s, %s, 1)"
        try:
            ejecutar_dml(sql, (id_aa, nombre))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe area_aplicacion con id {id_aa}.")
            raise

    def actualizar_parcial(self, id_aa: int, datos: dict) -> int:
        return actualizar_whitelist_logico("area_aplicacion", "id", id_aa, datos, self._PATCH)

    def eliminar(self, id_aa: int) -> int:
        return eliminar_logico("area_aplicacion", "id", id_aa)


class RepositorioTerminoClaveMariaDB(IRepositorioTerminoClave):
    _PATCH = {"termino_ingles"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT termino, termino_ingles FROM termino_clave WHERE activo = 1 ORDER BY termino LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, termino: str) -> dict | None:
        sql = "SELECT termino, termino_ingles FROM termino_clave WHERE termino = %s AND activo = 1"
        return consultar_fila(sql, (termino,))

    def crear(self, termino: str, termino_ingles: str | None) -> None:
        sql = "INSERT INTO termino_clave (termino, termino_ingles, activo) VALUES (%s, %s, 1)"
        try:
            ejecutar_dml(sql, (termino, termino_ingles))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe termino_clave '{termino}'.")
            raise

    def actualizar_parcial(self, termino: str, datos: dict) -> int:
        return actualizar_whitelist_logico("termino_clave", "termino", termino, datos, self._PATCH)

    def eliminar(self, termino: str) -> int:
        return eliminar_logico("termino_clave", "termino", termino)


class RepositorioLineaInvestigacionMariaDB(IRepositorioLineaInvestigacion):
    _PATCH = {"nombre", "descripcion"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, nombre, descripcion FROM linea_investigacion WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_linea: int) -> dict | None:
        sql = "SELECT id, nombre, descripcion FROM linea_investigacion WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_linea,))

    def crear(self, nombre: str, descripcion: str) -> int:
        sql = "INSERT INTO linea_investigacion (nombre, descripcion, activo) VALUES (%s, %s, 1)"
        return ejecutar_insert_autoincrement(sql, (nombre, descripcion))

    def actualizar_parcial(self, id_linea: int, datos: dict) -> int:
        return actualizar_whitelist_logico("linea_investigacion", "id", id_linea, datos, self._PATCH)

    def eliminar(self, id_linea: int) -> int:
        return eliminar_logico("linea_investigacion", "id", id_linea)


class RepositorioAliadoMariaDB(IRepositorioAliado):
    _PATCH = {"razon_social", "nombre_contacto", "correo", "telefono", "ciudad"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT nit, razon_social, nombre_contacto, correo, telefono, ciudad FROM aliado WHERE activo = 1 ORDER BY nit LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, nit: int) -> dict | None:
        sql = "SELECT nit, razon_social, nombre_contacto, correo, telefono, ciudad FROM aliado WHERE nit = %s AND activo = 1"
        return consultar_fila(sql, (nit,))

    def crear(self, nit: int, razon_social: str, nombre_contacto: str, correo: str, telefono: str, ciudad: str) -> None:
        sql = "INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo) VALUES (%s, %s, %s, %s, %s, %s, 1)"
        try:
            ejecutar_dml(sql, (nit, razon_social, nombre_contacto, correo, telefono, ciudad))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe un aliado con NIT {nit}.")
            raise

    def actualizar_parcial(self, nit: int, datos: dict) -> int:
        return actualizar_whitelist_logico("aliado", "nit", nit, datos, self._PATCH)

    def eliminar(self, nit: int) -> int:
        return eliminar_logico("aliado", "nit", nit)


class RepositorioTipoProductoMariaDB(IRepositorioTipoProducto):
    _PATCH = {"categoria", "clase", "nombre", "tipologia"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, categoria, clase, nombre, tipologia FROM tipo_producto WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_tipo: int) -> dict | None:
        sql = "SELECT id, categoria, clase, nombre, tipologia FROM tipo_producto WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_tipo,))

    def crear(self, id_tipo: int, categoria: str, clase: str, nombre: str, tipologia: str) -> None:
        sql = "INSERT INTO tipo_producto (id, categoria, clase, nombre, tipologia, activo) VALUES (%s, %s, %s, %s, %s, 1)"
        try:
            ejecutar_dml(sql, (id_tipo, categoria, clase, nombre, tipologia))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe tipo_producto con id {id_tipo}.")
            raise

    def actualizar_parcial(self, id_tipo: int, datos: dict) -> int:
        return actualizar_whitelist_logico("tipo_producto", "id", id_tipo, datos, self._PATCH)

    def eliminar(self, id_tipo: int) -> int:
        return eliminar_logico("tipo_producto", "id", id_tipo)


class RepositorioProyectoMariaDB(IRepositorioProyecto):
    _PATCH = {"titulo", "resumen", "presupuesto", "tipo_financiacion", "tipo_fondos", "fecha_inicio", "fecha_fin"}

    def obtener_todos(self, limite: int) -> list[dict]:
        sql = "SELECT id, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin FROM proyecto WHERE activo = 1 ORDER BY id LIMIT %s"
        return consultar_filas(sql, (limite,))

    def obtener_por_clave(self, id_proyecto: int) -> dict | None:
        sql = "SELECT id, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin FROM proyecto WHERE id = %s AND activo = 1"
        return consultar_fila(sql, (id_proyecto,))

    def crear(
        self,
        id_proyecto: int,
        titulo: str,
        resumen: str,
        presupuesto: float,
        tipo_financiacion: str,
        tipo_fondos: str,
        fecha_inicio: str,
        fecha_fin: str | None,
    ) -> None:
        sql = (
            "INSERT INTO proyecto (id, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin, activo) "
            "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, 1)"
        )
        try:
            ejecutar_dml(sql, (id_proyecto, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin))
        except Exception as ex:
            if extraer_codigo_mysql(ex) == 1062:
                raise ConflictoExcepcion(f"Ya existe un proyecto con id {id_proyecto}.")
            raise

    def actualizar_parcial(self, id_proyecto: int, datos: dict) -> int:
        return actualizar_whitelist_logico("proyecto", "id", id_proyecto, datos, self._PATCH)

    def eliminar(self, id_proyecto: int) -> int:
        return eliminar_logico("proyecto", "id", id_proyecto)
