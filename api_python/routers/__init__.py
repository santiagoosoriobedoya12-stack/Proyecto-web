# Paquete de Routers HTTP REST (Controladores FastAPI por versión: v1, v2, v3)
from .router_v1 import (
    router_producto,
    router_empresa,
    router_persona,
    router_rol,
    router_ruta,
    router_usuario,
)
from .router_v2 import (
    router_cliente,
    router_vendedor,
    router_factura,
    router_rol_usuario,
    router_rutarol,
    router_usuario_roles,
)
from .router_sesion import router_sesion, router_permisos
