# ==============================================================
# SERVICIO VERSIÓN 3: AUTENTICACIÓN Y EMISIÓN DE TOKENS JWT
# - Compone IRepositorioAcceso e IRepositorioUsuario por constructor (P.O.O.).
# - Verifica hash BCrypt y construye la entidad Sesion con JWT firmado HS256.
# ==============================================================

from datetime import datetime, timedelta, timezone
import uuid
import bcrypt
import jwt
from config import JWT_KEY, JWT_ISSUER, JWT_AUDIENCE, JWT_EXPIRACION_HORAS
from modelos import Sesion
from peticiones import SesionCrear
from repositorios.interfaces import IRepositorioAcceso, IRepositorioUsuario
from servicios.servicio_v1 import _hashear_contrasena


class ServicioSesion:
    """Servicio de dominio encargado de verificar credenciales y emitir JWT firmado."""

    def __init__(self, repo_acceso: IRepositorioAcceso, repo_usuario: IRepositorioUsuario):
        self.repo_acceso = repo_acceso
        self.repo_usuario = repo_usuario

    def iniciar_sesion(self, peticion: SesionCrear) -> dict | None:
        email_limpio = peticion.email.strip()
        cred = self.repo_acceso.obtener_credenciales_por_email(email_limpio)
        if not cred:
            return None

        hash_guardado = cred.get("contrasena", "") or ""
        es_valida = False

        try:
            if hash_guardado.startswith(("$2a$", "$2b$", "$2y$")):
                es_valida = bcrypt.checkpw(
                    peticion.contrasena.encode("utf-8"),
                    hash_guardado.encode("utf-8"),
                )
            else:
                es_valida = (peticion.contrasena == hash_guardado)
                if es_valida:
                    nuevo_hash = _hashear_contrasena(peticion.contrasena)
                    self.repo_usuario.actualizar_parcial(email_limpio, {"contrasena": nuevo_hash})
        except Exception:
            es_valida = False

        if not es_valida:
            return None

        roles = self.repo_acceso.obtener_roles_por_email(email_limpio)
        ahora = datetime.now(timezone.utc)
        expira = ahora + timedelta(hours=max(1, JWT_EXPIRACION_HORAS))

        if len(JWT_KEY) < 32:
            raise RuntimeError("Jwt:Key debe tener al menos 32 caracteres para HS256.")

        payload = {
            "sub": email_limpio,
            "email": email_limpio,
            "roles": roles,
            "jti": str(uuid.uuid4()),
            "iss": JWT_ISSUER,
            "aud": JWT_AUDIENCE,
            "iat": int(ahora.timestamp()),
            "exp": int(expira.timestamp()),
        }

        token = jwt.encode(payload, JWT_KEY, algorithm="HS256")
        if isinstance(token, bytes):
            token = token.decode("utf-8")

        return Sesion(token=token, email=email_limpio, roles=roles).a_diccionario()
