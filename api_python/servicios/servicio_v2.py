# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 2 (6 TABLAS CON FK + USUARIO CON ROLES)
# - Reciben sus repositorios por constructor tipados contra IRepositorio* (ABC).
# - Traducen arreglos a JSON para OPENJSON sin calcular precios/totales de factura.
# ==============================================================

import json
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from modelos import Cliente, Vendedor, RolUsuario
from peticiones import (
    ClienteCrear, ClienteReemplazo, ClienteActualizar,
    VendedorCrear, VendedorReemplazo, VendedorActualizar,
    FacturaCrear,
    RolUsuarioCrear, RutaRolCrear,
    UsuarioConRolesCrear, UsuarioConRolesActualizar,
)
from repositorios.interfaces import (
    IRepositorioCliente,
    IRepositorioVendedor,
    IRepositorioFactura,
    IRepositorioRolUsuario,
    IRepositorioRutaRol,
    IRepositorioUsuarioConRoles,
)
from servicios.servicio_v1 import _validar_limite, _validar_texto_clave, _hashear_contrasena


class ServicioCliente:
    """Reglas de negocio para 'cliente' (FK obligatoria a persona, opcional a empresa)."""

    def __init__(self, repo: IRepositorioCliente):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_cliente: int) -> dict:
        cli = self.repo.obtener_por_clave(id_cliente)
        if not cli:
            raise NoEncontradoExcepcion(f"No existe un cliente con id = {id_cliente}")
        return cli

    def crear(self, pet: ClienteCrear) -> dict:
        per = pet.fkcodpersona.strip()
        # Si fkcodempresa llega vacía "", se normaliza a None (NULL en SQL) para personas naturales
        emp = pet.fkcodempresa.strip() if pet.fkcodempresa and pet.fkcodempresa.strip() else None
        nuevo_id = self.repo.crear(pet.credito, per, emp)
        return Cliente(id=nuevo_id, credito=pet.credito, fkcodpersona=per, fkcodempresa=emp).a_diccionario()

    def reemplazar(self, id_cliente: int, pet: ClienteReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_cliente):
            raise NoEncontradoExcepcion(f"No existe un cliente con id = {id_cliente}")
        emp = pet.fkcodempresa.strip() if pet.fkcodempresa and pet.fkcodempresa.strip() else None
        per = pet.fkcodpersona.strip()
        self.repo.actualizar_parcial(id_cliente, {"credito": pet.credito, "fkcodpersona": per, "fkcodempresa": emp})
        return Cliente(id=id_cliente, credito=pet.credito, fkcodpersona=per, fkcodempresa=emp).a_diccionario()

    def actualizar_parcial(self, id_cliente: int, pet: ClienteActualizar) -> int:
        datos = {}
        if pet.credito is not None:
            datos["credito"] = pet.credito
        if pet.fkcodpersona is not None:
            datos["fkcodpersona"] = pet.fkcodpersona.strip()
        if pet.fkcodempresa is not None:
            datos["fkcodempresa"] = pet.fkcodempresa.strip() if pet.fkcodempresa.strip() else None
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(id_cliente):
            raise NoEncontradoExcepcion(f"No existe un cliente con id = {id_cliente}")
        return self.repo.actualizar_parcial(id_cliente, datos)

    def eliminar(self, id_cliente: int) -> int:
        filas = self.repo.eliminar(id_cliente)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un cliente con id = {id_cliente}")
        return filas


