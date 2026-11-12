# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 1 (LAS 6 TABLAS INDEPENDIENTES)
# Reglas de P.O.O. y Arquitectura en Capas:
# - Composición sobre herencia: cada servicio RECIBE la interfaz IRepositorio*
#   por constructor (nunca instancia Repositorio*SqlServer directamente).
# - No conocen HTTP ni pymssql; lanzan excepciones de dominio tipadas.
# - ServicioUsuario aplica BCrypt costo 12 antes de entregar el hash al repositorio.
# ==============================================================

import bcrypt
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from modelos import Producto, Empresa, Persona, Rol, Ruta, Usuario
from peticiones import (
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    EmpresaCrear, EmpresaReemplazo, EmpresaActualizar,
    PersonaCrear, PersonaReemplazo, PersonaActualizar,
    RolCrear, RolReemplazo, RolActualizar,
    RutaCrear, RutaReemplazo, RutaActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
)
from repositorios.interfaces import (
    IRepositorioProducto,
    IRepositorioEmpresa,
    IRepositorioPersona,
    IRepositorioRol,
    IRepositorioRuta,
    IRepositorioUsuario,
)


def _validar_limite(limite: int) -> None:
    """Regla de negocio común: el parámetro 'limite' debe ser un entero positivo."""
    if limite <= 0:
        raise ValidacionNegocioExcepcion("El parámetro 'limite' debe ser un número entero mayor a 0.")


def _validar_texto_clave(valor: str, etiqueta: str) -> str:
    """Valida que una clave de texto obligatoria no sea vacía y limpia espacios extremos."""
    if not valor or not valor.strip():
        raise ValidacionNegocioExcepcion(f"El {etiqueta} no puede estar vacío.")
    return valor.strip()


def _hashear_contrasena(contrasena: str) -> str:
    """Cifra la contraseña con algoritmo BCrypt factor de costo 12 y salt aleatoria."""
    sal = bcrypt.gensalt(rounds=12)
    return bcrypt.hashpw(contrasena.encode("utf-8"), sal).decode("utf-8")


class ServicioProducto:
    """Reglas de negocio para 'producto' desacopladas mediante IRepositorioProducto."""

    def __init__(self, repo: IRepositorioProducto):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, codigo: str) -> dict:
        cod = _validar_texto_clave(codigo, "código del producto")
        prod = self.repo.obtener_por_clave(cod)
        if not prod:
            raise NoEncontradoExcepcion(f"No existe un producto con codigo = {cod}")
        return prod

    def crear(self, pet: ProductoCrear) -> dict:
        entidad = Producto(
            codigo=pet.codigo.strip(),
            nombre=pet.nombre.strip(),
            stock=pet.stock,
            valorunitario=pet.valorunitario,
        )
        self.repo.crear(entidad.codigo, entidad.nombre, entidad.stock, entidad.valorunitario)
        return entidad.a_diccionario()

    def reemplazar(self, codigo: str, pet: ProductoReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código del producto")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe un producto con codigo = {cod}")
        entidad = Producto(
            codigo=cod,
            nombre=pet.nombre.strip(),
            stock=pet.stock,
            valorunitario=pet.valorunitario,
        )
        self.repo.actualizar_parcial(cod, {"nombre": entidad.nombre, "stock": entidad.stock, "valorunitario": entidad.valorunitario})
        return entidad.a_diccionario()

    def actualizar_parcial(self, codigo: str, pet: ProductoActualizar) -> int:
        cod = _validar_texto_clave(codigo, "código del producto")
        datos = {}
        if pet.nombre is not None:
            datos["nombre"] = pet.nombre.strip()
        if pet.stock is not None:
            datos["stock"] = pet.stock
        if pet.valorunitario is not None:
            datos["valorunitario"] = pet.valorunitario
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe un producto con codigo = {cod}")
        return self.repo.actualizar_parcial(cod, datos)

    def eliminar(self, codigo: str) -> int:
        cod = _validar_texto_clave(codigo, "código del producto")
        filas = self.repo.eliminar(cod)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un producto con codigo = {cod}")
        return filas


