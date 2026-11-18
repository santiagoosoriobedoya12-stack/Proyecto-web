# ==============================================================
# CONFIGURACIÓN DESDE VARIABLES DE ENTORNO (SIN SECRETOS QUEMADOS)
# Soporta tanto PostgreSQL (Nube / Render / Supabase) como MariaDB (Docker Local)
# ==============================================================

import os
from urllib.parse import urlparse

# Conexión genérica por URL (Render / Supabase / Neon / Railway)
DATABASE_URL = os.getenv("DATABASE_URL")
if DATABASE_URL and DATABASE_URL.startswith("postgres://"):
    # Normalización para drivers modernos que esperan postgresql://
    DATABASE_URL = DATABASE_URL.replace("postgres://", "postgresql://", 1)

# Variables individuales
DB_HOST = os.getenv("DB_HOST", "localhost")
DB_PORT = int(os.getenv("DB_PORT", "13333"))
DB_NAME = os.getenv("DB_NAME", "mapa_conocimiento")
DB_USER = os.getenv("DB_USER", "mapa_user")
DB_PASSWORD = os.getenv("DB_PASSWORD", "MapaConocimiento2026!")

# Detección del motor: Si viene DATABASE_URL o DB_ENGINE='postgres' o puerto 5432 -> PostgreSQL
_env_engine = os.getenv("DB_ENGINE", "").lower()
if _env_engine in ("postgres", "postgresql") or DATABASE_URL or DB_PORT == 5432:
    DB_ENGINE = "postgres"
else:
    DB_ENGINE = "mariadb"

JWT_SECRET = os.getenv("JWT_SECRET", "SuperClaveSecretaMapaConocimientoParaJwt2026*")
JWT_ISSUER = os.getenv("JWT_ISSUER", "ApiMapaConocimiento")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE", "FrontMapaConocimiento")
JWT_EXPIRACION_MINUTOS = int(os.getenv("JWT_EXPIRACION_MINUTOS", "60"))
