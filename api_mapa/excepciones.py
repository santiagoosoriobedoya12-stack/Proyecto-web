# ==============================================================
# EXCEPCIONES TIPADAS DE DOMINIO (ARQUITECTURA EN CAPAS)
# Herencia bien usada: especializan Exception para que los servicios lancen
# errores de negocio sin conocer códigos HTTP; main.py los traduce a REST.
# ==============================================================


class NoEncontradoExcepcion(Exception):
    """Recurso inexistente o retirado lógicamente (activo = 0) -> HTTP 404."""
    pass


class ConflictoExcepcion(Exception):
    """Clave duplicada (1062) o violación de integridad referencial (1451/1452) -> HTTP 409."""
    pass


class ValidacionNegocioExcepcion(Exception):
    """Regla de negocio o parámetro inválido (ej. PATCH sin campos, límite <= 0) -> HTTP 400."""
    pass


class NoAutenticadoExcepcion(Exception):
    """Token JWT ausente, inválido, expirado o credenciales erróneas -> HTTP 401."""
    pass


class SinPermisoExcepcion(Exception):
    """Usuario autenticado cuyo rol no autoriza la operación solicitada -> HTTP 403."""
    def __init__(self, mensaje: str, ruta: str = ""):
        super().__init__(mensaje)
        self.ruta = ruta
