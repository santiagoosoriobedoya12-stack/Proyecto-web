# ==============================================================
# MODELOS DE ENTIDAD DE DOMINIO — MAPA DE CONOCIMIENTO (21 TABLAS)
# Clases POO tipadas que representan los datos del negocio sin incluir
# columnas internas de control como 'activo' ni 'password'.
# ==============================================================

from dataclasses import asdict, dataclass, field
from typing import Optional


# ---------------- VERSIÓN 1: 8 TABLAS SIN FK ----------------

@dataclass
class AreaConocimiento:
    id: int
    gran_area: str
    area: str
    disciplina: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class ObjetivoDesarrolloSostenible:
    id: int
    nombre: str
    categoria: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class AreaAplicacion:
    id: int
    nombre: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class TerminoClave:
    termino: str
    termino_ingles: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class LineaInvestigacion:
    id: int
    nombre: str
    descripcion: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Aliado:
    nit: int
    razon_social: str
    nombre_contacto: str
    correo: str
    telefono: str
    ciudad: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class TipoProducto:
    id: int
    categoria: str
    clase: str
    nombre: str
    tipologia: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Proyecto:
    id: int
    titulo: str
    resumen: str
    presupuesto: float
    tipo_financiacion: str
    tipo_fondos: str
    fecha_inicio: str
    fecha_fin: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


# ---------------- VERSIÓN 2: 10 TABLAS CON FK ----------------

@dataclass
class Docente:
    cedula: int
    nombres: str
    apellidos: str
    genero: str
    cargo: str
    fecha_nacimiento: str
    correo: str
    telefono: str
    url_cvlac: str
    fecha_actualizacion: str
    escalafon: str
    perfil: str
    conv_minciencia: str
    nacionalidaad: str
    cat_minciencia: Optional[str] = None
    linea_investigacion_principal: Optional[int] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Producto:
    id: int
    nombre: str
    categoria: str
    fecha_entrega: str
    tipo_producto: int
    proyecto: Optional[int] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class AliadoProyecto:
    aliado: int
    proyecto: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Desarrolla:
    docente: int
    proyecto: int
    rol: str
    descripcion: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class PalabrasClave:
    proyecto: int
    termino_clave: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class AcProyecto:
    proyecto: int
    area_conocimiento: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class ProyectoLinea:
    proyecto: int
    linea_investigacion: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class OdsProyecto:
    proyecto: int
    ods: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class AaProyecto:
    proyecto: int
    area_aplicacion: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class DocenteProducto:
    docente: int
    producto: int

    def a_diccionario(self) -> dict:
        return asdict(self)


# ---------------- VERSIÓN 3: USUARIOS, ROLES Y SESIÓN ----------------

@dataclass
class Rol:
    id: int
    nombre: str
    descripcion: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Usuario:
    """Nunca incluye 'password' para proteger el hash BCrypt."""
    id: int
    username: str
    email: str
    nombre_completo: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class RolUsuario:
    usuario_id: int
    rol_id: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Sesion:
    token: str
    username: str
    email: str
    roles: list[str] = field(default_factory=list)
    mensaje: str = "Sesion iniciada."
    estado: int = 200

    def a_diccionario(self) -> dict:
        return asdict(self)
