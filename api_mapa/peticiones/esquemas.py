# ==============================================================
# ESQUEMAS PYDANTIC SEGREGADOS POR VERBO (FRONTERA HTTP 422)
# Regla de diseño (6_contracts.md):
# - *Crear (POST): campos obligatorios según esquema SQL.
# - *Reemplazo (PUT): reemplazo completo idempotente (422 si falta obligatorio).
# - *Actualizar (PATCH): todos opcionales (400 si el cuerpo llega vacío).
# ==============================================================

from datetime import date
from typing import Optional
from pydantic import BaseModel, EmailStr, Field


# ==============================================================
# VERSIÓN 1 — 8 TABLAS INDEPENDIENTES
# ==============================================================

class AreaConocimientoCrear(BaseModel):
    id: int = Field(..., ge=1)
    gran_area: str = Field(..., min_length=1, max_length=60)
    area: str = Field(..., min_length=1, max_length=60)
    disciplina: str = Field(..., min_length=1, max_length=60)


class AreaConocimientoReemplazo(BaseModel):
    gran_area: str = Field(..., min_length=1, max_length=60)
    area: str = Field(..., min_length=1, max_length=60)
    disciplina: str = Field(..., min_length=1, max_length=60)


class AreaConocimientoActualizar(BaseModel):
    gran_area: Optional[str] = Field(None, min_length=1, max_length=60)
    area: Optional[str] = Field(None, min_length=1, max_length=60)
    disciplina: Optional[str] = Field(None, min_length=1, max_length=60)


class OdsCrear(BaseModel):
    id: int = Field(..., ge=1)
    nombre: str = Field(..., min_length=1, max_length=60)
    categoria: str = Field(..., min_length=1, max_length=45)


class OdsReemplazo(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=60)
    categoria: str = Field(..., min_length=1, max_length=45)


class OdsActualizar(BaseModel):
    nombre: Optional[str] = Field(None, min_length=1, max_length=60)
    categoria: Optional[str] = Field(None, min_length=1, max_length=45)


class AreaAplicacionCrear(BaseModel):
    id: int = Field(..., ge=1)
    nombre: str = Field(..., min_length=1, max_length=60)


class AreaAplicacionReemplazo(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=60)


class AreaAplicacionActualizar(BaseModel):
    nombre: Optional[str] = Field(None, min_length=1, max_length=60)


class TerminoClaveCrear(BaseModel):
    termino: str = Field(..., min_length=1, max_length=30)
    termino_ingles: Optional[str] = Field(None, max_length=30)


class TerminoClaveReemplazo(BaseModel):
    termino_ingles: Optional[str] = Field(None, max_length=30)


class TerminoClaveActualizar(BaseModel):
    termino_ingles: Optional[str] = Field(None, max_length=30)


class LineaInvestigacionCrear(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=45)
    descripcion: str = Field(..., min_length=1, max_length=256)


class LineaInvestigacionReemplazo(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=45)
    descripcion: str = Field(..., min_length=1, max_length=256)


class LineaInvestigacionActualizar(BaseModel):
    nombre: Optional[str] = Field(None, min_length=1, max_length=45)
    descripcion: Optional[str] = Field(None, min_length=1, max_length=256)


class AliadoCrear(BaseModel):
    nit: int = Field(..., ge=1)
    razon_social: str = Field(..., min_length=1, max_length=60)
    nombre_contacto: str = Field(..., min_length=1, max_length=60)
    correo: EmailStr = Field(..., max_length=70)
    telefono: str = Field(..., min_length=1, max_length=45)
    ciudad: str = Field(..., min_length=1, max_length=45)


class AliadoReemplazo(BaseModel):
    razon_social: str = Field(..., min_length=1, max_length=60)
    nombre_contacto: str = Field(..., min_length=1, max_length=60)
    correo: EmailStr = Field(..., max_length=70)
    telefono: str = Field(..., min_length=1, max_length=45)
    ciudad: str = Field(..., min_length=1, max_length=45)


class AliadoActualizar(BaseModel):
    razon_social: Optional[str] = Field(None, min_length=1, max_length=60)
    nombre_contacto: Optional[str] = Field(None, min_length=1, max_length=60)
    correo: Optional[EmailStr] = Field(None, max_length=70)
    telefono: Optional[str] = Field(None, min_length=1, max_length=45)
    ciudad: Optional[str] = Field(None, min_length=1, max_length=45)


class TipoProductoCrear(BaseModel):
    id: int = Field(..., ge=1)
    categoria: str = Field(..., min_length=1, max_length=45)
    clase: str = Field(..., min_length=1, max_length=45)
    nombre: str = Field(..., min_length=1, max_length=45)
    tipologia: str = Field(..., min_length=1, max_length=45)


