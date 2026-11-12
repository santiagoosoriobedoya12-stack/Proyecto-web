# Paquete de Controladores HTTP REST (v1, v2, v3)
from .controlador_v1 import (
    router_producto,
    router_empresa,
    router_persona,
    router_rol,
    router_ruta,
    router_usuario,
)
from .controlador_v2 import (
    router_cliente,
    router_vendedor,
    router_factura,
    router_rol_usuario,
    router_rutarol,
    router_usuario_roles,
)
from .controlador_sesion import router_sesion, router_permisos
