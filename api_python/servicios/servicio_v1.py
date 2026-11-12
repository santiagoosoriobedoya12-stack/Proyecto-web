# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 1 (LAS 6 TABLAS INDEPENDIENTES)
# Reglas de arquitectura:
# - No saben nada de HTTP (no devuelven JSONResponse ni códigos HTTP).
# - Validan reglas de negocio y lanzan excepciones tipadas de dominio.
# - ServicioUsuario aplica cifrado BCrypt costo 12 antes de guardar (RF1 v3).
# ==============================================================

import bcrypt
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from peticiones import (
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    EmpresaCrear, EmpresaReemplazo, EmpresaActualizar,
    PersonaCrear, PersonaReemplazo, PersonaActualizar,
    RolCrear, RolReemplazo, RolActualizar,
    RutaCrear, RutaReemplazo, RutaActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
)
from repositorios import (
    RepositorioProducto,
    RepositorioEmpresa,
    RepositorioPersona,
    RepositorioRol,
    RepositorioRuta,
    RepositorioUsuario,
)


def _validar_limite(limite: int) -> None:
    """Regla común: el parámetro 'limite' debe ser un entero positivo."""
    if limite <= 0:
        raise ValidacionNegocioExcepcion("El parámetro 'limite' debe ser un número entero mayor a 0.")


def _validar_texto_clave(valor: str, etiqueta: str) -> str:
    """Valida que una clave de texto obligatoria no sea vacía y elimina espacios extremos."""
    if not valor or not valor.strip():
        raise ValidacionNegocioExcepcion(f"El {etiqueta} no puede estar vacío.")
    return valor.strip()


def _hashear_contrasena(contrasena: str) -> str:
    """
    Versión 3 (RF1): Cifra la contraseña con algoritmo BCrypt factor de costo 12.
    Nunca almacena contraseñas en texto claro ni usa hashes rápidos vulnerables como SHA-256.
    """
    sal = bcrypt.gensalt(rounds=12)
    return bcrypt.hashpw(contrasena.encode("utf-8"), sal).decode("utf-8")


class ServicioProducto:
    """Reglas de negocio para 'producto'."""

    def __init__(self, repo: RepositorioProducto):
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
        cod = pet.codigo.strip()
        nom = pet.nombre.strip()
        self.repo.crear(cod, nom, pet.stock, pet.valorunitario)
        return {"codigo": cod, "nombre": nom, "stock": pet.stock, "valorunitario": pet.valorunitario}

    def reemplazar(self, codigo: str, pet: ProductoReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código del producto")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe un producto con codigo = {cod}")
        datos = {"nombre": pet.nombre.strip(), "stock": pet.stock, "valorunitario": pet.valorunitario}
        self.repo.actualizar_parcial(cod, datos)
        return {"codigo": cod, **datos}

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
    """Reglas de negocio para 'empresa'."""

    def __init__(self, repo: RepositorioEmpresa):
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
        cod, nom = pet.codigo.strip(), pet.nombre.strip()
        self.repo.crear(cod, nom)
        return {"codigo": cod, "nombre": nom}

    def reemplazar(self, codigo: str, pet: EmpresaReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código de la empresa")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una empresa con codigo = {cod}")
        datos = {"nombre": pet.nombre.strip()}
        self.repo.actualizar_parcial(cod, datos)
        return {"codigo": cod, **datos}

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
    """Reglas de negocio para 'persona'."""

    def __init__(self, repo: RepositorioPersona):
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
        cod, nom, email, tel = pet.codigo.strip(), pet.nombre.strip(), str(pet.email).strip(), pet.telefono.strip()
        self.repo.crear(cod, nom, email, tel)
        return {"codigo": cod, "nombre": nom, "email": email, "telefono": tel}

    def reemplazar(self, codigo: str, pet: PersonaReemplazo) -> dict:
        cod = _validar_texto_clave(codigo, "código de la persona")
        if not self.repo.obtener_por_clave(cod):
            raise NoEncontradoExcepcion(f"No existe una persona con codigo = {cod}")
        datos = {"nombre": pet.nombre.strip(), "email": str(pet.email).strip(), "telefono": pet.telefono.strip()}
        self.repo.actualizar_parcial(cod, datos)
        return {"codigo": cod, **datos}

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
    """Reglas de negocio para 'rol'."""

    def __init__(self, repo: RepositorioRol):
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
        return {"id": nuevo_id, "nombre": nom}

    def reemplazar(self, id_rol: int, pet: RolReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_rol):
            raise NoEncontradoExcepcion(f"No existe un rol con id = {id_rol}")
        datos = {"nombre": pet.nombre.strip()}
        self.repo.actualizar_parcial(id_rol, datos)
        return {"id": id_rol, **datos}

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
    """Reglas de negocio para 'ruta'."""

    def __init__(self, repo: RepositorioRuta):
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
        return {"id": nuevo_id, "ruta": rt, "descripcion": desc}

    def reemplazar(self, id_ruta: int, pet: RutaReemplazo) -> dict:
        if not self.repo.obtener_por_clave(id_ruta):
            raise NoEncontradoExcepcion(f"No existe una ruta con id = {id_ruta}")
        datos = {"ruta": pet.ruta.strip(), "descripcion": pet.descripcion.strip()}
        self.repo.actualizar_parcial(id_ruta, datos)
        return {"id": id_ruta, **datos}

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
    Reglas de negocio para 'usuario'.
    Incluye cumplimiento estricto de RF1 de la v3:
    - Al crear: cifra con BCrypt costo 12.
    - Al editar (PUT/PATCH): si la contraseña llega vacía, NO la sobreescribe; si llega nueva, la cifra con BCrypt.
    """

    def __init__(self, repo: RepositorioUsuario):
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
        # Nunca devuelve 'contrasena' en la respuesta JSON
        return {"email": em}

    def reemplazar(self, email: str, pet: UsuarioReemplazo) -> dict:
        em = _validar_texto_clave(email, "email del usuario")
        if not self.repo.obtener_por_clave(em):
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        # RF1 v3: Si llega vacía en edición, significa "déjela como está"
        if pet.contrasena and pet.contrasena.strip():
            hash_pwd = _hashear_contrasena(pet.contrasena)
            self.repo.actualizar_parcial(em, {"contrasena": hash_pwd})
        return {"email": em}

    def actualizar_parcial(self, email: str, pet: UsuarioActualizar) -> int:
        em = _validar_texto_clave(email, "email del usuario")
        if not self.repo.obtener_por_clave(em):
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        if pet.contrasena is None or not pet.contrasena.strip():
            # RF1 v3: campo vacío conserva la contraseña vigente sin error
            return 0
        hash_pwd = _hashear_contrasena(pet.contrasena)
        return self.repo.actualizar_parcial(em, {"contrasena": hash_pwd})

    def eliminar(self, email: str) -> int:
        em = _validar_texto_clave(email, "email del usuario")
        filas = self.repo.eliminar(em)
        if filas == 0:
            raise NoEncontradoExcepcion(f"No existe un usuario con email = {em}")
        return filas