class TipoProductoReemplazo(BaseModel):
    categoria: str = Field(..., min_length=1, max_length=45)
    clase: str = Field(..., min_length=1, max_length=45)
    nombre: str = Field(..., min_length=1, max_length=45)
    tipologia: str = Field(..., min_length=1, max_length=45)


class TipoProductoActualizar(BaseModel):
    categoria: Optional[str] = Field(None, min_length=1, max_length=45)
    clase: Optional[str] = Field(None, min_length=1, max_length=45)
    nombre: Optional[str] = Field(None, min_length=1, max_length=45)
    tipologia: Optional[str] = Field(None, min_length=1, max_length=45)


class ProyectoCrear(BaseModel):
    id: int = Field(..., ge=1)
    titulo: str = Field(..., min_length=1, max_length=70)
    resumen: str = Field(..., min_length=1, max_length=256)
    presupuesto: float = Field(..., ge=0)
    tipo_financiacion: str = Field(..., min_length=1, max_length=45)
    tipo_fondos: str = Field(..., min_length=1, max_length=45)
    fecha_inicio: date
    fecha_fin: Optional[date] = None


class ProyectoReemplazo(BaseModel):
    titulo: str = Field(..., min_length=1, max_length=70)
    resumen: str = Field(..., min_length=1, max_length=256)
    presupuesto: float = Field(..., ge=0)
    tipo_financiacion: str = Field(..., min_length=1, max_length=45)
    tipo_fondos: str = Field(..., min_length=1, max_length=45)
    fecha_inicio: date
    fecha_fin: Optional[date] = None


class ProyectoActualizar(BaseModel):
    titulo: Optional[str] = Field(None, min_length=1, max_length=70)
    resumen: Optional[str] = Field(None, min_length=1, max_length=256)
    presupuesto: Optional[float] = Field(None, ge=0)
    tipo_financiacion: Optional[str] = Field(None, min_length=1, max_length=45)
    tipo_fondos: Optional[str] = Field(None, min_length=1, max_length=45)
    fecha_inicio: Optional[date] = None
    fecha_fin: Optional[date] = None


# ==============================================================
# VERSIÓN 2 — 10 TABLAS CON CLAVE FORÁNEA
# ==============================================================

class DocenteCrear(BaseModel):
    cedula: int = Field(..., ge=1)
    nombres: str = Field(..., min_length=1, max_length=60)
    apellidos: str = Field(..., min_length=1, max_length=60)
    genero: str = Field(..., min_length=1, max_length=12)
    cargo: str = Field(..., min_length=1, max_length=30)
    fecha_nacimiento: date
    correo: EmailStr = Field(..., max_length=70)
    telefono: str = Field(..., min_length=1, max_length=20)
    url_cvlac: str = Field(..., min_length=1, max_length=128)
    fecha_actualizacion: date
    escalafon: str = Field(..., min_length=1, max_length=45)
    perfil: str = Field(..., min_length=1)
    cat_minciencia: Optional[str] = Field(None, max_length=45)
    conv_minciencia: str = Field(..., min_length=1, max_length=45)
    nacionalidaad: str = Field(..., min_length=1, max_length=45)
    linea_investigacion_principal: Optional[int] = Field(None, ge=1)


class DocenteReemplazo(BaseModel):
    nombres: str = Field(..., min_length=1, max_length=60)
    apellidos: str = Field(..., min_length=1, max_length=60)
    genero: str = Field(..., min_length=1, max_length=12)
    cargo: str = Field(..., min_length=1, max_length=30)
    fecha_nacimiento: date
    correo: EmailStr = Field(..., max_length=70)
    telefono: str = Field(..., min_length=1, max_length=20)
    url_cvlac: str = Field(..., min_length=1, max_length=128)
    fecha_actualizacion: date
    escalafon: str = Field(..., min_length=1, max_length=45)
    perfil: str = Field(..., min_length=1)
    cat_minciencia: Optional[str] = Field(None, max_length=45)
    conv_minciencia: str = Field(..., min_length=1, max_length=45)
    nacionalidaad: str = Field(..., min_length=1, max_length=45)
    linea_investigacion_principal: Optional[int] = Field(None, ge=1)


class DocenteActualizar(BaseModel):
    nombres: Optional[str] = Field(None, min_length=1, max_length=60)
    apellidos: Optional[str] = Field(None, min_length=1, max_length=60)
    genero: Optional[str] = Field(None, min_length=1, max_length=12)
    cargo: Optional[str] = Field(None, min_length=1, max_length=30)
    fecha_nacimiento: Optional[date] = None
    correo: Optional[EmailStr] = Field(None, max_length=70)
    telefono: Optional[str] = Field(None, min_length=1, max_length=20)
    url_cvlac: Optional[str] = Field(None, min_length=1, max_length=128)
    fecha_actualizacion: Optional[date] = None
    escalafon: Optional[str] = Field(None, min_length=1, max_length=45)
    perfil: Optional[str] = Field(None, min_length=1)
    cat_minciencia: Optional[str] = Field(None, max_length=45)
    conv_minciencia: Optional[str] = Field(None, min_length=1, max_length=45)
    nacionalidaad: Optional[str] = Field(None, min_length=1, max_length=45)
    linea_investigacion_principal: Optional[int] = Field(None, ge=1)


