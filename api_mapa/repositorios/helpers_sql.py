# ==============================================================
# UTILIDADES SQL PARA MARIADB (BORRADO LÓGICO Y LISTAS BLANCAS)
# Regla de la metodología (§7.0):
# - Toda consulta filtra 'activo = 1'.
# - Eliminar nunca borra físicamente en módulos v1/v2/v3; marca 'activo = 0'.
# - Todo padre foráneo debe existir con 'activo = 1' antes de vincular hijos.
# ==============================================================

from database import consultar_fila, ejecutar_dml, extraer_codigo_mysql
from excepciones import ConflictoExcepcion


def verificar_padre_activo(tabla: str, col_pk: str, valor, etiqueta: str) -> None:
    """Garantiza que una clave foránea exista y no haya sido retirada lógicamente (activo = 1)."""
    sql = f"SELECT 1 FROM {tabla} WHERE {col_pk} = %s AND activo = 1"
    if not consultar_fila(sql, (valor,)):
        raise ConflictoExcepcion(f"No existe {etiqueta} activo con identificador '{valor}'.")


def actualizar_whitelist_logico(
    tabla: str,
    col_pk: str,
    clave,
    datos: dict,
    permitidos: set[str],
) -> int:
    """Ejecuta UPDATE parametrizado validando columnas permitidas y filtrando activo = 1."""
    clausulas = []
    valores = []
    for col, val in datos.items():
        col_l = col.lower()
        if col_l in permitidos:
            clausulas.append(f"{col_l} = %s")
            valores.append(val)
    if not clausulas:
        return 0
    valores.append(clave)
    sql = f"UPDATE {tabla} SET {', '.join(clausulas)} WHERE {col_pk} = %s AND activo = 1"
    try:
        return ejecutar_dml(sql, tuple(valores))
    except Exception as ex:
        cod = extraer_codigo_mysql(ex)
        if cod == 1062:
            raise ConflictoExcepcion(f"Valor único duplicado en '{tabla}'.")
        if cod in (1451, 1452):
            raise ConflictoExcepcion(f"Conflicto de clave foránea en '{tabla}'.")
        raise


def eliminar_logico(tabla: str, col_pk: str, clave) -> int:
    """Retira lógicamente marcando activo = 0 únicamente si estaba activo = 1."""
    sql = f"UPDATE {tabla} SET activo = 0 WHERE {col_pk} = %s AND activo = 1"
    return ejecutar_dml(sql, (clave,))
