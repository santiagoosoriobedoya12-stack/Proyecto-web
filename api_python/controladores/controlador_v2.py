# ==============================================================
# CONTROLADORES HTTP VERSIÓN 2: LOS 6 RECURSOS CON FK + USUARIO CON ROLES
# Reciben sus servicios ya ensamblados desde ensamblador.py.
# Protegidos transversalmente por JWT + RBAC (Versión 3).
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_permiso
from controladores.controlador_v1 import _respuesta_lista
from ensamblador import (
    servicio_cliente,
    servicio_vendedor,
    servicio_factura,
    servicio_rol_usuario,
    servicio_rutarol,
    servicio_usuario_roles,
)
from peticiones import (
    ClienteCrear, ClienteReemplazo, ClienteActualizar,
    VendedorCrear, VendedorReemplazo, VendedorActualizar,
    FacturaCrear,
    RolUsuarioCrear, RutaRolCrear,
    UsuarioConRolesCrear, UsuarioConRolesActualizar,
)


# ---------------- 7. CLIENTE (/api/cliente) ----------------
router_cliente = APIRouter(prefix="/api/cliente", tags=["v2 - Cliente"], dependencies=[Depends(exige_permiso("interfaz.clientes"))])


@router_cliente.get("")
def listar_clientes(limite: int = 1000):
    return _respuesta_lista("cliente", limite, servicio_cliente.listar(limite))


@router_cliente.get("/{id_cliente}")
def obtener_cliente(id_cliente: int):
    return servicio_cliente.obtener(id_cliente)


@router_cliente.post("", status_code=status.HTTP_201_CREATED)
def crear_cliente(pet: ClienteCrear):
    return servicio_cliente.crear(pet)


@router_cliente.put("/{id_cliente}")
def reemplazar_cliente(id_cliente: int, pet: ClienteReemplazo):
    return servicio_cliente.reemplazar(id_cliente, pet)


@router_cliente.patch("/{id_cliente}")
def actualizar_cliente(id_cliente: int, pet: ClienteActualizar):
    filas = servicio_cliente.actualizar_parcial(id_cliente, pet)
    return {"mensaje": "Cliente actualizado correctamente.", "filas_afectadas": filas}


@router_cliente.delete("/{id_cliente}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_cliente(id_cliente: int):
    servicio_cliente.eliminar(id_cliente)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 8. VENDEDOR (/api/vendedor) ----------------
router_vendedor = APIRouter(prefix="/api/vendedor", tags=["v2 - Vendedor"], dependencies=[Depends(exige_permiso("interfaz.vendedores"))])


@router_vendedor.get("")
def listar_vendedores(limite: int = 1000):
    return _respuesta_lista("vendedor", limite, servicio_vendedor.listar(limite))


@router_vendedor.get("/{id_vendedor}")
def obtener_vendedor(id_vendedor: int):
    return servicio_vendedor.obtener(id_vendedor)


@router_vendedor.post("", status_code=status.HTTP_201_CREATED)
def crear_vendedor(pet: VendedorCrear):
    return servicio_vendedor.crear(pet)


@router_vendedor.put("/{id_vendedor}")
def reemplazar_vendedor(id_vendedor: int, pet: VendedorReemplazo):
    return servicio_vendedor.reemplazar(id_vendedor, pet)


@router_vendedor.patch("/{id_vendedor}")
def actualizar_vendedor(id_vendedor: int, pet: VendedorActualizar):
    filas = servicio_vendedor.actualizar_parcial(id_vendedor, pet)
    return {"mensaje": "Vendedor actualizado correctamente.", "filas_afectadas": filas}


@router_vendedor.delete("/{id_vendedor}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_vendedor(id_vendedor: int):
    servicio_vendedor.eliminar(id_vendedor)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 9 y 10. FACTURA MAESTRO-DETALLE (/api/factura) ----------------
router_factura = APIRouter(prefix="/api/factura", tags=["v2 - Factura"], dependencies=[Depends(exige_permiso("interfaz.facturas"))])


@router_factura.get("")
def listar_facturas():
    datos = servicio_factura.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "factura", "total": len(datos), "datos": datos}


@router_factura.get("/{numero}")
def obtener_factura(numero: int):
    return servicio_factura.obtener(numero)


@router_factura.post("", status_code=status.HTTP_201_CREATED)
def crear_factura(pet: FacturaCrear):
    return servicio_factura.crear(pet)


@router_factura.delete("/{numero}")
def anular_factura(numero: int):
    return servicio_factura.anular(numero)


# ---------------- 11. ROL_USUARIO (/api/rol-usuario) ----------------
router_rol_usuario = APIRouter(prefix="/api/rol-usuario", tags=["v2 - RolUsuario"], dependencies=[Depends(exige_permiso("interfaz.roles"))])


@router_rol_usuario.get("")
def listar_rol_usuario():
    datos = servicio_rol_usuario.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "rol_usuario", "total": len(datos), "datos": datos}