class ServicioEmpresa:
    """Reglas de negocio para 'empresa' desacopladas mediante IRepositorioEmpresa."""

    def __init__(self, repo: IRepositorioEmpresa):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, codigo: str) -> dict:
        cod = _validar_texto_clave(codigo, "código de la empresa")
        emp = self.repo.obtener_por_clave(cod)
        if not emp:
            raise NoEncontradoExcepcion(f"No existe una empresa con codigo = {cod}")
        return emp

    def crear(self, pet: EmpresaCrear) -> dict:
        entidad = Empresa(codigo=pet.codigo.strip(), nombre=pet.nombre.strip())
        self.repo.crear(entidad.codigo, entidad.nombre)
        return entidad.a_diccionario()

    def reemplazar(self, codigo: str, pet: EmpresaReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código de la empresa")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una empresa con codigo = {cod}")
        entidad = Empresa(codigo=cod, nombre=pet.nombre.strip())
        self.repo.actualizar_parcial(cod, {"nombre": entidad.nombre})
        return entidad.a_diccionario()

    def actualizar_parcial(self, codigo: str, pet: EmpresaActualizar) -> int:
        cod = _validar_texto_clave(codigo, "código de la empresa")
        if pet.nombre is None:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una empresa con codigo = {cod}")
        return self.repo.actualizar_parcial(cod, {"nombre": pet.nombre.strip()})

    def eliminar(self, codigo: str) -> int:
        cod = _validar_texto_clave(codigo, "código de la empresa")
        filas = self.repo.eliminar(cod)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe una empresa con codigo = {cod}")
        return filas


class ServicioPersona:
    """Reglas de negocio para 'persona' desacopladas mediante IRepositorioPersona."""

    def __init__(self, repo: IRepositorioPersona):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, codigo: str) -> dict:
        cod = _validar_texto_clave(codigo, "código de la persona")
        per = self.repo.obtener_por_clave(cod)
        if not per:
            raise NoEncontradoExcepcion(f"No existe una persona con codigo = {cod}")
        return per

    def crear(self, pet: PersonaCrear) -> dict:
        entidad = Persona(
            codigo=pet.codigo.strip(),
            nombre=pet.nombre.strip(),
            email=str(pet.email).strip(),
            telefono=pet.telefono.strip(),
        )
        self.repo.crear(entidad.codigo, entidad.nombre, entidad.email, entidad.telefono)
        return entidad.a_diccionario()

    def reemplazar(self, codigo: str, pet: PersonaReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código de la persona")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una persona con codigo = {cod}")
        entidad = Persona(
            codigo=cod,
            nombre=pet.nombre.strip(),
            email=str(pet.email).strip(),
            telefono=pet.telefono.strip(),
        )
        self.repo.actualizar_parcial(cod, {"nombre": entidad.nombre, "email": entidad.email, "telefono": entidad.telefono})
        return entidad.a_diccionario()

    def actualizar_parcial(self, codigo: str, pet: PersonaActualizar) -> int:
        cod = _validar_texto_clave(codigo, "código de la persona")
        datos = {}
        if pet.nombre is not None:
            datos["nombre"] = pet.nombre.strip()
        if pet.email is not None:
            datos["email"] = str(pet.email).strip()
        if pet.telefono is not None:
            datos["telefono"] = pet.telefono.strip()
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una persona con codigo = {cod}")
        return self.repo.actualizar_parcial(cod, datos)

    def eliminar(self, codigo: str) -> int:
        cod = _validar_texto_clave(codigo, "código de la persona")
        filas = self.repo.eliminar(cod)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe una persona con codigo = {cod}")
        return filas


