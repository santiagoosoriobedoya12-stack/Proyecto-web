# ==============================================================
# AUTORIZACIÓN TRANSVERSAL VERSIÓN 3 (JWT + ROLES EN TIEMPO REAL)
# - Valida firma JWT HS256 con leeway=0 (-> 401 si falta, altera o vence).
# - Consulta en MariaDB los roles activos del usuario en CADA petición:
#   * Administrador: acceso total (módulo + gestión de usuarios y roles).
#   * Investigador: lectura y escritura en el módulo (sin gestión de usuarios/roles -> 403).
#   * Consulta: solo lectura GET (cualquier POST/PUT/PATCH/DELETE -> 403).
# ==============================================================

from fastapi import Depends
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
import jwt
from config import JWT_AUDIENCE, JWT_ISSUER, JWT_SECRET
from ensamblador import obtener_repositorio_acceso
from excepciones import NoAutenticadoExcepcion, SinPermisoExcepcion

_esquema_bearer = HTTPBearer(auto_error=False)
_repo_acceso = obtener_repositorio_acceso()


def exige_usuario_autenticado(
    credenciales: HTTPAuthorizationCredentials | None = Depends(_esquema_bearer),
) -> dict:
    """Valida el JWT Bearer y retorna {id, username, email, roles} consultados en BD."""
    if not credenciales or credenciales.scheme.lower() != "bearer" or not credenciales.credentials:
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    try:
        payload = jwt.decode(
            credenciales.credentials,
            JWT_SECRET,
            algorithms=["HS256"],
            issuer=JWT_ISSUER,
            audience=JWT_AUDIENCE,
            leeway=0,
        )
    except jwt.PyJWTError:
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    usuario_id = payload.get("usuario_id")
    if not usuario_id:
        raise NoAutenticadoExcepcion("No hay una sesion valida. Inicie sesion.")

    # Consulta en vivo los roles vigentes para que revocar un rol surta efecto inmediato
    roles_activos = _repo_acceso.obtener_roles_usuario(int(usuario_id))
    if not roles_activos:
        raise NoAutenticadoExcepcion("El usuario no tiene roles activos asignados.")

    return {
        "id": int(usuario_id),
        "username": payload.get("username", ""),
        "email": payload.get("email", ""),
        "roles": roles_activos,
    }


def exige_roles(*roles_permitidos: str):
    """Fábrica de dependencia RBAC: verifica si el usuario posee alguno de los roles requeridos."""
    permitidos_lower = {r.lower() for r in roles_permitidos}

    def _verificar(usuario: dict = Depends(exige_usuario_autenticado)) -> dict:
        roles_usuario = {r.lower() for r in usuario.get("roles", [])}
        if not (roles_usuario & permitidos_lower):
            raise SinPermisoExcepcion(
                "Su rol no tiene permiso para realizar esta operacion.",
                ruta=",".join(roles_permitidos),
            )
        return usuario

    return _verificar
