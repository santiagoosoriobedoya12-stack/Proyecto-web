# ==============================================================
# CONTROLADORES HTTP VERSIÓN 1: LOS 6 RECURSOS INDEPENDIENTES
# - Solo saben de HTTP (rutas, verbos, códigos y sobre JSON estándar).
# - Consumen los servicios ya ensamblados desde ensamblador.py.
# - Protegidos transversalmente por JWT + RBAC (Versión 3).
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_permiso
from ensamblador import (
    servicio_producto,
    servicio_empresa,
    servicio_persona,
    servicio_rol,
    servicio_ruta,
    servicio_usuario,
)
from peticiones import (
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    EmpresaCrear, EmpresaReemplazo, EmpresaActualizar,
    PersonaCrear, PersonaReemplazo, PersonaActualizar,
    RolCrear, RolReemplazo, RolActualizar,
    RutaCrear, RutaReemplazo, RutaActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
)


def _respuesta_lista(tabla: str, limite: int, datos: list[dict]):
    """Retorna HTTP 204 No Content si la tabla está vacía, o 200 OK con el sobre estándar."""
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": tabla, "limite": limite, "total": len(datos), "datos": datos}


# ---------------- 1. PRODUCTO (/api/producto) ----------------
router_producto = APIRouter(prefix="/api/producto", tags=["v1 - Producto"], dependencies=[Depends(exige_permiso("interfaz.productos"))])


@router_producto.get("")
def listar_productos(limite: int = 1000):
    return _respuesta_lista("producto", limite, servicio_producto.listar(limite))


@router_producto.get("/{codigo}")
def obtener_producto(codigo: str):
    return servicio_producto.obtener(codigo)


@router_producto.post("", status_code=status.HTTP_201_CREATED)
def crear_producto(pet: ProductoCrear):
    return servicio_producto.crear(pet)


@router_producto.put("/{codigo}")
def reemplazar_producto(codigo: str, pet: ProductoReemplazo):
    return servicio_producto.reemplazar(codigo, pet)


@router_producto.patch("/{codigo}")
def actualizar_producto(codigo: str, pet: ProductoActualizar):
    filas = servicio_producto.actualizar_parcial(codigo, pet)
    return {"mensaje": "Producto actualizado correctamente.", "filas_afectadas": filas}


@router_producto.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_producto(codigo: str):
    servicio_producto.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 2. EMPRESA (/api/empresa) ----------------
router_empresa = APIRouter(prefix="/api/empresa", tags=["v1 - Empresa"], dependencies=[Depends(exige_permiso("interfaz.empresas"))])


@router_empresa.get("")
def listar_empresas(limite: int = 1000):
    return _respuesta_lista("empresa", limite, servicio_empresa.listar(limite))


@router_empresa.get("/{codigo}")
def obtener_empresa(codigo: str):
    return servicio_empresa.obtener(codigo)


@router_empresa.post("", status_code=status.HTTP_201_CREATED)
def crear_empresa(pet: EmpresaCrear):
    return servicio_empresa.crear(pet)


@router_empresa.put("/{codigo}")
def reemplazar_empresa(codigo: str, pet: EmpresaReemplazo):
    return servicio_empresa.reemplazar(codigo, pet)


@router_empresa.patch("/{codigo}")
def actualizar_empresa(codigo: str, pet: EmpresaActualizar):
    filas = servicio_empresa.actualizar_parcial(codigo, pet)
    return {"mensaje": "Empresa actualizada correctamente.", "filas_afectadas": filas}


@router_empresa.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_empresa(codigo: str):
    servicio_empresa.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 3. PERSONA (/api/persona) ----------------
router_persona = APIRouter(prefix="/api/persona", tags=["v1 - Persona"], dependencies=[Depends(exige_permiso("interfaz.personas"))])


@router_persona.get("")
def listar_personas(limite: int = 1000):
    return _respuesta_lista("persona", limite, servicio_persona.listar(limite))