class ProductoCrear(BaseModel):
    id: int = Field(..., ge=1)
    nombre: str = Field(..., min_length=1, max_length=45)
    categoria: str = Field(..., min_length=1, max_length=45)
    fecha_entrega: date
    proyecto: Optional[int] = Field(None, ge=1)
    tipo_producto: int = Field(..., ge=1)


class ProductoReemplazo(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=45)
    categoria: str = Field(..., min_length=1, max_length=45)
    fecha_entrega: date
    proyecto: Optional[int] = Field(None, ge=1)
    tipo_producto: int = Field(..., ge=1)


class ProductoActualizar(BaseModel):
    nombre: Optional[str] = Field(None, min_length=1, max_length=45)
    categoria: Optional[str] = Field(None, min_length=1, max_length=45)
    fecha_entrega: Optional[date] = None
    proyecto: Optional[int] = Field(None, ge=1)
    tipo_producto: Optional[int] = Field(None, ge=1)


class AliadoProyectoCrear(BaseModel):
    aliado: int = Field(..., ge=1)
    proyecto: int = Field(..., ge=1)


class DesarrollaCrear(BaseModel):
    docente: int = Field(..., ge=1)
    proyecto: int = Field(..., ge=1)
    rol: str = Field(..., min_length=1, max_length=45)
    descripcion: str = Field(..., min_length=1, max_length=256)


class DesarrollaReemplazo(BaseModel):
    rol: str = Field(..., min_length=1, max_length=45)
    descripcion: str = Field(..., min_length=1, max_length=256)


class DesarrollaActualizar(BaseModel):
    rol: Optional[str] = Field(None, min_length=1, max_length=45)
    descripcion: Optional[str] = Field(None, min_length=1, max_length=256)


class PalabrasClaveCrear(BaseModel):
    proyecto: int = Field(..., ge=1)
    termino_clave: str = Field(..., min_length=1, max_length=30)


class AcProyectoCrear(BaseModel):
    proyecto: int = Field(..., ge=1)
    area_conocimiento: int = Field(..., ge=1)


class ProyectoLineaCrear(BaseModel):
    proyecto: int = Field(..., ge=1)
    linea_investigacion: int = Field(..., ge=1)


class OdsProyectoCrear(BaseModel):
    proyecto: int = Field(..., ge=1)
    ods: int = Field(..., ge=1)


class AaProyectoCrear(BaseModel):
    proyecto: int = Field(..., ge=1)
    area_aplicacion: int = Field(..., ge=1)


class DocenteProductoCrear(BaseModel):
    docente: int = Field(..., ge=1)
    producto: int = Field(..., ge=1)


# ==============================================================
# VERSIÓN 3 — SEGURIDAD, ROLES, USUARIOS Y LOGIN JWT
# ==============================================================

class RolCrear(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=100)
    descripcion: Optional[str] = None


class RolReemplazo(BaseModel):
    nombre: str = Field(..., min_length=1, max_length=100)
    descripcion: Optional[str] = None


class RolActualizar(BaseModel):
    nombre: Optional[str] = Field(None, min_length=1, max_length=100)
    descripcion: Optional[str] = None


class UsuarioCrear(BaseModel):
    username: str = Field(..., min_length=1, max_length=100)
    password: str = Field(..., min_length=1, max_length=255)
    email: EmailStr = Field(..., max_length=150)
    nombre_completo: Optional[str] = Field(None, max_length=200)


class UsuarioReemplazo(BaseModel):
    username: str = Field(..., min_length=1, max_length=100)
    password: Optional[str] = Field("", max_length=255)
    email: EmailStr = Field(..., max_length=150)
    nombre_completo: Optional[str] = Field(None, max_length=200)


class UsuarioActualizar(BaseModel):
    username: Optional[str] = Field(None, min_length=1, max_length=100)
    password: Optional[str] = Field(None, max_length=255)
    email: Optional[EmailStr] = Field(None, max_length=150)
    nombre_completo: Optional[str] = Field(None, max_length=200)


class RolUsuarioCrear(BaseModel):
    usuario_id: int = Field(..., ge=1)
    rol_id: int = Field(..., ge=1)


class LoginPeticion(BaseModel):
    """Acepta login por username o correo electrónico en el campo 'usuario' o 'email'."""
    usuario: Optional[str] = Field(None, min_length=1)
    email: Optional[str] = Field(None, min_length=1)
    password: str = Field(..., min_length=1)
