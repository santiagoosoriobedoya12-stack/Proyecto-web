# Paquete de Servicios de Dominio (v1, v2, v3)
from .servicio_v1 import (
    ServicioAreaConocimiento,
    ServicioOds,
    ServicioAreaAplicacion,
    ServicioTerminoClave,
    ServicioLineaInvestigacion,
    ServicioAliado,
    ServicioTipoProducto,
    ServicioProyecto,
    hashear_password,
)
from .servicio_v2 import (
    ServicioDocente,
    ServicioProducto,
    ServicioDesarrolla,
    ServicioPuente,
)
from .servicio_v3 import (
    ServicioRol,
    ServicioUsuario,
    ServicioRolUsuario,
    ServicioLogin,
)
