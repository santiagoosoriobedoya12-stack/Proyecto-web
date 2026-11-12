# ==============================================================
# MODELOS DE ENTIDAD DE DOMINIO (CAPA DE MODELOS - P.O.O.)
# Representan los objetos del negocio tipados en Python, independientes de
# SQL Server (pymssql) y de las peticiones HTTP de entrada (Peticiones/).
# ==============================================================

from dataclasses import dataclass, field, asdict
from typing import Optional


@dataclass
class Producto:
    """Entidad de dominio 'producto': representa un artículo del inventario."""
    codigo: str
    nombre: str
    stock: int
    valorunitario: float

    def a_diccionario(self) -> dict:
        """Serializa la entidad a diccionario plano para respuestas JSON."""
        return asdict(self)


@dataclass
class Empresa:
    """Entidad de dominio 'empresa': persona jurídica registrada."""
    codigo: str
    nombre: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Persona:
    """Entidad de dominio 'persona': datos básicos de contacto."""
    codigo: str
    nombre: str
    email: str
    telefono: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Rol:
    """Entidad de dominio 'rol': perfil de acceso dentro del sistema."""
    id: int
    nombre: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Ruta:
    """Entidad de dominio 'ruta': recurso o pantalla protegible por RBAC."""
    id: int
    ruta: str
    descripcion: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Usuario:
    """
    Entidad de dominio 'usuario'.
    Por seguridad (v3), nunca expone la contraseña ni su hash en respuestas JSON.
    """
    email: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Cliente:
    """Entidad de dominio 'cliente' (v2): referencia obligatoria a persona y opcional a empresa."""
    id: int
    credito: float
    fkcodpersona: str
    fkcodempresa: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Vendedor:
    """Entidad de dominio 'vendedor' (v2): fuerza comercial asociada a una persona."""
    id: int
    carnet: int
    direccion: str
    fkcodpersona: str

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class ProductoDeFactura:
    """Renglón individual calculado por SQL Server en 'productosporfactura'."""
    fknumfactura: int
    fkcodproducto: str
    cantidad: int
    subtotal: float

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class Factura:
    """Entidad Maestro-Detalle de 'factura' junto con su colección de renglones."""
    numero: int
    fecha: str
    total: float
    estado: str
    fkidcliente: int
    fkidvendedor: int
    productos: list[ProductoDeFactura] = field(default_factory=list)

    def a_diccionario(self) -> dict:
        return {
            "numero": self.numero,
            "fecha": self.fecha,
            "total": self.total,
            "estado": self.estado,
            "fkidcliente": self.fkidcliente,
            "fkidvendedor": self.fkidvendedor,
            "productos": [p.a_diccionario() for p in self.productos],
        }


@dataclass
class RolUsuario:
    """Entidad puente M:N entre 'usuario' y 'rol'."""
    fkemail: str
    fkidrol: int
    nombre_rol: Optional[str] = None

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class RutaRol:
    """Entidad puente M:N entre 'ruta' y 'rol' (permisos RBAC)."""
    fkidruta: int
    fkidrol: int

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class UsuarioConRoles:
    """Entidad compuesta de usuario junto con su colección de roles asignados."""
    email: str
    roles: list[dict] = field(default_factory=list)

    def a_diccionario(self) -> dict:
        return asdict(self)


@dataclass
class ConfiguracionJwt:
    """Parámetros criptográficos para firmar y verificar tokens HS256 (v3)."""
    clave: str
    emisor: str
    audiencia: str
    expiracion_horas: int = 1


@dataclass
class Sesion:
    """Resultado de autenticación exitosa devuelto al cliente tras validar BCrypt."""
    token: str
    email: str
    roles: list[str] = field(default_factory=list)
    mensaje: str = "Sesion iniciada."
    estado: int = 200

    def a_diccionario(self) -> dict:
        return asdict(self)
