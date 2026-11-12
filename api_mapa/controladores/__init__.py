# Paquete de Controladores HTTP REST (21 recursos organizados en v1, v2, v3)
from .controlador_v1 import (
    router_area_conocimiento,
    router_ods,
    router_area_aplicacion,
    router_termino_clave,
    router_linea_investigacion,
    router_aliado,
    router_tipo_producto,
    router_proyecto,
)
from .controlador_v2 import (
    router_docente,
    router_producto,
    router_desarrolla,
    router_aliado_proyecto,
    router_palabras_clave,
    router_ac_proyecto,
    router_proyecto_linea,
    router_ods_proyecto,
    router_aa_proyecto,
    router_docente_producto,
)
from .controlador_v3 import (
    router_login,
    router_rol,
    router_usuario,
    router_rol_usuario,
)
