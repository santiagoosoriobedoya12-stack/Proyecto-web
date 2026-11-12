# ==============================================================
# REPOSITORIOS VERSIÓN 1: LAS 6 TABLAS INDEPENDIENTES (SIN FK)
# Reglas arquitectónicas:
# - SQL explícito y a la vista, siempre parametrizado con '%s' (jamás f-strings con valores).
# - Lista blanca de columnas permitidas en PATCH para evitar inyección SQL.
# - Captura errores de SQL Server (2627/2601 duplicados, 547 FK si un padre tiene hijos) -> ConflictoExcepcion (409).
# - En RepositorioUsuario, los SELECT jamás devuelven la columna 'contrasena' (seguridad v3).
# ==============================================================

from database import obtener_conexion, extraer_codigo_sql, normalizar_fila, normalizar_filas
from excepciones import ConflictoExcepcion, NoEncontradoExcepcion


class RepositorioTablaBase:
    """
    Clase base reutilizable para repositorios CRUD con SQL parametrizado en SQL Server.
    Evita duplicar lógica idéntica de SELECT / INSERT / PATCH / DELETE manteniendo el SQL explícito.
    """
    tabla: str = ""
    columna_pk: str = ""
    columnas_select: str = ""
    columnas_insert: tuple[str, ...] = ()
    columnas_patch: set[str] = set()
    orden_por: str = ""

    def obtener_todos(self, limite: int) -> list[dict]:
        """Consulta TOP (%s) registros ordenados por clave primaria."""
        sql = f"SELECT TOP (%s) {self.columnas_select} FROM {self.tabla} ORDER BY {self.orden_por or self.columna_pk}"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (limite,))
                return normalizar_filas(cur.fetchall())

    def obtener_por_clave(self, clave) -> dict | None:
        """Busca un único registro por su clave primaria (retorna None si no existe)."""
        sql = f"SELECT {self.columnas_select} FROM {self.tabla} WHERE {self.columna_pk} = %s"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (clave,))
                return normalizar_fila(cur.fetchone())

    def actualizar_parcial(self, clave, datos: dict) -> int:
        """
        Construye UPDATE dinámico validando cada columna contra la lista blanca 'columnas_patch'.
        Devuelve el conteo de filas modificadas en SQL Server.
        """
        if not datos:
            return 0
        clausulas_set = []
        valores = []
        for col, val in datos.items():
            col_lower = col.lower()
            if col_lower in self.columnas_patch:
                clausulas_set.append(f"{col_lower} = %s")
                valores.append(val)
        if not clausulas_set:
            return 0
        valores.append(clave)
        sql = f"UPDATE {self.tabla} SET {', '.join(clausulas_set)} WHERE {self.columna_pk} = %s"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, tuple(valores))
                    return cur.rowcount
        except Exception as ex:
            codigo = extraer_codigo_sql(ex)
            if codigo in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe un registro duplicado en '{self.tabla}'.")
            if codigo == 547:
                raise ConflictoExcepcion(f"Conflicto de integridad referencial en '{self.tabla}'.")
            raise

    def eliminar(self, clave) -> int:
        """Elimina físicamente un registro por clave primaria; captura error 547 si tiene registros hijos."""
        sql = f"DELETE FROM {self.tabla} WHERE {self.columna_pk} = %s"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (clave,))
                    return cur.rowcount
        except Exception as ex:
            if extraer_codigo_sql(ex) == 547:
                raise ConflictoExcepcion(
                    f"No se puede eliminar de '{self.tabla}' porque tiene registros asociados (clave foránea)."
                )
            raise


class RepositorioProducto(RepositorioTablaBase):
    """Repositorio SQL Server para 'producto' (PK string 'codigo')."""
    tabla = "producto"
    columna_pk = "codigo"
    columnas_select = "codigo, nombre, stock, valorunitario"
    columnas_patch = {"nombre", "stock", "valorunitario"}

    def crear(self, codigo: str, nombre: str, stock: int, valorunitario: float) -> None:
        """Inserta un nuevo producto; lanza ConflictoExcepcion (409) si el código ya existe."""
        sql = "INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES (%s, %s, %s, %s)"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (codigo, nombre, stock, valorunitario))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe un producto con codigo '{codigo}'.")
            raise


