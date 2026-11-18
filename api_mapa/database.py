# ==============================================================
# CONEXIÓN A BASE DE DATOS (HÍBRIDO POSTGRESQL + MARIADB / MYSQL)
# - En la nube (Render / Supabase / Neon): usa PostgreSQL vía psycopg2 con RealDictCursor.
# - En local (Docker Compose): usa MariaDB vía pymysql con DictCursor y CLIENT.FOUND_ROWS.
# - Traduce códigos de error nativos (PostgreSQL 23505 -> 1062, 23503 -> 1452) para
#   que los repositorios sigan funcionando sin modificar su lógica de negocio.
# ==============================================================

from contextlib import contextmanager
from datetime import date, datetime
from decimal import Decimal
from config import (
    DATABASE_URL,
    DB_ENGINE,
    DB_HOST,
    DB_NAME,
    DB_PASSWORD,
    DB_PORT,
    DB_USER,
)


@contextmanager
def obtener_conexion():
    """Abre conexión con cursor tipo diccionario según el motor configurado."""
    if DB_ENGINE == "postgres":
        import psycopg2
        import psycopg2.extras

        if DATABASE_URL:
            conexion = psycopg2.connect(
                DATABASE_URL,
                cursor_factory=psycopg2.extras.RealDictCursor,
            )
        else:
            conexion = psycopg2.connect(
                host=DB_HOST,
                port=DB_PORT,
                user=DB_USER,
                password=DB_PASSWORD,
                dbname=DB_NAME,
                cursor_factory=psycopg2.extras.RealDictCursor,
            )
        conexion.autocommit = True
        try:
            yield conexion
        finally:
            conexion.close()
    else:
        import pymysql
        from pymysql.constants import CLIENT
        from pymysql.cursors import DictCursor

        conexion = pymysql.connect(
            host=DB_HOST,
            port=DB_PORT,
            user=DB_USER,
            password=DB_PASSWORD,
            database=DB_NAME,
            charset="utf8mb4",
            cursorclass=DictCursor,
            client_flag=CLIENT.FOUND_ROWS,
            autocommit=True,
        )
        try:
            yield conexion
        finally:
            conexion.close()


def extraer_codigo_mysql(ex: Exception) -> int | None:
    """
    Extrae o unifica el código de error SQL:
      - 1062: Duplicate entry (PostgreSQL 23505)
      - 1452 / 1451: Conflicto de clave foránea (PostgreSQL 23503)
    """
    # Si viene de PostgreSQL (psycopg2)
    pgcode = getattr(ex, "pgcode", None)
    if pgcode:
        if pgcode == "23505":
            return 1062  # unique_violation
        if pgcode in ("23503", "23502"):
            return 1452  # foreign_key_violation / not_null_violation
        return None

    # Si viene de MariaDB / MySQL (pymysql)
    if hasattr(ex, "args") and len(ex.args) > 0 and isinstance(ex.args[0], int):
        return ex.args[0]
    return None


def normalizar_fila(fila: dict | None) -> dict | None:
    """Convierte Decimal, date y datetime a tipos serializables JSON estándar."""
    if fila is None:
        return None
    salida = {}
    for clave, valor in fila.items():
        if isinstance(valor, Decimal):
            salida[clave] = float(valor)
        elif isinstance(valor, (datetime, date)):
            salida[clave] = valor.isoformat()
        else:
            salida[clave] = valor
    return salida


def normalizar_filas(filas: list[dict]) -> list[dict]:
    """Aplica normalizar_fila a una lista de filas."""
    return [normalizar_fila(f) for f in filas]


def consultar_filas(sql: str, params: tuple = ()) -> list[dict]:
    """Ejecuta SELECT parametrizado y retorna lista de diccionarios normalizados."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            filas = cur.fetchall()
            return normalizar_filas(filas)


def consultar_fila(sql: str, params: tuple = ()) -> dict | None:
    """Ejecuta SELECT de un único registro y lo normaliza."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            fila = cur.fetchone()
            return normalizar_fila(fila)


def ejecutar_dml(sql: str, params: tuple = ()) -> int:
    """Ejecuta INSERT/UPDATE/DELETE parametrizado y retorna filas afectadas/coincidentes."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return cur.rowcount


def ejecutar_insert_autoincrement(sql: str, params: tuple = (), col_id: str = "id") -> int:
    """
    Ejecuta INSERT en tabla autoincremental y retorna el nuevo ID generado.
    Soporta MariaDB (cur.lastrowid) y PostgreSQL (RETURNING id).
    """
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            if DB_ENGINE == "postgres":
                sql_ret = f"{sql.rstrip(';')} RETURNING {col_id};"
                cur.execute(sql_ret, params)
                res = cur.fetchone()
                if isinstance(res, dict):
                    return int(res[col_id])
                return int(res[0])
            else:
                cur.execute(sql, params)
                return int(cur.lastrowid)
