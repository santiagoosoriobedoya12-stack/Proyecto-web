# ==============================================================
# EXCEPCIONES TIPADAS DE DOMINIO (ARQUITECTURA EN 3 CAPAS)
# La capa de Servicio y Repositorio lanzan estas excepciones sin saber nada de HTTP.
# Los manejadores globales en main.py las traducen al código HTTP exacto del contrato.
# ==============================================================


class NoEncontradoExcepcion(Exception):
    """Se lanza cuando un recurso solicitado por código, ID o email no existe (HTTP 404)."""
    pass


class ConflictoExcepcion(Exception):
    """Se lanza al violar claves foráneas (SQL 547), duplicar registros (2627/2601) o anular dos veces (HTTP 409)."""
    pass


class ValidacionNegocioExcepcion(Exception):
    """Se lanza cuando un parámetro o regla de negocio no es válido (HTTP 400)."""
    pass


class NoAutenticadoExcepcion(Exception):
    """Se lanza cuando falta el token JWT, expiró, fue alterado o fallan las credenciales (HTTP 401)."""
    pass


class SinPermisoExcepcion(Exception):
    """Se lanza cuando el usuario tiene token válido pero su rol no tiene permiso en 'rutarol' (HTTP 403)."""
    def __init__(self, mensaje: str, ruta: str = ""):
        super().__init__(mensaje)
        self.ruta = ruta
