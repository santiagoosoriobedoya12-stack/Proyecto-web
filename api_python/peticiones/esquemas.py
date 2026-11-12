# ==============================================================
# PETICIONES SEGREGADAS POR VERBO HTTP (v1, v2 y v3)
# Regla de la especificación:
# - *Crear (POST): Todos los campos obligatorios
# - *Reemplazo (PUT): Todos los campos obligatorios (excepto la clave de URL)
# - *Actualizar (PATCH): Todos los campos opcionales (None si no se envían)
# Si una validación falla, FastAPI responde automáticamente HTTP 422.
# ==============================================================

from typing import Optional, List
from pydantic import BaseModel, Field, EmailStr


# ---------------- VERSIÓN 1: 6 TABLAS INDEPENDIENTES ----------------

class ProductoCrear(BaseModel):
    """Petición POST /api/producto: exige código único, nombre, stock >= 0 y valorunitario > 0."""
    codigo: str = Field(..., min_length=1, max_length=10, description="Código único del producto")
    nombre: str = Field(..., min_length=1, max_length=100, description="Nombre comercial")
    stock: int = Field(..., ge=0, description="Existencias físicas (no negativas)")
    valorunitario: float = Field(..., gt=0, description="Precio unitario mayor a 0")


class ProductoReemplazo(BaseModel):
    """Petición PUT /api/producto/{codigo}: obliga enviar todos los campos editables."""
    nombre: str = Field(..., min_length=1, max_length=100)
    stock: int = Field(..., ge=0)
    valorunitario: float = Field(..., gt=0)


class ProductoActualizar(BaseModel):
    """Petición PATCH /api/producto/{codigo}: solo se envían los campos a modificar."""
    nombre: Optional[str] = Field(None, min_length=1, max_length=100)
    stock: Optional[int] = Field(None, ge=0)
    valorunitario: Optional[float] = Field(None, gt=0)


class EmpresaCrear(BaseModel):
    """Petición POST /api/empresa: código único y razón social."""
    codigo: str = Field(..., min_length=1, max_length=10)
    nombre: str = Field(..., min_length=1, max_length=100)


class EmpresaReemplazo(BaseModel):
    """Petición PUT /api/empresa/{codigo}."""
    nombre: str = Field(..., min_length=1, max_length=100)


class EmpresaActualizar(BaseModel):
    """Petición PATCH /api/empresa/{codigo}."""
    nombre: Optional[str] = Field(None, min_length=1, max_length=100)


class PersonaCrear(BaseModel):
    """Petición POST /api/persona: código, nombre, correo válido y teléfono."""
    codigo: str = Field(..., min_length=1, max_length=10)
    nombre: str = Field(..., min_length=1, max_length=100)
    email: EmailStr
    telefono: str = Field(..., min_length=1, max_length=20)


class PersonaReemplazo(BaseModel):
    """Petición PUT /api/persona/{codigo}."""
    nombre: str = Field(..., min_length=1, max_length=100)
    email: EmailStr
    telefono: str = Field(..., min_length=1, max_length=20)


class PersonaActualizar(BaseModel):
    """Petición PATCH /api/persona/{codigo}."""
    nombre: Optional[str] = Field(None, min_length=1, max_length=100)
    email: Optional[EmailStr] = None
    telefono: Optional[str] = Field(None, min_length=1, max_length=20)


class RolCrear(BaseModel):
    """Petición POST /api/rol: el id lo genera IDENTITY en SQL Server."""
    nombre: str = Field(..., min_length=1, max_length=50)


class RolReemplazo(BaseModel):
    """Petición PUT /api/rol/{id}."""
    nombre: str = Field(..., min_length=1, max_length=50)


class RolActualizar(BaseModel):
    """Petición PATCH /api/rol/{id}."""
    nombre: Optional[str] = Field(None, min_length=1, max_length=50)


class RutaCrear(BaseModel):
    """Petición POST /api/ruta: patrón de interfaz/permiso y descripción."""
    ruta: str = Field(..., min_length=1, max_length=100)
    descripcion: str = Field(..., min_length=1, max_length=200)


class RutaReemplazo(BaseModel):
    """Petición PUT /api/ruta/{id}."""
    ruta: str = Field(..., min_length=1, max_length=100)
    descripcion: str = Field(..., min_length=1, max_length=200)


class RutaActualizar(BaseModel):
    """Petición PATCH /api/ruta/{id}."""
    ruta: Optional[str] = Field(None, min_length=1, max_length=100)
    descripcion: Optional[str] = Field(None, min_length=1, max_length=200)


