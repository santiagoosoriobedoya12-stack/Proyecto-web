# ==============================================================
# TRANSPORTE HTTP Y GESTIÓN DE SESIONES EN SERVIDOR (front_python - Flask)
# - Mantiene el token JWT en memoria del servidor (SESIONES[sid]), vinculado
#   al navegador mediante cookie HttpOnly ('factucorp_sid').
# - El front jamás importa pymssql ni toca SQL Server: solo habla HTTP con la API.
# - Maneja errores 401, 403, 409, 422 y desconexión de red sin colapsar la vista.
# ==============================================================

import os
import secrets
import httpx

URL_API = os.getenv("UrlApi", os.getenv("URL_API", "http://localhost:8035")).rstrip("/")
COOKIE_SESION = "factucorp_sid"

# Almacén de sesiones en memoria del servidor Flask: { sid: { token, email, roles, permisos } }
SESIONES: dict[str, dict] = {}


def obtener_sesion(sid: str | None) -> dict | None:
    """Devuelve la sesión activa del servidor si existe la cookie de sesión."""
    if not sid:
        return None
    return SESIONES.get(sid)


def crear_sesion_servidor(token: str, email: str, roles: list[str], permisos: list[str]) -> str:
    """Guarda el token JWT y permisos en memoria y retorna el identificador aleatorio HttpOnly."""
    sid = secrets.token_urlsafe(32)
    SESIONES[sid] = {
        "token": token,
        "email": email,
        "roles": roles,
        "permisos": permisos,
    }
    return sid


def cerrar_sesion_servidor(sid: str | None) -> None:
    """Elimina la sesión activa del servidor al cerrar sesión."""
    if sid and sid in SESIONES:
        del SESIONES[sid]


def tiene_permiso(sesion: dict | None, ruta: str) -> bool:
    """Verifica si la ruta aparece en los permisos cargados para el menú visual."""
    if not sesion:
        return False
    return ruta in sesion.get("permisos", [])


def peticion_api(
    metodo: str,
    endpoint: str,
    sesion: dict | None = None,
    json_body: dict | None = None,
) -> dict:
    """
    Ejecuta una petición HTTP síncrona vía httpx hacia la API FastAPI.
    Inyecta 'Authorization: Bearer <token>' si hay sesión activa.
    """
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
            "mensaje": f"No se pudo conectar con el servicio de datos ({URL_API}). Verifica que la API esté encendida.",
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
            "mensaje": "Operación completada correctamente.",
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

    mensaje = "Error al procesar la petición."
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