class ServicioVendedor:
    """Reglas de negocio para 'vendedor' (FK obligatoria a persona)."""

    def __init__(self, repo: IRepositorioVendedor):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_vendedor: int) -> dict:
        ven = self.repo.obtener_por_clave(id_vendedor)
        if not ven:
            raise NoEncontradoExcepcion(f"No existe un vendedor con id = {id_vendedor}")
        return ven

    def crear(self, pet: VendedorCrear) -> dict:
        dir_limpia = pet.direccion.strip()
        per = pet.fkcodpersona.strip()
        nuevo_id = self.repo.crear(pet.carnet, dir_limpia, per)
        return Vendedor(id=nuevo_id, carnet=pet.carnet, direccion=dir_limpia, fkcodpersona=per).a_diccionario()

    def reemplazar(self, id_vendedor: int, pet: VendedorReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_vendedor):
            raise NoEncontradoExcepcion(f"No existe un vendedor con id = {id_vendedor}")
        dir_limpia = pet.direccion.strip()
        per = pet.fkcodpersona.strip()
        self.repo.actualizar_parcial(id_vendedor, {"carnet": pet.carnet, "direccion": dir_limpia, "fkcodpersona": per})
        return Vendedor(id=id_vendedor, carnet=pet.carnet, direccion=dir_limpia, fkcodpersona=per).a_diccionario()

    def actualizar_parcial(self, id_vendedor: int, pet: VendedorActualizar) -> int:
        datos = {}
        if pet.carnet is not None:
            datos["carnet"] = pet.carnet
        if pet.direccion is not None:
            datos["direccion"] = pet.direccion.strip()
        if pet.fkcodpersona is not None:
            datos["fkcodpersona"] = pet.fkcodpersona.strip()
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(id_vendedor):
            raise NoEncontradoExcepcion(f"No existe un vendedor con id = {id_vendedor}")
        return self.repo.actualizar_parcial(id_vendedor, datos)

    def eliminar(self, id_vendedor: int) -> int:
        filas = self.repo.eliminar(id_vendedor)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un vendedor con id = {id_vendedor}")
        return filas


class ServicioFactura:
    """Orquesta la facturación Maestro-Detalle delegando totales y stock en SQL Server."""

    def __init__(self, repo: IRepositorioFactura):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.listar()

    def obtener(self, numero: int) -> dict:
        return self.repo.obtener_por_numero(numero)

    def crear(self, pet: FacturaCrear) -> dict:
        productos_json = json.dumps(
            [{"fkcodproducto": item.fkcodproducto.strip(), "cantidad": item.cantidad} for item in pet.productos]
        )
        return self.repo.crear(pet.fkidcliente, pet.fkidvendedor, productos_json)

    def anular(self, numero: int) -> dict:
        return self.repo.anular(numero)


class ServicioRolUsuario:
    """Reglas de negocio para la tabla puente 'rol_usuario'."""

    def __init__(self, repo: IRepositorioRolUsuario):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.obtener_todos()

    def obtener_por_usuario(self, email: str) -> list[dict]:
        em = _validar_texto_clave(email, "email del usuario")
        return self.repo.obtener_por_usuario(em)

    def obtener_por_rol(self, idrol: int) -> list[dict]:
        return self.repo.obtener_por_rol(idrol)

    def crear(self, pet: RolUsuarioCrear) -> dict:
        em = str(pet.fkemail).strip()
        self.repo.crear(em, pet.fkidrol)
        return {"fkemail": em, "fkidrol": pet.fkidrol}

    def eliminar(self, email: str, idrol: int) -> int:
        em = _validar_texto_clave(email, "email del usuario")
        filas = self.repo.eliminar(em, idrol)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe asignación del rol {idrol} para el usuario '{em}'.")
        return filas


class ServicioRutaRol:
    """Reglas de negocio para la tabla puente de permisos 'rutarol'."""

    def __init__(self, repo: IRepositorioRutaRol):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.listar()

    def crear(self, pet: RutaRolCrear) -> dict:
        return self.repo.crear(pet.fkidruta, pet.fkidrol)

    def eliminar(self, idruta: int, idrol: int) -> dict:
        return self.repo.eliminar(idruta, idrol)


class ServicioUsuarioConRoles:
    """Reglas de dominio para el recurso compuesto 'usuario-con-roles' (OPENJSON + BCrypt)."""

    def __init__(self, repo: IRepositorioUsuarioConRoles):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.listar()

    def consultar(self, email: str) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        return self.repo.consultar(em)

    def crear(self, pet: UsuarioConRolesCrear) -> dict:
        em = str(pet.email).strip()
        hash_pwd = _hashear_contrasena(pet.contrasena)
        roles_json = json.dumps([{"fkidrol": id_rol} for id_rol in pet.roles])
        return self.repo.crear(em, hash_pwd, roles_json)

    def actualizar(self, email: str, pet: UsuarioConRolesActualizar) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        hash_pwd = _hashear_contrasena(pet.contrasena) if pet.contrasena and pet.contrasena.strip() else None
        roles_json = json.dumps([{"fkidrol": id_rol} for id_rol in pet.roles])
        return self.repo.actualizar(em, hash_pwd, roles_json)

    def eliminar(self, email: str) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        return self.repo.eliminar(em)
