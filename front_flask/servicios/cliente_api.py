# ==============================================================
# TRANSPORTE HTTP Y SESIONES EN SERVIDOR FLASK (front_flask)
# - Almacena el JWT únicamente en memoria del servidor (SESIONES) tras cookie HttpOnly.
# - Jamás importa pymysql ni conoce credenciales de MariaDB.
# - Resiliente ante caída de api-mapa (reporta aviso limpio y cero filas).
# ==============================================================

import os
import secrets
import httpx

URL_API = os.getenv("URL_API", "http://localhost:8117").rstrip("/")
COOKIE_SESION = "mapa_sid"

# { sid: { "token", "username", "email", "roles", "puede_escribir", "es_admin" } }
SESIONES: dict[str, dict] = {}


def obtener_sesion(sid: str | None) -> dict | None:
    if not sid:
        return None
    return SESIONES.get(sid)


def crear_sesion_servidor(
    token: str,
    username: str,
    email: str,
    roles: list[str],
    puede_escribir: bool,
    es_admin: bool,
) -> str:
    sid = secrets.token_urlsafe(32)
    SESIONES[sid] = {
        "token": token,
        "username": username,
        "email": email,
        "roles": roles,
        "puede_escribir": puede_escribir,
        "es_admin": es_admin,
    }
    return sid


def cerrar_sesion_servidor(sid: str | None) -> None:
    if sid and sid in SESIONES:
        del SESIONES[sid]


def peticion_api(
    metodo: str,
    endpoint: str,
    sesion: dict | None = None,
    json_body: dict | None = None,
) -> dict:
    """Ejecuta peticiones HTTP vía httpx hacia api-mapa traduciendo códigos 204/401/403/409/422 y fallos de red."""
    url = f"{URL_API}/{endpoint.lstrip('/')}"
    headers = {}
    if sesion and sesion.get("token"):
        headers["Authorization"] = f"Bearer {sesion['token']}"

    try:
        with httpx.Client(timeout=15.0) as client:
            resp = client.request(metodo.upper(), url, headers=headers, json=json_body)
    except httpx.RequestError as ex:
        return {
            "ok": False,
            "status": 0,
            "data": None,
            "mensaje": f"El servicio de datos no está disponible en este momento ({URL_API}).",
            "errores": [],
            "error_conexion": True,
            "sin_permiso": False,
            "no_autenticado": False,
            "detalle": str(ex),
        }

    if resp.status_code == 204:
        return {
            "ok": True,
            "status": 204,
            "data": None,
            "mensaje": "Operación completada.",
            "errores": [],
            "error_conexion": False,
            "sin_permiso": False,
            "no_autenticado": False,
        }

    try:
        cuerpo = resp.json()
    except Exception:
        cuerpo = {"mensaje": resp.text}

    if 200 <= resp.status_code < 300:
        return {
            "ok": True,
            "status": resp.status_code,
            "data": cuerpo,
            "mensaje": cuerpo.get("mensaje", "Operación exitosa.") if isinstance(cuerpo, dict) else "Operación exitosa.",
            "errores": [],
            "error_conexion": False,
            "sin_permiso": False,
            "no_autenticado": False,
        }

    mensaje = "No fue posible procesar la solicitud."
    errores = []
    if isinstance(cuerpo, dict):
        mensaje = cuerpo.get("mensaje") or cuerpo.get("detail") or mensaje
        if cuerpo.get("detalle"):
            mensaje = f"{mensaje} {cuerpo['detalle']}"
        if isinstance(cuerpo.get("errores"), list):
            errores = cuerpo["errores"]

    return {
        "ok": False,
        "status": resp.status_code,
        "data": cuerpo,
        "mensaje": mensaje,
        "errores": errores,
        "error_conexion": False,
        "sin_permiso": resp.status_code == 403,
        "no_autenticado": resp.status_code == 401,
    }
