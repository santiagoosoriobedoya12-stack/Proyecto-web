# ==============================================================
# ENSAMBLADOR DE DEPENDENCIAS (ÚNICO LUGAR QUE INSTANCIA CONCRETOS)
# Instancia todos los repositorios MariaDB y los inyecta en sus servicios.
# ==============================================================

from repositorios import (
    RepositorioAreaConocimientoMariaDB,
    RepositorioOdsMariaDB,
    RepositorioAreaAplicacionMariaDB,
    RepositorioTerminoClaveMariaDB,
    RepositorioLineaInvestigacionMariaDB,
    RepositorioAliadoMariaDB,
    RepositorioTipoProductoMariaDB,
    RepositorioProyectoMariaDB,
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
    RepositorioRolMariaDB,
    RepositorioUsuarioMariaDB,
    RepositorioRolUsuarioMariaDB,
    RepositorioAccesoMariaDB,
)
from repositorios.interfaces import IRepositorioAcceso
from servicios import (
    ServicioAreaConocimiento,
    ServicioOds,
    ServicioAreaAplicacion,
    ServicioTerminoClave,
    ServicioLineaInvestigacion,
    ServicioAliado,
    ServicioTipoProducto,
    ServicioProyecto,
    ServicioDocente,
    ServicioProducto,
    ServicioDesarrolla,
    ServicioPuente,
    ServicioRol,
    ServicioUsuario,
    ServicioRolUsuario,
    ServicioLogin,
)

# v1 — 8 servicios sin FK
servicio_area_conocimiento = ServicioAreaConocimiento(RepositorioAreaConocimientoMariaDB())
servicio_ods = ServicioOds(RepositorioOdsMariaDB())
servicio_area_aplicacion = ServicioAreaAplicacion(RepositorioAreaAplicacionMariaDB())
servicio_termino_clave = ServicioTerminoClave(RepositorioTerminoClaveMariaDB())
servicio_linea_investigacion = ServicioLineaInvestigacion(RepositorioLineaInvestigacionMariaDB())
servicio_aliado = ServicioAliado(RepositorioAliadoMariaDB())
servicio_tipo_producto = ServicioTipoProducto(RepositorioTipoProductoMariaDB())
servicio_proyecto = ServicioProyecto(RepositorioProyectoMariaDB())

# v2 — 10 servicios con FK y puentes M:N
servicio_docente = ServicioDocente(RepositorioDocenteMariaDB())
servicio_producto = ServicioProducto(RepositorioProductoMariaDB())
servicio_desarrolla = ServicioDesarrolla(RepositorioDesarrollaMariaDB())

servicio_aliado_proyecto = ServicioPuente(RepositorioAliadoProyectoMariaDB(), "aliado", "proyecto")
servicio_palabras_clave = ServicioPuente(RepositorioPalabrasClaveMariaDB(), "proyecto", "termino_clave")
servicio_ac_proyecto = ServicioPuente(RepositorioAcProyectoMariaDB(), "proyecto", "area_conocimiento")
servicio_proyecto_linea = ServicioPuente(RepositorioProyectoLineaMariaDB(), "proyecto", "linea_investigacion")
servicio_ods_proyecto = ServicioPuente(RepositorioOdsProyectoMariaDB(), "proyecto", "ods")
servicio_aa_proyecto = ServicioPuente(RepositorioAaProyectoMariaDB(), "proyecto", "area_aplicacion")
servicio_docente_producto = ServicioPuente(RepositorioDocenteProductoMariaDB(), "docente", "producto")

# v3 — Seguridad, roles y usuarios
_repo_acceso = RepositorioAccesoMariaDB()
servicio_rol = ServicioRol(RepositorioRolMariaDB())
servicio_usuario = ServicioUsuario(RepositorioUsuarioMariaDB())
servicio_rol_usuario = ServicioRolUsuario(RepositorioRolUsuarioMariaDB())
servicio_login = ServicioLogin(_repo_acceso)


def obtener_repositorio_acceso() -> IRepositorioAcceso:
    """Entrega la abstracción IRepositorioAcceso para las dependencias JWT/RBAC."""
    return _repo_acceso
