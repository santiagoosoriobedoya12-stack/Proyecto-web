# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 2 (6 TABLAS CON FK + USUARIO CON ROLES)
# - Traduce las colecciones de entrada al JSON exacto que esperan los Stored Procedures:
#   * Factura -> [{"fkcodproducto": "P001", "cantidad": 2}] (sin precios ni totales!)
#   * UsuarioConRoles -> [{"fkidrol": 1}, {"fkidrol": 3}] (clave obligatoria 'fkidrol' para OPENJSON)
# - En UsuarioConRoles cifra nuevas contraseñas con BCrypt costo 12.
# ==============================================================

import json
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from peticiones import (
    ClienteCrear, ClienteReemplazo, ClienteActualizar,
    VendedorCrear, VendedorReemplazo, VendedorActualizar,
    FacturaCrear,
    RolUsuarioCrear, RutaRolCrear,
    UsuarioConRolesCrear, UsuarioConRolesActualizar,
)
from repositorios import (
    RepositorioCliente,
    RepositorioVendedor,
    RepositorioFactura,
    RepositorioRolUsuario,
    RepositorioRutaRol,
    RepositorioUsuarioConRoles,
)
from servicios.servicio_v1 import _validar_limite, _validar_texto_clave, _hashear_contrasena


class ServicioCliente:
    """Reglas de negocio para 'cliente' (FK obligatoria a persona, opcional a empresa)."""

    def __init__(self, repo: RepositorioCliente):
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
        # Si fkcodempresa viene vacía "", se normaliza a None (NULL en SQL) para evitar 409 falso (C2 v2)
        emp = pet.fkcodempresa.strip() if pet.fkcodempresa and pet.fkcodempresa.strip() else None
        nuevo_id = self.repo.crear(pet.credito, per, emp)
        return {"id": nuevo_id, "credito": pet.credito, "fkcodpersona": per, "fkcodempresa": emp}

    def reemplazar(self, id_cliente: int, pet: ClienteReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_cliente):
            raise NoEncontradoExcepcion(f"No existe un cliente con id = {id_cliente}")
        emp = pet.fkcodempresa.strip() if pet.fkcodempresa and pet.fkcodempresa.strip() else None
        datos = {"credito": pet.credito, "fkcodpersona": pet.fkcodpersona.strip(), "fkcodempresa": emp}
        self.repo.actualizar_parcial(id_cliente, datos)
        return {"id": id_cliente, **datos}

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

    def __init__(self, repo: RepositorioVendedor):
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
        return {"id": nuevo_id, "carnet": pet.carnet, "direccion": dir_limpia, "fkcodpersona": per}

    def reemplazar(self, id_vendedor: int, pet: VendedorReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_vendedor):
            raise NoEncontradoExcepcion(f"No existe un vendedor con id = {id_vendedor}")
        datos = {"carnet": pet.carnet, "direccion": pet.direccion.strip(), "fkcodpersona": pet.fkcodpersona.strip()}
        self.repo.actualizar_parcial(id_vendedor, datos)
        return {"id": id_vendedor, **datos}

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
    """
    Orquesta la facturación Maestro-Detalle.
    Toda la matemática contable (precio, subtotal, total, descuento y retorno de stock)
    se ejecuta dentro de SQL Server mediante Stored Procedures y Triggers.
    """

    def __init__(self, repo: RepositorioFactura):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.listar()

    def obtener(self, numero: int) -> dict:
        return self.repo.obtener_por_numero(numero)

    def crear(self, pet: FacturaCrear) -> dict:
        # Serializa únicamente código de producto y cantidad para OPENJSON en sp_insertar_factura_y_productosporfactura
        productos_json = json.dumps(
            [{"fkcodproducto": item.fkcodproducto.strip(), "cantidad": item.cantidad} for item in pet.productos]
        )
        return self.repo.crear(pet.fkidcliente, pet.fkidvendedor, productos_json)

    def anular(self, numero: int) -> dict:
        return self.repo.anular(numero)


class ServicioRolUsuario:
    """Reglas de negocio para la tabla puente 'rol_usuario'."""

    def __init__(self, repo: RepositorioRolUsuario):
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

    def __init__(self, repo: RepositorioRutaRol):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.listar()

    def crear(self, pet: RutaRolCrear) -> dict:
        return self.repo.crear(pet.fkidruta, pet.fkidrol)

    def eliminar(self, idruta: int, idrol: int) -> dict:
        return self.repo.eliminar(idruta, idrol)


class ServicioUsuarioConRoles:
    """
    Reglas de dominio para 'usuario-con-roles'.
    1. Convierte la lista [1, 3] a [{"fkidrol": 1}, {"fkidrol": 3}] para OPENJSON.
    2. Cifra contraseñas con BCrypt costo 12; en PUT, si llega vacía envía NULL al SP para conservar la clave.
    """

    def __init__(self, repo: RepositorioUsuarioConRoles):
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
