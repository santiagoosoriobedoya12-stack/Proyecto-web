# ==============================================================
# ROUTERS VERSIÓN 3: SESIÓN (JWT) Y PERMISOS PROPIOS
# - POST /api/sesion -> Público (sin token) para autenticar con email + contrasena.
# - GET /api/permisos/mios -> Exige JWT válido, retorna rutas autorizadas para el menú.
# ==============================================================

from fastapi import APIRouter, Depends
from autorizacion import exige_usuario_autenticado
from excepciones import NoAutenticadoExcepcion
from peticiones import SesionCrear
from repositorios import RepositorioAcceso, RepositorioUsuario
from servicios import ServicioSesion

_repo_acceso = RepositorioAcceso()
_srv_sesion = ServicioSesion(_repo_acceso, RepositorioUsuario())

router_sesion = APIRouter(prefix="/api/sesion", tags=["v3 - Sesión / Login"])
router_permisos = APIRouter(prefix="/api/permisos", tags=["v3 - Permisos Propios"])


@router_sesion.post("")
def iniciar_sesion(peticion: SesionCrear):
    """
    Autentica al usuario verificando su hash BCrypt.
    Criterio 2 v3: Devuelve exactamente el mismo mensaje 401 si falla correo o clave.
    """
    sesion = _srv_sesion.iniciar_sesion(peticion)
    if not sesion:
        raise NoAutenticadoExcepcion("El correo o la contrasena no son correctos.")
    return sesion


@router_permisos.get("/mios")
def obtener_mis_permisos(email: str = Depends(exige_usuario_autenticado)):
    """
    Devuelve la lista de rutas permitidas para el usuario del token JWT.
    Solo arma la vista del menú en el frontend; la seguridad real reside en cada ruta API.
    """
    rutas = _repo_acceso.obtener_rutas_permitidas_por_email(email)
    return {"email": email, "total": len(rutas), "datos": rutas}