@router_persona.get("/{codigo}")
def obtener_persona(codigo: str):
    return servicio_persona.obtener(codigo)


@router_persona.post("", status_code=status.HTTP_201_CREATED)
def crear_persona(pet: PersonaCrear):
    return servicio_persona.crear(pet)


@router_persona.put("/{codigo}")
def reemplazar_persona(codigo: str, pet: PersonaReemplazo):
    return servicio_persona.reemplazar(codigo, pet)


@router_persona.patch("/{codigo}")
def actualizar_persona(codigo: str, pet: PersonaActualizar):
    filas = servicio_persona.actualizar_parcial(codigo, pet)
    return {"mensaje": "Persona actualizada correctamente.", "filas_afectadas": filas}


@router_persona.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_persona(codigo: str):
    servicio_persona.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 4. ROL (/api/rol) ----------------
router_rol = APIRouter(prefix="/api/rol", tags=["v1 - Rol"], dependencies=[Depends(exige_permiso("interfaz.roles"))])


@router_rol.get("")
def listar_roles(limite: int = 1000):
    return _respuesta_lista("rol", limite, servicio_rol.listar(limite))


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
    return {"mensaje": "Rol actualizado correctamente.", "filas_afectadas": filas}


@router_rol.delete("/{id_rol}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_rol(id_rol: int):
    servicio_rol.eliminar(id_rol)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 5. RUTA (/api/ruta) ----------------
router_ruta = APIRouter(prefix="/api/ruta", tags=["v1 - Ruta"], dependencies=[Depends(exige_permiso("interfaz.rutas"))])


@router_ruta.get("")
def listar_rutas(limite: int = 1000):
    return _respuesta_lista("ruta", limite, servicio_ruta.listar(limite))


@router_ruta.get("/{id_ruta}")
def obtener_ruta(id_ruta: int):
    return servicio_ruta.obtener(id_ruta)


@router_ruta.post("", status_code=status.HTTP_201_CREATED)
def crear_ruta(pet: RutaCrear):
    return servicio_ruta.crear(pet)


@router_ruta.put("/{id_ruta}")
def reemplazar_ruta(id_ruta: int, pet: RutaReemplazo):
    return servicio_ruta.reemplazar(id_ruta, pet)


@router_ruta.patch("/{id_ruta}")
def actualizar_ruta(id_ruta: int, pet: RutaActualizar):
    filas = servicio_ruta.actualizar_parcial(id_ruta, pet)
    return {"mensaje": "Ruta actualizada correctamente.", "filas_afectadas": filas}


@router_ruta.delete("/{id_ruta}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_ruta(id_ruta: int):
    servicio_ruta.eliminar(id_ruta)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 6. USUARIO (/api/usuario) ----------------
router_usuario = APIRouter(prefix="/api/usuario", tags=["v1 - Usuario"], dependencies=[Depends(exige_permiso("interfaz.usuarios"))])


@router_usuario.get("")
def listar_usuarios(limite: int = 1000):
    return _respuesta_lista("usuario", limite, servicio_usuario.listar(limite))


@router_usuario.get("/{email}")
def obtener_usuario(email: str):
    return servicio_usuario.obtener(email)


@router_usuario.post("", status_code=status.HTTP_201_CREATED)
def crear_usuario(pet: UsuarioCrear):
    return servicio_usuario.crear(pet)


@router_usuario.put("/{email}")
def reemplazar_usuario(email: str, pet: UsuarioReemplazo):
    return servicio_usuario.reemplazar(email, pet)


@router_usuario.patch("/{email}")
def actualizar_usuario(email: str, pet: UsuarioActualizar):
    filas = servicio_usuario.actualizar_parcial(email, pet)
    return {"mensaje": "Usuario actualizado correctamente.", "filas_afectadas": filas}


@router_usuario.delete("/{email}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_usuario(email: str):
    servicio_usuario.eliminar(email)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
