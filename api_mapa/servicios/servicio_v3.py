# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 3: ROLES, USUARIOS Y AUTENTICACIÓN JWT
# - Contraseñas cifradas con BCrypt costo 12.
# - Mismo error 401 si usuario no existe o contraseña falla.
# ==============================================================

from datetime import datetime, timedelta, timezone
import uuid
import bcrypt
import jwt
from config import JWT_AUDIENCE, JWT_EXPIRACION_MINUTOS, JWT_ISSUER, JWT_SECRET
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from modelos import Rol, Usuario, RolUsuario, Sesion
from peticiones import (
    RolCrear, RolReemplazo, RolActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
    RolUsuarioCrear, LoginPeticion,
)
from repositorios.interfaces import (
    IRepositorioRol,
    IRepositorioUsuario,
    IRepositorioRolUsuario,
    IRepositorioAcceso,
)
from servicios.servicio_v1 import hashear_password, validar_limite


class ServicioRol:
    def __init__(self, repo: IRepositorioRol):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_rol: int) -> dict:
        fila = self.repo.obtener_por_clave(id_rol)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe rol con id = {id_rol}")
        return fila

    def crear(self, pet: RolCrear) -> dict:
        desc = pet.descripcion.strip() if pet.descripcion and pet.descripcion.strip() else None
        nuevo_id = self.repo.crear(pet.nombre.strip(), desc)
        return Rol(nuevo_id, pet.nombre.strip(), desc).a_diccionario()

    def reemplazar(self, id_rol: int, pet: RolReemplazo) -> dict:
        self.obtener(id_rol)
        desc = pet.descripcion.strip() if pet.descripcion and pet.descripcion.strip() else None
        ent = Rol(id_rol, pet.nombre.strip(), desc)
        self.repo.actualizar_parcial(id_rol, {"nombre": ent.nombre, "descripcion": ent.descripcion})
        return ent.a_diccionario()

    def actualizar_parcial(self, id_rol: int, pet: RolActualizar) -> int:
        datos = {}
        if pet.nombre is not None:
            datos["nombre"] = pet.nombre.strip()
        if pet.descripcion is not None:
            datos["descripcion"] = pet.descripcion.strip() if pet.descripcion.strip() else None
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        self.obtener(id_rol)
        return self.repo.actualizar_parcial(id_rol, datos)

    def eliminar(self, id_rol: int) -> int:
        if self.repo.eliminar(id_rol) == 0:
            raise NoEncontradoExcepcion(f"No existe rol activo con id = {id_rol}")
        return 1


class ServicioUsuario:
    def __init__(self, repo: IRepositorioUsuario):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_usuario: int) -> dict:
        fila = self.repo.obtener_por_clave(id_usuario)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe usuario con id = {id_usuario}")
        return fila

    def crear(self, pet: UsuarioCrear) -> dict:
        pwd_hash = hashear_password(pet.password)
        nom = pet.nombre_completo.strip() if pet.nombre_completo and pet.nombre_completo.strip() else None
        nuevo_id = self.repo.crear(pet.username.strip(), pwd_hash, str(pet.email).strip(), nom)
        return Usuario(nuevo_id, pet.username.strip(), str(pet.email).strip(), nom).a_diccionario()

    def reemplazar(self, id_usuario: int, pet: UsuarioReemplazo) -> dict:
        self.obtener(id_usuario)
        nom = pet.nombre_completo.strip() if pet.nombre_completo and pet.nombre_completo.strip() else None
        datos = {
            "username": pet.username.strip(),
            "email": str(pet.email).strip(),
            "nombre_completo": nom,
        }
        if pet.password and pet.password.strip():
            datos["password"] = hashear_password(pet.password)
        self.repo.actualizar_parcial(id_usuario, datos)
        return Usuario(id_usuario, datos["username"], datos["email"], nom).a_diccionario()

    def actualizar_parcial(self, id_usuario: int, pet: UsuarioActualizar) -> int:
        datos = {}
        if pet.username is not None:
            datos["username"] = pet.username.strip()
        if pet.email is not None:
            datos["email"] = str(pet.email).strip()
        if pet.nombre_completo is not None:
            datos["nombre_completo"] = pet.nombre_completo.strip() if pet.nombre_completo.strip() else None
        if pet.password is not None and pet.password.strip():
            datos["password"] = hashear_password(pet.password)
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        self.obtener(id_usuario)
        return self.repo.actualizar_parcial(id_usuario, datos)

    def eliminar(self, id_usuario: int) -> int:
        if self.repo.eliminar(id_usuario) == 0:
            raise NoEncontradoExcepcion(f"No existe usuario activo con id = {id_usuario}")
        return 1


class ServicioRolUsuario:
    def __init__(self, repo: IRepositorioRolUsuario):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.obtener_todos()

    def obtener_por_usuario(self, usuario_id: int) -> list[dict]:
        return self.repo.obtener_por_usuario(usuario_id)

    def obtener_por_rol(self, rol_id: int) -> list[dict]:
        return self.repo.obtener_por_rol(rol_id)

    def crear(self, pet: RolUsuarioCrear) -> dict:
        self.repo.crear(pet.usuario_id, pet.rol_id)
        return RolUsuario(pet.usuario_id, pet.rol_id).a_diccionario()

    def eliminar(self, usuario_id: int, rol_id: int) -> int:
        if self.repo.eliminar(usuario_id, rol_id) == 0:
            raise NoEncontradoExcepcion(f"No existe relación usuario={usuario_id}, rol={rol_id}.")
        return 1


class ServicioLogin:
    """Autentica por username o email, valida hash BCrypt y firma JWT HS256."""

    def __init__(self, repo_acceso: IRepositorioAcceso):
        self.repo_acceso = repo_acceso

    def autenticar(self, pet: LoginPeticion) -> dict | None:
        identificador = (pet.usuario or pet.email or "").strip()
        if not identificador:
            return None

        usr = self.repo_acceso.obtener_usuario_login(identificador)
        if not usr:
            return None

        hash_guardado = usr.get("password") or ""
        try:
            valida = bcrypt.checkpw(pet.password.encode("utf-8"), hash_guardado.encode("utf-8"))
        except Exception:
            valida = False

        if not valida:
            return None

        roles = self.repo_acceso.obtener_roles_usuario(usr["id"])
        ahora = datetime.now(timezone.utc)
        expira = ahora + timedelta(minutes=max(1, JWT_EXPIRACION_MINUTOS))

        if len(JWT_SECRET) < 32:
            raise RuntimeError("JWT_SECRET debe tener al menos 32 caracteres.")

        payload = {
            "sub": str(usr["id"]),
            "usuario_id": usr["id"],
            "username": usr["username"],
            "email": usr["email"],
            "roles": roles,
            "jti": str(uuid.uuid4()),
            "iss": JWT_ISSUER,
            "aud": JWT_AUDIENCE,
            "iat": int(ahora.timestamp()),
            "exp": int(expira.timestamp()),
        }
        token = jwt.encode(payload, JWT_SECRET, algorithm="HS256")
        if isinstance(token, bytes):
            token = token.decode("utf-8")

        return Sesion(
            token=token,
            username=usr["username"],
            email=usr["email"],
            roles=roles,
        ).a_diccionario()
