# ==============================================================
# CAPA DE INFRAESTRUCTURA DE DATOS (SQL SERVER VÍA PYMSSQL)
# Encapsula la apertura/cierre seguro de conexiones, conversión de tipos JSON
# y utilidades parametrizadas para los repositorios concretos.
# ==============================================================

from contextlib import contextmanager
from decimal import Decimal
from datetime import datetime, date
import json
import pymssql
from config import DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD


@contextmanager
def obtener_conexion():
    """
    Abre una conexión hacia SQL Server 2022 usando pymssql con as_dict=True
    (cada fila llega como diccionario {columna: valor}).
    El bloque 'with' garantiza cerrar la conexión incluso ante excepciones.
    """
    conexion = pymssql.connect(
        server=DB_HOST,
        port=DB_PORT,
        user=DB_USER,
        password=DB_PASSWORD,
        database=DB_NAME,
        charset="UTF-8",
        as_dict=True,
        autocommit=True,
    )
    try:
        yield conexion
    finally:
        conexion.close()


def extraer_codigo_sql(ex: Exception) -> int | None:
    """
    Extrae el código numérico de error nativo de SQL Server:
      - 547: Violación de clave foránea (FK inexistente o padre con hijos)
      - 2627 / 2601: Violación de clave primaria o restricción UNIQUE
      - 50001..50010: Errores controlados con THROW dentro de Stored Procedures
    """
    if hasattr(ex, "args") and len(ex.args) > 0 and isinstance(ex.args[0], int):
        return ex.args[0]
    return None


def normalizar_fila(fila: dict | None) -> dict | None:
    """Convierte Decimal y datetime/date de SQL Server a tipos JSON estándar."""
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
    """Normaliza una lista de diccionarios retornada por pymssql."""
    return [normalizar_fila(f) for f in filas]


def consultar_filas(sql: str, params: tuple = ()) -> list[dict]:
    """Ejecuta una consulta SELECT parametrizada y retorna filas normalizadas."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return normalizar_filas(cur.fetchall())


def consultar_fila(sql: str, params: tuple = ()) -> dict | None:
    """Ejecuta una consulta SELECT de un único registro y lo normaliza."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return normalizar_fila(cur.fetchone())


def ejecutar_dml(sql: str, params: tuple = ()) -> int:
    """Ejecuta INSERT/UPDATE/DELETE parametrizado y retorna el número de filas afectadas."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            return cur.rowcount


def ejecutar_insert_identity(sql: str, params: tuple = ()) -> int:
    """Ejecuta un INSERT con SCOPE_IDENTITY() y retorna el ID entero generado."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            fila = cur.fetchone()
            return int(fila["nuevo_id"])


def ejecutar_sp_json(sql: str, params: tuple = ()) -> dict | list:
    """Ejecuta un Stored Procedure con parámetro @p_resultado OUTPUT JSON y lo deserializa."""
    with obtener_conexion() as conn:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            fila = cur.fetchone()
            raw_json = fila["resultado"] if fila and fila.get("resultado") else "[]"
            return json.loads(raw_json)
