# Módulo estándar para leer variables de entorno definidas en docker-compose.yml o el sistema
import os


# ==============================================================
# CONFIGURACIÓN DE CONEXIÓN A SQL SERVER 2022
# ==============================================================
# Permite leer las variables individuales de entorno o usar valores locales por defecto
DB_HOST = os.getenv("DB_HOST", "localhost")
DB_PORT = int(os.getenv("DB_PORT", "11463"))
DB_NAME = os.getenv("DB_NAME", "bdfacturas_sqlserver_local")
DB_USER = os.getenv("DB_USER", "sa")
DB_PASSWORD = os.getenv("DB_PASSWORD", "Paradigmas123!")

# También soporta parsear ConnectionStrings__SqlServer si viene en formato ADO.NET
_conn_str = os.getenv("ConnectionStrings__SqlServer", "")
if _conn_str:
    # Recorre cada segmento separado por ';' (Server=..., Database=..., User Id=..., Password=...)
    for parte in _conn_str.split(";"):
        if "=" in parte:
            k, v = parte.split("=", 1)
            kl = k.strip().lower()
            v = v.strip()
            if kl == "server":
                if "," in v:
                    h, p = v.split(",", 1)
                    DB_HOST = h.strip()
                    DB_PORT = int(p.strip())
                else:
                    DB_HOST = v
            elif kl == "database":
                DB_NAME = v
            elif kl in ("user id", "uid"):
                DB_USER = v
            elif kl in ("password", "pwd"):
                DB_PASSWORD = v

# ==============================================================
# CONFIGURACIÓN DE SEGURIDAD JWT (VERSIÓN 3)
# ==============================================================
# Clave secreta simétrica de mínimo 32 caracteres para firmar tokens HMAC-SHA256
JWT_KEY = os.getenv("Jwt__Key", "SuperClaveSecretaUltraSeguraParaFirmarJwtDeFacturas2026*")
# Emisor oficial del token
JWT_ISSUER = os.getenv("Jwt__Issuer", "ApiFacturas")
# Audiencia autorizada para consumir el token
JWT_AUDIENCE = os.getenv("Jwt__Audience", "FrontFacturas")
# Tiempo de vigencia del token en horas (por defecto 1 hora)
JWT_EXPIRACION_HORAS = int(os.getenv("Jwt__ExpiracionHoras", "1"))