class RepositorioEmpresa(RepositorioTablaBase):
    """Repositorio SQL Server para 'empresa' (PK string 'codigo')."""
    tabla = "empresa"
    columna_pk = "codigo"
    columnas_select = "codigo, nombre"
    columnas_patch = {"nombre"}

    def crear(self, codigo: str, nombre: str) -> None:
        """Inserta una nueva empresa; lanza 409 si el código está duplicado."""
        sql = "INSERT INTO empresa (codigo, nombre) VALUES (%s, %s)"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (codigo, nombre))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una empresa con codigo '{codigo}'.")
            raise


class RepositorioPersona(RepositorioTablaBase):
    """Repositorio SQL Server para 'persona' (PK string 'codigo')."""
    tabla = "persona"
    columna_pk = "codigo"
    columnas_select = "codigo, nombre, email, telefono"
    columnas_patch = {"nombre", "email", "telefono"}

    def crear(self, codigo: str, nombre: str, email: str, telefono: str) -> None:
        """Inserta una persona; lanza 409 si el código está duplicado."""
        sql = "INSERT INTO persona (codigo, nombre, email, telefono) VALUES (%s, %s, %s, %s)"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (codigo, nombre, email, telefono))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una persona con codigo '{codigo}'.")
            raise


class RepositorioRol(RepositorioTablaBase):
    """Repositorio SQL Server para 'rol' (PK autoincremental IDENTITY 'id')."""
    tabla = "rol"
    columna_pk = "id"
    columnas_select = "id, nombre"
    columnas_patch = {"nombre"}

    def crear(self, nombre: str) -> int:
        """Inserta un rol y recupera el ID recién generado mediante SCOPE_IDENTITY()."""
        sql = "INSERT INTO rol (nombre) VALUES (%s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        with obtener_conexion() as conn:
            with conn.cursor() as cur:
                cur.execute(sql, (nombre,))
                fila = cur.fetchone()
                return int(fila["nuevo_id"])


class RepositorioRuta(RepositorioTablaBase):
    """Repositorio SQL Server para 'ruta' (PK IDENTITY 'id' y restricción UNIQUE en 'ruta')."""
    tabla = "ruta"
    columna_pk = "id"
    columnas_select = "id, ruta, descripcion"
    columnas_patch = {"ruta", "descripcion"}

    def crear(self, ruta: str, descripcion: str) -> int:
        """Inserta una ruta protegible y devuelve su ID; lanza 409 si el patrón 'ruta' ya existe."""
        sql = "INSERT INTO ruta (ruta, descripcion) VALUES (%s, %s); SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevo_id;"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (ruta, descripcion))
                    fila = cur.fetchone()
                    return int(fila["nuevo_id"])
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe una ruta registrada con el patrón '{ruta}'.")
            raise


class RepositorioUsuario(RepositorioTablaBase):
    """
    Repositorio SQL Server para 'usuario' (PK string 'email').
    REGLA v3: 'columnas_select' solo incluye 'email'; jamás retorna la columna 'contrasena'.
    """
    tabla = "usuario"
    columna_pk = "email"
    columnas_select = "email"
    columnas_patch = {"contrasena"}

    def crear(self, email: str, contrasena_hash: str) -> None:
        """Inserta un usuario guardando el hash BCrypt en 'contrasena'."""
        sql = "INSERT INTO usuario (email, contrasena) VALUES (%s, %s)"
        try:
            with obtener_conexion() as conn:
                with conn.cursor() as cur:
                    cur.execute(sql, (email, contrasena_hash))
        except Exception as ex:
            if extraer_codigo_sql(ex) in (2627, 2601):
                raise ConflictoExcepcion(f"Ya existe un usuario con email '{email}'.")
            raise
