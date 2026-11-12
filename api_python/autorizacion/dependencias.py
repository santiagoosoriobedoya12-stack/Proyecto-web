# ==============================================================
# AUTORIZACIÓN TRANSVERSAL VERSIÓN 3 (JWT + RBAC DINÁMICO)
# Equivalente en Python al middleware JwtBearer + [ExigePermiso("...")]:
# 1. exige_usuario_autenticado -> Valida firma, emisor, audiencia y expiración del JWT (401 si falla).
# 2. exige_permiso(nombre_ruta) -> Consulta 'verificar_acceso_ruta' en SQL Server en CADA petición (403 si falla).
# ==============================================================

from fastapi import Depends
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import jwt
from config import JWT_KEY, JWT_ISSUER, JWT_AUDIENCE
from excepciones import NoAutenticadoExcepcion, SinPermisoExcepcion
from repositorios import RepositorioAcceso

# auto_error=False permite responder nuestro JSON personalizado 401 en lugar del 403 genérico por defecto de FastAPI
_esquema_bearer = HTTPBearer(auto_error=False)
_repo_acceso = RepositorioAcceso()


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
        # leeway=0 equivale a ClockSkew = TimeSpan.Zero: sin margen de tolerancia al expirar
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
    Fábrica de dependencia FastAPI (equivalente a [ExigePermiso("interfaz.x")]).
    Verifica en vivo mediante el Stored Procedure 'verificar_acceso_ruta' si el rol del usuario
    tiene autorizada la ruta indicada. Quitar un permiso surte efecto inmediato (Criterio 7).
    """
    def _verificar(email: str = Depends(exige_usuario_autenticado)) -> str:
        # 1. Traduce el nombre lógico de ruta al ID numérico en la tabla 'ruta'
        id_ruta = _repo_acceso.obtener_id_ruta_por_nombre(nombre_ruta)
        if id_ruta is None:
            # Si la ruta no existe registrada en BD, falla cerrado por seguridad (HTTP 403)
            raise SinPermisoExcepcion("Su rol no tiene permiso para esta operacion.", ruta=nombre_ruta)

        # 2. Invoca el SP oficial 'verificar_acceso_ruta' en SQL Server
        tiene_acceso = _repo_acceso.verificar_acceso_ruta(email, id_ruta)
        if not tiene_acceso:
            raise SinPermisoExcepcion("Su rol no tiene permiso para esta operacion.", ruta=nombre_ruta)

        return email

    return _verificar
