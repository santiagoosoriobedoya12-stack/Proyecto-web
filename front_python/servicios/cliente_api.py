# ==============================================================
# CLIENTE HTTP Y GESTIÓN DE SESIONES EN MEMORIA (front_python)
# Equivalente en Python al 'EstadoSesion' Scoped + 'Servicio*' de Blazor:
# 1. El token JWT NUNCA viaja ni se guarda en JavaScript/LocalStorage del navegador.
#    Solo se envía una cookie HttpOnly con un ID aleatorio de sesión; el token JWT
#    vive exclusivamente en memoria del servidor Python (SESIONES[sid]).
# 2. Traduce respuestas 200, 201, 204, 400, 401, 403, 404, 409, 422 y caídas de red.
# ==============================================================

import os
import secrets
import httpx

# URL base de la API REST (en Docker: http://api-facturas:8032; local: http://localhost:8035)
URL_API = os.getenv("UrlApi", os.getenv("URL_API", "http://localhost:8035")).rstrip("/")

COOKIE_SESION = "factucorp_sid"

# Diccionario de sesiones activas del lado del servidor: { sid: { "token", "email", "roles", "permisos" } }
SESIONES: dict[str, dict] = {}


def obtener_sesion(sid: str | None) -> dict | None:
    """Devuelve la sesión activa del servidor si existe el identificador de cookie."""
    if not sid:
        return None
    return SESIONES.get(sid)


def crear_sesion_servidor(token: str, email: str, roles: list[str], permisos: list[str]) -> str:
    """Almacena el token JWT y permisos en memoria del servidor y retorna el ID aleatorio de cookie."""
    sid = secrets.token_urlsafe(32)
    SESIONES[sid] = {
        "token": token,
        "email": email,
        "roles": roles,
        "permisos": permisos,
    }
    return sid


def cerrar_sesion_servidor(sid: str | None) -> None:
    """Elimina la sesión activa de memoria al hacer clic en 'Cerrar sesión' o al expirar el token."""
    if sid and sid in SESIONES:
        del SESIONES[sid]


def tiene_permiso(sesion: dict | None, ruta: str) -> bool:
    """Comprueba si el usuario conectado tiene asignada la ruta solicitada para mostrar enlaces en el menú."""
    if not sesion:
        return False
    return ruta in sesion.get("permisos", [])


async def llamar_api(
    metodo: str,
    endpoint: str,
    sesion: dict | None = None,
    json_body: dict | None = None,
) -> dict:
    """
    Ejecuta una petición HTTP asíncrona hacia la API REST en Python (/api/...).
    Inyecta automáticamente 'Authorization: Bearer <token>' si hay sesión activa.
    Retorna un sobre unificado para los templates:
      { "ok": bool, "status": int, "data": any, "mensaje": str, "errores": list, "error_conexion": bool, "sin_permiso": bool }
    """
    url = f"{URL_API}/{endpoint.lstrip('/')}"
    headers = {}
    if sesion and sesion.get("token"):
        headers["Authorization"] = f"Bearer {sesion['token']}"

    try:
        async with httpx.AsyncClient(timeout=15.0) as client:
            resp = await client.request(metodo.upper(), url, headers=headers, json=json_body)
    except httpx.RequestError as ex:
        # API apagada o inaccesible: el frontend sigue vivo mostrando aviso visual
        return {
            "ok": False,
            "status": 0,
            "data": None,
            "mensaje": f"No se pudo conectar con la API ({URL_API}). Verifica que esté encendida.",
            "errores": [],
            "error_conexion": True,
            "sin_permiso": False,
            "detalle": str(ex),
        }

    # 204 No Content (listado vacío o eliminación exitosa)
    if resp.status_code == 204:
        return {
            "ok": True,
            "status": 204,
            "data": None,
            "mensaje": "Operación completada correctamente.",
            "errores": [],
            "error_conexion": False,
            "sin_permiso": False,
        }

    # Intenta parsear respuesta JSON
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
        }

    # Respuestas de error HTTP (400, 401, 403, 404, 409, 422, 500)
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