@router_rol_usuario.get("/usuario/{email}")
def obtener_roles_de_usuario(email: str):
    datos = servicio_rol_usuario.obtener_por_usuario(email)
    return {"usuario": email, "total": len(datos), "datos": datos}


@router_rol_usuario.get("/rol/{idrol}")
def obtener_usuarios_de_rol(idrol: int):
    datos = servicio_rol_usuario.obtener_por_rol(idrol)
    return {"idrol": idrol, "total": len(datos), "datos": datos}


@router_rol_usuario.post("", status_code=status.HTTP_201_CREATED)
def crear_rol_usuario(pet: RolUsuarioCrear):
    return servicio_rol_usuario.crear(pet)


@router_rol_usuario.delete("/{email}/{idrol}", status_code=status.HTTP_204_NO_CONTENT)
def eliminar_rol_usuario(email: str, idrol: int):
    servicio_rol_usuario.eliminar(email, idrol)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 12. RUTAROL (/api/rutarol) ----------------
router_rutarol = APIRouter(prefix="/api/rutarol", tags=["v2/v3 - RutaRol (Permisos)"], dependencies=[Depends(exige_permiso("interfaz.permisos"))])


@router_rutarol.get("")
def listar_rutarol():
    datos = servicio_rutarol.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "rutarol", "total": len(datos), "datos": datos}


@router_rutarol.post("", status_code=status.HTTP_201_CREATED)
def crear_rutarol(pet: RutaRolCrear):
    return servicio_rutarol.crear(pet)


@router_rutarol.delete("/{idruta}/{idrol}")
def eliminar_rutarol(idruta: int, idrol: int):
    return servicio_rutarol.eliminar(idruta, idrol)


# ---------------- RECURSO COMPUESTO: USUARIO CON ROLES (/api/usuario-con-roles) ----------------
router_usuario_roles = APIRouter(prefix="/api/usuario-con-roles", tags=["v2 - UsuarioConRoles"], dependencies=[Depends(exige_permiso("interfaz.usuarios"))])


@router_usuario_roles.get("")
def listar_usuarios_con_roles():
    datos = servicio_usuario_roles.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "usuario_con_roles", "total": len(datos), "datos": datos}


@router_usuario_roles.get("/{email}")
def consultar_usuario_con_roles(email: str):
    return servicio_usuario_roles.consultar(email)


@router_usuario_roles.post("", status_code=status.HTTP_201_CREATED)
def crear_usuario_con_roles(pet: UsuarioConRolesCrear):
    return servicio_usuario_roles.crear(pet)


@router_usuario_roles.put("/{email}")
def actualizar_usuario_con_roles(email: str, pet: UsuarioConRolesActualizar):
    return servicio_usuario_roles.actualizar(email, pet)


@router_usuario_roles.delete("/{email}")
def eliminar_usuario_con_roles(email: str):
    return servicio_usuario_roles.eliminar(email)
