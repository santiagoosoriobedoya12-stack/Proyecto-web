# ==============================================================
# CONTROLADORES HTTP VERSIÓN 3: LOGIN JWT + GESTIÓN DE USUARIOS/ROLES
# - POST /api/login (y /api/sesion) -> Público para obtener el JWT.
# - GET /api/permisos/mios -> Devuelve roles y secciones visibles del usuario conectado.
# - /api/rol, /api/usuario, /api/rol-usuario -> EXCLUSIVOS para rol 'Administrador'.
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_roles, exige_usuario_autenticado
from controladores.controlador_v1 import respuesta_lista
from ensamblador import (
    servicio_login,
    servicio_rol,
    servicio_usuario,
    servicio_rol_usuario,
)
from excepciones import NoAutenticadoExcepcion
from peticiones import (
    LoginPeticion,
    RolCrear, RolReemplazo, RolActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
    RolUsuarioCrear,
)

_SOLO_ADMIN = Depends(exige_roles("Administrador"))

router_login = APIRouter(tags=["v3 - Autenticación JWT"])
router_rol = APIRouter(prefix="/api/rol", tags=["v3 - Rol (Solo Admin)"], dependencies=[_SOLO_ADMIN])
router_usuario = APIRouter(prefix="/api/usuario", tags=["v3 - Usuario (Solo Admin)"], dependencies=[_SOLO_ADMIN])
router_rol_usuario = APIRouter(prefix="/api/rol-usuario", tags=["v3 - RolUsuario (Solo Admin)"], dependencies=[_SOLO_ADMIN])


@router_login.post("/api/login")
@router_login.post("/api/sesion")
def iniciar_sesion(pet: LoginPeticion):
    sesion = servicio_login.autenticar(pet)
    if not sesion:
        raise NoAutenticadoExcepcion("El usuario/correo o la contrasena no son correctos.")
    return sesion


@router_login.get("/api/permisos/mios")
def mis_permisos(usuario: dict = Depends(exige_usuario_autenticado)):
    roles = usuario.get("roles", [])
    es_admin = "Administrador" in roles
    puede_escribir = es_admin or ("Investigador" in roles)
    return {
        "usuario": usuario,
        "roles": roles,
        "puede_escribir": puede_escribir,
        "es_admin": es_admin,
    }


# ---------------- 19. ROL (/api/rol) ----------------
@router_rol.get("")
def listar_roles(limite: int = 1000):
    return respuesta_lista("rol", limite, servicio_rol.listar(limite))

@router_rol.get("/{id_rol}")
def obtener_rol(id_rol: int):
    return servicio_rol.obtener(id_rol)

@router_rol.post("", status_code=status.HTTP_201_CREATED)
def crear_rol(pet: RolCrear):
    return servicio_rol.crear(pet)

@router_rol.put("/{id_rol}")
def reemplazar_rol(id_rol: int, pet: RolReemplazo):
    return servicio_rol.reemplazar(id_rol, pet)

@router_rol.patch("/{id_rol}")
def actualizar_rol(id_rol: int, pet: RolActualizar):
    filas = servicio_rol.actualizar_parcial(id_rol, pet)
    return {"mensaje": "Rol actualizado.", "filas_afectadas": filas}

@router_rol.delete("/{id_rol}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_rol(id_rol: int):
    servicio_rol.eliminar(id_rol)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 20. USUARIO (/api/usuario) ----------------
@router_usuario.get("")
def listar_usuarios(limite: int = 1000):
    return respuesta_lista("usuario", limite, servicio_usuario.listar(limite))

@router_usuario.get("/{id_usuario}")
def obtener_usuario(id_usuario: int):
    return servicio_usuario.obtener(id_usuario)

@router_usuario.post("", status_code=status.HTTP_201_CREATED)
def crear_usuario(pet: UsuarioCrear):
    return servicio_usuario.crear(pet)

@router_usuario.put("/{id_usuario}")
def reemplazar_usuario(id_usuario: int, pet: UsuarioReemplazo):
    return servicio_usuario.reemplazar(id_usuario, pet)

@router_usuario.patch("/{id_usuario}")
def actualizar_usuario(id_usuario: int, pet: UsuarioActualizar):
    filas = servicio_usuario.actualizar_parcial(id_usuario, pet)
    return {"mensaje": "Usuario actualizado.", "filas_afectadas": filas}

@router_usuario.delete("/{id_usuario}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_usuario(id_usuario: int):
    servicio_usuario.eliminar(id_usuario)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 21. ROL_USUARIO (/api/rol-usuario) ----------------
@router_rol_usuario.get("")
def listar_rol_usuario():
    datos = servicio_rol_usuario.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "rol_usuario", "total": len(datos), "datos": datos}

@router_rol_usuario.get("/usuario/{usuario_id}")
def roles_de_usuario(usuario_id: int):
    datos = servicio_rol_usuario.obtener_por_usuario(usuario_id)
    return {"usuario_id": usuario_id, "total": len(datos), "datos": datos}

@router_rol_usuario.get("/rol/{rol_id}")
def usuarios_de_rol(rol_id: int):
    datos = servicio_rol_usuario.obtener_por_rol(rol_id)
    return {"rol_id": rol_id, "total": len(datos), "datos": datos}

@router_rol_usuario.post("", status_code=status.HTTP_201_CREATED)
def asignar_rol(pet: RolUsuarioCrear):
    return servicio_rol_usuario.crear(pet)

@router_rol_usuario.delete("/{usuario_id}/{rol_id}", status_code=status.HTTP_204_NO_CONTENT)
def revocar_rol(usuario_id: int, rol_id: int):
    servicio_rol_usuario.eliminar(usuario_id, rol_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
