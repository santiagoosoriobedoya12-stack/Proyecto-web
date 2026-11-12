# ==============================================================
# OPERACIONES EXPLÍCITAS POR RECURSO CONTRA LA API (front_flask)
# Evita usar un cliente genérico con el nombre de tabla como parámetro en las vistas:
# cada recurso declara explícitamente sus funciones de consulta y escritura.
# ==============================================================

from urllib.parse import quote
from servicios.cliente_api import peticion_api


def _crud_ops(endpoint_base: str) -> dict:
    """Genera funciones explícitas ligadas a un endpoint REST concreto."""
    return {
        "listar": lambda sesion, lim=1000: peticion_api("GET", f"{endpoint_base}?limite={lim}", sesion=sesion),
        "obtener": lambda sesion, pk: peticion_api("GET", f"{endpoint_base}/{quote(str(pk))}", sesion=sesion),
        "crear": lambda sesion, datos: peticion_api("POST", endpoint_base, sesion=sesion, json_body=datos),
        "reemplazar": lambda sesion, pk, datos: peticion_api("PUT", f"{endpoint_base}/{quote(str(pk))}", sesion=sesion, json_body=datos),
        "actualizar": lambda sesion, pk, datos: peticion_api("PATCH", f"{endpoint_base}/{quote(str(pk))}", sesion=sesion, json_body=datos),
        "retirar": lambda sesion, pk: peticion_api("DELETE", f"{endpoint_base}/{quote(str(pk))}", sesion=sesion),
    }


def _puente_ops(endpoint_base: str, campo_izq: str, campo_der: str) -> dict:
    return {
        "listar": lambda sesion: peticion_api("GET", endpoint_base, sesion=sesion),
        "vincular": lambda sesion, izq, der: peticion_api("POST", endpoint_base, sesion=sesion, json_body={campo_izq: izq, campo_der: der}),
        "retirar": lambda sesion, izq, der: peticion_api("DELETE", f"{endpoint_base}/{quote(str(izq))}/{quote(str(der))}", sesion=sesion),
    }


# --- v3: Autenticación y permisos propios ---
def login_api(usuario: str, password: str) -> dict:
    return peticion_api("POST", "api/login", json_body={"usuario": usuario.strip(), "password": password})


def obtener_mis_permisos_api(sesion: dict) -> dict:
    return peticion_api("GET", "api/permisos/mios", sesion=sesion)


# --- v1: 8 tablas sin FK ---
ops_areas_conocimiento = _crud_ops("api/area-conocimiento")
ops_ods = _crud_ops("api/objetivo-desarrollo-sostenible")
ops_areas_aplicacion = _crud_ops("api/area-aplicacion")
ops_terminos_clave = _crud_ops("api/termino-clave")
ops_lineas_investigacion = _crud_ops("api/linea-investigacion")
ops_aliados = _crud_ops("api/aliado")
ops_tipos_producto = _crud_ops("api/tipo-producto")
ops_proyectos = _crud_ops("api/proyecto")

# --- v2: 10 tablas con FK ---
ops_docentes = _crud_ops("api/docente")
ops_productos = _crud_ops("api/producto")

def listar_desarrolla(sesion: dict | None) -> dict:
    return peticion_api("GET", "api/desarrolla", sesion=sesion)

def crear_desarrolla(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/desarrolla", sesion=sesion, json_body=datos)

def reemplazar_desarrolla(sesion: dict | None, docente: int, proyecto: int, datos: dict) -> dict:
    return peticion_api("PUT", f"api/desarrolla/{docente}/{proyecto}", sesion=sesion, json_body=datos)

def retirar_desarrolla(sesion: dict | None, docente: int, proyecto: int) -> dict:
    return peticion_api("DELETE", f"api/desarrolla/{docente}/{proyecto}", sesion=sesion)

ops_aliado_proyecto = _puente_ops("api/aliado-proyecto", "aliado", "proyecto")
ops_palabras_clave = _puente_ops("api/palabras-clave", "proyecto", "termino_clave")
ops_ac_proyecto = _puente_ops("api/ac-proyecto", "proyecto", "area_conocimiento")
ops_proyecto_linea = _puente_ops("api/proyecto-linea", "proyecto", "linea_investigacion")
ops_ods_proyecto = _puente_ops("api/ods-proyecto", "proyecto", "ods")
ops_aa_proyecto = _puente_ops("api/aa-proyecto", "proyecto", "area_aplicacion")
ops_docente_producto = _puente_ops("api/docente-producto", "docente", "producto")

# --- v3: Admin usuarios y roles ---
ops_roles = _crud_ops("api/rol")
ops_usuarios = _crud_ops("api/usuario")
ops_rol_usuario = _puente_ops("api/rol-usuario", "usuario_id", "rol_id")
