# ==============================================================
# ROUTERS HTTP VERSIÓN 1: LOS 6 RECURSOS INDEPENDIENTES
# Protegidos transversalmente por JWT + RBAC (Versión 3).
# Responden el sobre estándar: { "tabla", "limite", "total", "datos" } o 204 si vacío.
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_permiso
from peticiones import (
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    EmpresaCrear, EmpresaReemplazo, EmpresaActualizar,
    PersonaCrear, PersonaReemplazo, PersonaActualizar,
    RolCrear, RolReemplazo, RolActualizar,
    RutaCrear, RutaReemplazo, RutaActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
)
from repositorios import (
    RepositorioProducto, RepositorioEmpresa, RepositorioPersona,
    RepositorioRol, RepositorioRuta, RepositorioUsuario,
)
from servicios import (
    ServicioProducto, ServicioEmpresa, ServicioPersona,
    ServicioRol, ServicioRuta, ServicioUsuario,
)

# Instanciación de repositorios y servicios (Inyección simple de 3 capas)
_srv_producto = ServicioProducto(RepositorioProducto())
_srv_empresa = ServicioEmpresa(RepositorioEmpresa())
_srv_persona = ServicioPersona(RepositorioPersona())
_srv_rol = ServicioRol(RepositorioRol())
_srv_ruta = ServicioRuta(RepositorioRuta())
_srv_usuario = ServicioUsuario(RepositorioUsuario())


def _respuesta_lista(tabla: str, limite: int, datos: list[dict]):
    """Retorna 204 No Content si la tabla está vacía, o 200 OK con el sobre estándar."""
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": tabla, "limite": limite, "total": len(datos), "datos": datos}


# ---------------- 1. PRODUCTO (/api/producto) ----------------
router_producto = APIRouter(prefix="/api/producto", tags=["v1 - Producto"], dependencies=[Depends(exige_permiso("interfaz.productos"))])


@router_producto.get("")
def listar_productos(limite: int = 1000):
    return _respuesta_lista("producto", limite, _srv_producto.listar(limite))


@router_producto.get("/{codigo}")
def obtener_producto(codigo: str):
    return _srv_producto.obtener(codigo)


@router_producto.post("", status_code=status.HTTP_201_CREATED)
def crear_producto(pet: ProductoCrear):
    return _srv_producto.crear(pet)


@router_producto.put("/{codigo}")
def reemplazar_producto(codigo: str, pet: ProductoReemplazo):
    return _srv_producto.reemplazar(codigo, pet)


@router_producto.patch("/{codigo}")
def actualizar_producto(codigo: str, pet: ProductoActualizar):
    filas = _srv_producto.actualizar_parcial(codigo, pet)
    return {"mensaje": "Producto actualizado correctamente.", "filas_afectadas": filas}


@router_producto.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_producto(codigo: str):
    _srv_producto.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 2. EMPRESA (/api/empresa) ----------------
router_empresa = APIRouter(prefix="/api/empresa", tags=["v1 - Empresa"], dependencies=[Depends(exige_permiso("interfaz.empresas"))])


@router_empresa.get("")
def listar_empresas(limite: int = 1000):
    return _respuesta_lista("empresa", limite, _srv_empresa.listar(limite))


@router_empresa.get("/{codigo}")
def obtener_empresa(codigo: str):
    return _srv_empresa.obtener(codigo)


@router_empresa.post("", status_code=status.HTTP_201_CREATED)
def crear_empresa(pet: EmpresaCrear):
    return _srv_empresa.crear(pet)


@router_empresa.put("/{codigo}")
def reemplazar_empresa(codigo: str, pet: EmpresaReemplazo):
    return _srv_empresa.reemplazar(codigo, pet)


@router_empresa.patch("/{codigo}")
def actualizar_empresa(codigo: str, pet: EmpresaActualizar):
    filas = _srv_empresa.actualizar_parcial(codigo, pet)
    return {"mensaje": "Empresa actualizada correctamente.", "filas_afectadas": filas}


@router_empresa.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_empresa(codigo: str):
    _srv_empresa.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 3. PERSONA (/api/persona) ----------------
router_persona = APIRouter(prefix="/api/persona", tags=["v1 - Persona"], dependencies=[Depends(exige_permiso("interfaz.personas"))])


@router_persona.get("")
def listar_personas(limite: int = 1000):
    return _respuesta_lista("persona", limite, _srv_persona.listar(limite))


@router_persona.get("/{codigo}")
def obtener_persona(codigo: str):
    return _srv_persona.obtener(codigo)


