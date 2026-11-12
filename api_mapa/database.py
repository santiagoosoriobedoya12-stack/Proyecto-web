# ==============================================================
# CONEXIÓN A MARIADB / MYSQL VÍA PYMYSQL (CAPA DE INFRAESTRUCTURA)
# Nota crítica (§guía v1): Se activa CLIENT.FOUND_ROWS para que las sentencias
# UPDATE retornen las filas COINCIDENTES y no solo las modificadas (evita 404
# falso en PUT idempotente con los mismos valores).
# ==============================================================

from contextlib import contextmanager
from datetime import date, datetime
from decimal import Decimal
import pymysql
from pymysql.constants import CLIENT
from pymysql.cursors import DictCursor
from config import DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD


@contextmanager
def obtener_conexion():
    """Abre conexión MariaDB con DictCursor + FOUND_ROWS y la cierra al salir."""
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
    Extrae el código de error nativo de MySQL/MariaDB:
      - 1062: Duplicate entry (PK o UNIQUE repetido)
      - 1452: Cannot add or update a child row (FK padre no existe)
      - 1451: Cannot delete or update a parent row (tiene hijos asociados)
    """
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
            return normalizar_filas(cur.fetchall())


def consultar_fila(sql: str, params: tuple = ()) -> dict | None:
    """Ejecuta SELECT de un único registro y lo normaliza."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return normalizar_fila(cur.fetchone())


def ejecutar_dml(sql: str, params: tuple = ()) -> int:
    """Ejecuta INSERT/UPDATE/DELETE parametrizado y retorna filas afectadas/coincidentes."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return cur.rowcount


def ejecutar_insert_autoincrement(sql: str, params: tuple = ()) -> int:
    """Ejecuta INSERT en tabla AUTO_INCREMENT y retorna el nuevo ID generado."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return int(cur.lastrowid)