class ServicioRol:
    """Reglas de negocio para 'rol' desacopladas mediante IRepositorioRol."""

    def __init__(self, repo: IRepositorioRol):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_rol: int) -> dict:
        rol = self.repo.obtener_por_clave(id_rol)
        if not rol:
            raise NoEncontradoExcepcion(f"No existe un rol con id = {id_rol}")
        return rol

    def crear(self, pet: RolCrear) -> dict:
        nom = pet.nombre.strip()
        nuevo_id = self.repo.crear(nom)
        return Rol(id=nuevo_id, nombre=nom).a_diccionario()

    def reemplazar(self, id_rol: int, pet: RolReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_rol):
            raise NoEncontradoExcepcion(f"No existe un rol con id = {id_rol}")
        nom = pet.nombre.strip()
        self.repo.actualizar_parcial(id_rol, {"nombre": nom})
        return Rol(id=id_rol, nombre=nom).a_diccionario()

    def actualizar_parcial(self, id_rol: int, pet: RolActualizar) -> int:
        if pet.nombre is None:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(id_rol):
            raise NoEncontradoExcepcion(f"No existe un rol con id = {id_rol}")
        return self.repo.actualizar_parcial(id_rol, {"nombre": pet.nombre.strip()})

    def eliminar(self, id_rol: int) -> int:
        filas = self.repo.eliminar(id_rol)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un rol con id = {id_rol}")
        return filas


class ServicioRuta:
    """Reglas de negocio para 'ruta' desacopladas mediante IRepositorioRuta."""

    def __init__(self, repo: IRepositorioRuta):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_ruta: int) -> dict:
        ruta = self.repo.obtener_por_clave(id_ruta)
        if not ruta:
            raise NoEncontradoExcepcion(f"No existe una ruta con id = {id_ruta}")
        return ruta

    def crear(self, pet: RutaCrear) -> dict:
        rt, desc = pet.ruta.strip(), pet.descripcion.strip()
        nuevo_id = self.repo.crear(rt, desc)
        return Ruta(id=nuevo_id, ruta=rt, descripcion=desc).a_diccionario()

    def reemplazar(self, id_ruta: int, pet: RutaReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_ruta):
            raise NoEncontradoExcepcion(f"No existe una ruta con id = {id_ruta}")
        rt, desc = pet.ruta.strip(), pet.descripcion.strip()
        self.repo.actualizar_parcial(id_ruta, {"ruta": rt, "descripcion": desc})
        return Ruta(id=id_ruta, ruta=rt, descripcion=desc).a_diccionario()

    def actualizar_parcial(self, id_ruta: int, pet: RutaActualizar) -> int:
        datos = {}
        if pet.ruta is not None:
            datos["ruta"] = pet.ruta.strip()
        if pet.descripcion is not None:
            datos["descripcion"] = pet.descripcion.strip()
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_por_clave(id_ruta):
            raise NoEncontradoExcepcion(f"No existe una ruta con id = {id_ruta}")
        return self.repo.actualizar_parcial(id_ruta, datos)

    def eliminar(self, id_ruta: int) -> int:
        filas = self.repo.eliminar(id_ruta)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe una ruta con id = {id_ruta}")
        return filas


class ServicioUsuario:
    """
    Reglas de negocio para 'usuario' desacopladas mediante IRepositorioUsuario.
    Cumple RF1 v3: cifra con BCrypt costo 12; en edición, contraseña vacía conserva la actual.
    """

    def __init__(self, repo: IRepositorioUsuario):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        _validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, email: str) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        usr = self.repo.obtener_por_clave(em)
        if not usr:
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        return usr

    def crear(self, pet: UsuarioCrear) -> dict:
        em = str(pet.email).strip()
        hash_pwd = _hashear_contrasena(pet.contrasena)
        self.repo.crear(em, hash_pwd)
        return Usuario(email=em).a_diccionario()

    def reemplazar(self, email: str, pet: UsuarioReemplazo) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        if not self.repo.obtener_por_clave(em):
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        if pet.contrasena and pet.contrasena.strip():
            hash_pwd = _hashear_contrasena(pet.contrasena)
            self.repo.actualizar_parcial(em, {"contrasena": hash_pwd})
        return Usuario(email=em).a_diccionario()

    def actualizar_parcial(self, email: str, pet: UsuarioActualizar) -> int:
        em = _validar_texto_clave(email, "email del usuario")
        if not self.repo.obtener_por_clave(em):
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        if pet.contrasena is None or not pet.contrasena.strip():
            return 0
        hash_pwd = _hashear_contrasena(pet.contrasena)
        return self.repo.actualizar_parcial(em, {"contrasena": hash_pwd})

    def eliminar(self, email: str) -> int:
        em = _validar_texto_clave(email, "email del usuario")
        filas = self.repo.eliminar(em)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        return filas