@router_persona.post("", status_code=status.HTTP_201_CREATED)
def crear_persona(pet: PersonaCrear):
    return _srv_persona.crear(pet)


@router_persona.put("/{codigo}")
def reemplazar_persona(codigo: str, pet: PersonaReemplazo):
    return _srv_persona.reemplazar(codigo, pet)


@router_persona.patch("/{codigo}")
def actualizar_persona(codigo: str, pet: PersonaActualizar):
    filas = _srv_persona.actualizar_parcial(codigo, pet)
    return {"mensaje": "Persona actualizada correctamente.", "filas_afectadas": filas}


@router_persona.delete("/{codigo}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_persona(codigo: str):
    _srv_persona.eliminar(codigo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 4. ROL (/api/rol) ----------------
router_rol = APIRouter(prefix="/api/rol", tags=["v1 - Rol"], dependencies=[Depends(exige_permiso("interfaz.roles"))])


@router_rol.get("")
def listar_roles(limite: int = 1000):
    return _respuesta_lista("rol", limite, _srv_rol.listar(limite))


@router_rol.get("/{id_rol}")
def obtener_rol(id_rol: int):
    return _srv_rol.obtener(id_rol)


@router_rol.post("", status_code=status.HTTP_201_CREATED)
def crear_rol(pet: RolCrear):
    return _srv_rol.crear(pet)


@router_rol.put("/{id_rol}")
def reemplazar_rol(id_rol: int, pet: RolReemplazo):
    return _srv_rol.reemplazar(id_rol, pet)


@router_rol.patch("/{id_rol}")
def actualizar_rol(id_rol: int, pet: RolActualizar):
    filas = _srv_rol.actualizar_parcial(id_rol, pet)
    return {"mensaje": "Rol actualizado correctamente.", "filas_afectadas": filas}


@router_rol.delete("/{id_rol}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_rol(id_rol: int):
    _srv_rol.eliminar(id_rol)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 5. RUTA (/api/ruta) ----------------
router_ruta = APIRouter(prefix="/api/ruta", tags=["v1 - Ruta"], dependencies=[Depends(exige_permiso("interfaz.rutas"))])


@router_ruta.get("")
def listar_rutas(limite: int = 1000):
    return _respuesta_lista("ruta", limite, _srv_ruta.listar(limite))


@router_ruta.get("/{id_ruta}")
def obtener_ruta(id_ruta: int):
    return _srv_ruta.obtener(id_ruta)


@router_ruta.post("", status_code=status.HTTP_201_CREATED)
def crear_ruta(pet: RutaCrear):
    return _srv_ruta.crear(pet)


@router_ruta.put("/{id_ruta}")
def reemplazar_ruta(id_ruta: int, pet: RutaReemplazo):
    return _srv_ruta.reemplazar(id_ruta, pet)


@router_ruta.patch("/{id_ruta}")
def actualizar_ruta(id_ruta: int, pet: RutaActualizar):
    filas = _srv_ruta.actualizar_parcial(id_ruta, pet)
    return {"mensaje": "Ruta actualizada correctamente.", "filas_afectadas": filas}


@router_ruta.delete("/{id_ruta}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_ruta(id_ruta: int):
    _srv_ruta.eliminar(id_ruta)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 6. USUARIO (/api/usuario) ----------------
router_usuario = APIRouter(prefix="/api/usuario", tags=["v1 - Usuario"], dependencies=[Depends(exige_permiso("interfaz.usuarios"))])


@router_usuario.get("")
def listar_usuarios(limite: int = 1000):
    return _respuesta_lista("usuario", limite, _srv_usuario.listar(limite))


@router_usuario.get("/{email}")
def obtener_usuario(email: str):
    return _srv_usuario.obtener(email)


@router_usuario.post("", status_code=status.HTTP_201_CREATED)
def crear_usuario(pet: UsuarioCrear):
    return _srv_usuario.crear(pet)


@router_usuario.put("/{email}")
def reemplazar_usuario(email: str, pet: UsuarioReemplazo):
    return _srv_usuario.reemplazar(email, pet)


@router_usuario.patch("/{email}")
def actualizar_usuario(email: str, pet: UsuarioActualizar):
    filas = _srv_usuario.actualizar_parcial(email, pet)
    return {"mensaje": "Usuario actualizado correctamente.", "filas_afectadas": filas}


@router_usuario.delete("/{email}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_usuario(email: str):
    _srv_usuario.eliminar(email)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
