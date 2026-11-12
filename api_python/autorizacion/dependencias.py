# ==============================================================
# AUTORIZACIÓN TRANSVERSAL VERSIÓN 3 (JWT + RBAC DINÁMICO)
# Utiliza el contrato abstracto IRepositorioAcceso entregado por el ensamblador
# para validar la firma JWT (401) y consultar 'verificar_acceso_ruta' (403).
# ==============================================================

from fastapi import Depends
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import jwt
from config import JWT_KEY, JWT_ISSUER, JWT_AUDIENCE
from ensamblador import obtener_repositorio_acceso
from excepciones import NoAutenticadoExcepcion, SinPermisoExcepcion

_esquema_bearer = HTTPBearer(auto_error=False)
_repo_acceso = obtener_repositorio_acceso()


def exige_usuario_autenticado(
    credenciales: HTTPAuthorizationCredentials | None = Depends(_esquema_bearer),
) -> str:
    """
    Extrae y valida el token Bearer JWT de la cabecera 'Authorization'.
    Retorna el email del usuario autenticado o lanza NoAutenticadoExcepcion (HTTP 401).
    """
    if not credenciales or credenciales.scheme.lower() != "bearer" or not credenciales.credentials:
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    token = credenciales.credentials
    try:
        payload = jwt.decode(
            token,
            JWT_KEY,
            algorithms=["HS256"],
            issuer=JWT_ISSUER,
            audience=JWT_AUDIENCE,
            leeway=0,
        )
    except jwt.PyJWTError:
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    email = payload.get("sub") or payload.get("email")
    if not email or not str(email).strip():
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    return str(email).strip()


def exige_permiso(nombre_ruta: str):
    """
    Fábrica de dependencia FastAPI que valida en vivo el permiso RBAC contra SQL Server
    invocando 'verificar_acceso_ruta'. Revocar un permiso surte efecto inmediato.
    """
    def _verificar(email: str = Depends(exige_usuario_autenticado)) -> str:
        id_ruta = _repo_acceso.obtener_id_ruta_por_nombre(nombre_ruta)
        if id_ruta is None:
            raise SinPermisoExcepcion("Su rol no tiene permiso para esta operacion.", ruta=nombre_ruta)

        tiene_acceso = _repo_acceso.verificar_acceso_ruta(email, id_ruta)
        if not tiene_acceso:
            raise SinPermisoExcepcion("Su rol no tiene permiso para esta operacion.", ruta=nombre_ruta)

        return email

    return _verificar
