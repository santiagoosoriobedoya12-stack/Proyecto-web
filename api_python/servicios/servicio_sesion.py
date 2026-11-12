# ==============================================================
# SERVICIO VERSIÓN 3: AUTENTICACIÓN Y EMISIÓN DE TOKENS JWT
# Reglas de seguridad (2_spec.md v3):
# 1. Compara la contraseña contra el hash BCrypt guardado en 'usuario'.
# 2. Devuelve None tanto si no existe el correo como si la clave falla -> mismo 401.
# 3. El token JWT transporta únicamente 'sub' (email) y 'roles' (NUNCA permisos).
# ==============================================================

from datetime import datetime, timedelta, timezone
import uuid
import bcrypt
import jwt
from config import JWT_KEY, JWT_ISSUER, JWT_AUDIENCE, JWT_EXPIRACION_HORAS
from peticiones import SesionCrear
from repositorios import RepositorioAcceso, RepositorioUsuario
from servicios.servicio_v1 import _hashear_contrasena


class ServicioSesion:
    """Servicio de dominio encargado de verificar credenciales y emitir JWT firmado."""

    def __init__(self, repo_acceso: RepositorioAcceso, repo_usuario: RepositorioUsuario):
        self.repo_acceso = repo_acceso
        self.repo_usuario = repo_usuario

    def iniciar_sesion(self, peticion: SesionCrear) -> dict | None:
        """
        Verifica correo y contraseña. Retorna diccionario con el token JWT si son válidos,
        o None en cualquier fallo (correo inexistente o clave errónea).
        """
        email_limpio = peticion.email.strip()
        # 1. Buscar credenciales en base de datos (email + hash de contrasena)
        cred = self.repo_acceso.obtener_credenciales_por_email(email_limpio)
        if not cred:
            return None

        hash_guardado = cred.get("contrasena", "") or ""
        es_valida = False

        # 2. Verificar contraseña usando BCrypt costo 12
        try:
            if hash_guardado.startswith(("$2a$", "$2b$", "$2y$")):
                es_valida = bcrypt.checkpw(
                    peticion.contrasena.encode("utf-8"),
                    hash_guardado.encode("utf-8"),
                )
            else:
                # Resiliencia por si hubiera clave heredada en claro: valida y migra a BCrypt 12
                es_valida = (peticion.contrasena == hash_guardado)
                if es_valida:
                    nuevo_hash = _hashear_contrasena(peticion.contrasena)
                    self.repo_usuario.actualizar_parcial(email_limpio, {"contrasena": nuevo_hash})
        except Exception:
            es_valida = False

        if not es_valida:
            return None

        # 3. Consultar los roles asignados al usuario
        roles = self.repo_acceso.obtener_roles_por_email(email_limpio)

        # 4. Construir payload del JWT (sin permisos de rutas para evitar desfases al revocar permisos)
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

        # 5. Firmar token con algoritmo simétrico HMAC-SHA256
        token = jwt.encode(payload, JWT_KEY, algorithm="HS256")
        if isinstance(token, bytes):
            token = token.decode("utf-8")

        return {
            "estado": 200,
            "mensaje": "Sesion iniciada.",
            "token": token,
            "email": email_limpio,
            "roles": roles,
        }
