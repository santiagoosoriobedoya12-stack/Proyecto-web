# Paquete de Repositorios: interfaces ABC e implementaciones MariaDB
from .interfaces import (
    IRepositorioAreaConocimiento,
    IRepositorioOds,
    IRepositorioAreaAplicacion,
    IRepositorioTerminoClave,
    IRepositorioLineaInvestigacion,
    IRepositorioAliado,
    IRepositorioTipoProducto,
    IRepositorioProyecto,
    IRepositorioDocente,
    IRepositorioProducto,
    IRepositorioDesarrolla,
    IRepositorioPuente,
    IRepositorioRol,
    IRepositorioUsuario,
    IRepositorioRolUsuario,
    IRepositorioAcceso,
)
from .repositorio_v1 import (
    RepositorioAreaConocimientoMariaDB,
    RepositorioOdsMariaDB,
    RepositorioAreaAplicacionMariaDB,
    RepositorioTerminoClaveMariaDB,
    RepositorioLineaInvestigacionMariaDB,
    RepositorioAliadoMariaDB,
    RepositorioTipoProductoMariaDB,
    RepositorioProyectoMariaDB,
)
from .repositorio_v2 import (
    RepositorioDocenteMariaDB,
    RepositorioProductoMariaDB,
    RepositorioDesarrollaMariaDB,
    RepositorioAliadoProyectoMariaDB,
    RepositorioPalabrasClaveMariaDB,
    RepositorioAcProyectoMariaDB,
    RepositorioProyectoLineaMariaDB,
    RepositorioOdsProyectoMariaDB,
    RepositorioAaProyectoMariaDB,
    RepositorioDocenteProductoMariaDB,
)
from .repositorio_v3 import (
    RepositorioRolMariaDB,
    RepositorioUsuarioMariaDB,
    RepositorioRolUsuarioMariaDB,
    RepositorioAccesoMariaDB,
)
