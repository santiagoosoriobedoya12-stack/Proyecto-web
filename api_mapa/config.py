# ==============================================================
# CONFIGURACIÓN DESDE VARIABLES DE ENTORNO (SIN SECRETOS QUEMADOS)
# Lee credenciales de MariaDB y firma JWT desde el entorno del contenedor / .env.
# ==============================================================

import os

DB_HOST = os.getenv("DB_HOST", "localhost")
DB_PORT = int(os.getenv("DB_PORT", "13333"))
DB_NAME = os.getenv("DB_NAME", "mapa_conocimiento")
DB_USER = os.getenv("DB_USER", "mapa_user")
DB_PASSWORD = os.getenv("DB_PASSWORD", "MapaConocimiento2026!")

JWT_SECRET = os.getenv("JWT_SECRET", "SuperClaveSecretaMapaConocimientoParaJwt2026*")
JWT_ISSUER = os.getenv("JWT_ISSUER", "ApiMapaConocimiento")
JWT_AUDIENCE = os.getenv("JWT_AUDIENCE", "FrontMapaConocimiento")
JWT_EXPIRACION_MINUTOS = int(os.getenv("JWT_EXPIRACION_MINUTOS", "60"))
