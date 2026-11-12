# Context manager para cerrar conexiones SQL Server de forma automática incluso si ocurre un error
from contextlib import contextmanager
# Permite serializar tipos Decimal de SQL Server como float/JSON estándar
from decimal import Decimal
from datetime import datetime, date
import pymssql
from config import DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD


@contextmanager
def obtener_conexion():
    """
    Abre una conexión hacia SQL Server 2022 usando pymssql con as_dict=True
    (cada fila devuelta llega como diccionario {columna: valor}).
    Al salir del bloque 'with', cierra automáticamente la conexión.
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
    Extrae el código numérico de error nativo de SQL Server desde una excepción de pymssql.
    Ejemplos:
      - 547: Violación de clave foránea (FK inexistente o padre con hijos asociados)
      - 2627 / 2601: Violación de clave primaria o restricción UNIQUE (duplicado)
      - 50001..50010: Errores controlados lanzados con THROW dentro de los Stored Procedures
    """
    if hasattr(ex, "args") and len(ex.args) > 0:
        primero = ex.args[0]
        if isinstance(primero, int):
            return primero
    return None


def normalizar_fila(fila: dict | None) -> dict | None:
    """
    Convierte tipos especiales de SQL Server (Decimal, datetime, date) a tipos nativos JSON
    para serialización limpia hacia respuestas REST.
    """
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
    """Aplica normalizar_fila a toda una lista de registros devueltos por SQL Server."""
    return [normalizar_fila(f) for f in filas]