class UsuarioCrear(BaseModel):
    """Petición POST /api/usuario: la contraseña se cifra con BCrypt costo 12 en el servicio."""
    email: EmailStr
    contrasena: str = Field(..., min_length=1, max_length=200)


class UsuarioReemplazo(BaseModel):
    """Petición PUT /api/usuario/{email}: en v3, si llega vacía no borra la clave anterior."""
    contrasena: str = Field(..., max_length=200)


class UsuarioActualizar(BaseModel):
    """Petición PATCH /api/usuario/{email}."""
    contrasena: Optional[str] = Field(None, max_length=200)


# ---------------- VERSIÓN 2: 6 TABLAS CON CLAVE FORÁNEA ----------------

class ClienteCrear(BaseModel):
    """Petición POST /api/cliente: fkcodpersona es obligatorio; fkcodempresa es opcional (null si es persona natural)."""
    credito: float = Field(0, ge=0)
    fkcodpersona: str = Field(..., min_length=1, max_length=10)
    fkcodempresa: Optional[str] = Field(None, max_length=10)


class ClienteReemplazo(BaseModel):
    """Petición PUT /api/cliente/{id}."""
    credito: float = Field(..., ge=0)
    fkcodpersona: str = Field(..., min_length=1, max_length=10)
    fkcodempresa: Optional[str] = Field(None, max_length=10)


class ClienteActualizar(BaseModel):
    """Petición PATCH /api/cliente/{id}."""
    credito: Optional[float] = Field(None, ge=0)
    fkcodpersona: Optional[str] = Field(None, min_length=1, max_length=10)
    fkcodempresa: Optional[str] = Field(None, max_length=10)


class VendedorCrear(BaseModel):
    """Petición POST /api/vendedor: carnet numérico, dirección y código de persona FK."""
    carnet: int = Field(..., gt=0)
    direccion: str = Field(..., min_length=1, max_length=100)
    fkcodpersona: str = Field(..., min_length=1, max_length=10)


class VendedorReemplazo(BaseModel):
    """Petición PUT /api/vendedor/{id}."""
    carnet: int = Field(..., gt=0)
    direccion: str = Field(..., min_length=1, max_length=100)
    fkcodpersona: str = Field(..., min_length=1, max_length=10)


class VendedorActualizar(BaseModel):
    """Petición PATCH /api/vendedor/{id}."""
    carnet: Optional[int] = Field(None, gt=0)
    direccion: Optional[str] = Field(None, min_length=1, max_length=100)
    fkcodpersona: Optional[str] = Field(None, min_length=1, max_length=10)


class ItemFacturaCrear(BaseModel):
    """
    Renglón individual de factura.
    REGLA DE v2: NO recibe precio unitario ni subtotal; SQL Server los calcula vía trigger.
    """
    fkcodproducto: str = Field(..., min_length=1, max_length=10)
    cantidad: int = Field(..., gt=0)


class FacturaCrear(BaseModel):
    """
    Petición POST /api/factura (Maestro-Detalle).
    Exige cliente, vendedor y al menos un renglón en 'productos'. El total se calcula en SQL Server.
    """
    fkidcliente: int = Field(..., gt=0)
    fkidvendedor: int = Field(..., gt=0)
    productos: List[ItemFacturaCrear] = Field(..., min_length=1)


class RolUsuarioCrear(BaseModel):
    """Petición POST /api/rol-usuario para asociar un usuario y un rol (clave compuesta)."""
    fkemail: EmailStr
    fkidrol: int = Field(..., gt=0)


class RutaRolCrear(BaseModel):
    """Petición POST /api/rutarol para otorgar permiso de una ruta a un rol (clave compuesta)."""
    fkidruta: int = Field(..., gt=0)
    fkidrol: int = Field(..., gt=0)


class UsuarioConRolesCrear(BaseModel):
    """Petición POST /api/usuario-con-roles: obliga mínimo 1 rol ([MinLength(1)] -> 422 si vacío)."""
    email: EmailStr
    contrasena: str = Field(..., min_length=1, max_length=200)
    roles: List[int] = Field(..., min_length=1)


class UsuarioConRolesActualizar(BaseModel):
    """Petición PUT /api/usuario-con-roles/{email}: contraseña vacía conserva la anterior (RF1 v3)."""
    contrasena: Optional[str] = Field(None, max_length=200)
    roles: List[int] = Field(..., min_length=1)


# ---------------- VERSIÓN 3: SESIÓN Y AUTENTICACIÓN ----------------

class SesionCrear(BaseModel):
    """Petición POST /api/sesion: credenciales enviadas estrictamente en el cuerpo JSON."""
    email: str = Field(..., min_length=1)
    contrasena: str = Field(..., min_length=1)
