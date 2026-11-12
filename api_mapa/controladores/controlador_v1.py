# ==============================================================
# CONTROLADORES HTTP VERSIÓN 1: LAS 8 TABLAS SIN CLAVE FORÁNEA
# - Lectura (GET): Administrador, Investigador, Consulta.
# - Escritura / Retiro lógico (POST, PUT, PATCH, DELETE): Administrador, Investigador.
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_roles
from ensamblador import (
    servicio_area_conocimiento,
    servicio_ods,
    servicio_area_aplicacion,
    servicio_termino_clave,
    servicio_linea_investigacion,
    servicio_aliado,
    servicio_tipo_producto,
    servicio_proyecto,
)
from peticiones import (
    AreaConocimientoCrear, AreaConocimientoReemplazo, AreaConocimientoActualizar,
    OdsCrear, OdsReemplazo, OdsActualizar,
    AreaAplicacionCrear, AreaAplicacionReemplazo, AreaAplicacionActualizar,
    TerminoClaveCrear, TerminoClaveReemplazo, TerminoClaveActualizar,
    LineaInvestigacionCrear, LineaInvestigacionReemplazo, LineaInvestigacionActualizar,
    AliadoCrear, AliadoReemplazo, AliadoActualizar,
    TipoProductoCrear, TipoProductoReemplazo, TipoProductoActualizar,
    ProyectoCrear, ProyectoReemplazo, ProyectoActualizar,
)

_SOLO_LECTURA = Depends(exige_roles("Administrador", "Investigador", "Consulta"))
_ESCRITURA = Depends(exige_roles("Administrador", "Investigador"))


def respuesta_lista(tabla: str, limite: int, datos: list[dict]):
    """Retorna HTTP 204 No Content si la consulta viene vacía, o 200 OK con el sobre estándar."""
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": tabla, "limite": limite, "total": len(datos), "datos": datos}


# ---------------- 1. AREA_CONOCIMIENTO ----------------
router_area_conocimiento = APIRouter(prefix="/api/area-conocimiento", tags=["v1 - Área de Conocimiento"])

@router_area_conocimiento.get("", dependencies=[_SOLO_LECTURA])
def listar_areas_conocimiento(limite: int = 1000):
    return respuesta_lista("area_conocimiento", limite, servicio_area_conocimiento.listar(limite))

@router_area_conocimiento.get("/{id_area}", dependencies=[_SOLO_LECTURA])
def obtener_area_conocimiento(id_area: int):
    return servicio_area_conocimiento.obtener(id_area)

@router_area_conocimiento.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_area_conocimiento(pet: AreaConocimientoCrear):
    return servicio_area_conocimiento.crear(pet)

@router_area_conocimiento.put("/{id_area}", dependencies=[_ESCRITURA])
def reemplazar_area_conocimiento(id_area: int, pet: AreaConocimientoReemplazo):
    return servicio_area_conocimiento.reemplazar(id_area, pet)

@router_area_conocimiento.patch("/{id_area}", dependencies=[_ESCRITURA])
def actualizar_area_conocimiento(id_area: int, pet: AreaConocimientoActualizar):
    filas = servicio_area_conocimiento.actualizar_parcial(id_area, pet)
    return {"mensaje": "Área de conocimiento actualizada.", "filas_afectadas": filas}

@router_area_conocimiento.delete("/{id_area}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_area_conocimiento(id_area: int):
    servicio_area_conocimiento.eliminar(id_area)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 2. OBJETIVO_DESARROLLO_SOSTENIBLE (ODS) ----------------
router_ods = APIRouter(prefix="/api/objetivo-desarrollo-sostenible", tags=["v1 - ODS"])

@router_ods.get("", dependencies=[_SOLO_LECTURA])
def listar_ods(limite: int = 1000):
    return respuesta_lista("objetivo_desarrollo_sostenible", limite, servicio_ods.listar(limite))

@router_ods.get("/{id_ods}", dependencies=[_SOLO_LECTURA])
def obtener_ods(id_ods: int):
    return servicio_ods.obtener(id_ods)

@router_ods.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_ods(pet: OdsCrear):
    return servicio_ods.crear(pet)

@router_ods.put("/{id_ods}", dependencies=[_ESCRITURA])
def reemplazar_ods(id_ods: int, pet: OdsReemplazo):
    return servicio_ods.reemplazar(id_ods, pet)

@router_ods.patch("/{id_ods}", dependencies=[_ESCRITURA])
def actualizar_ods(id_ods: int, pet: OdsActualizar):
    filas = servicio_ods.actualizar_parcial(id_ods, pet)
    return {"mensaje": "ODS actualizado.", "filas_afectadas": filas}

@router_ods.delete("/{id_ods}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_ods(id_ods: int):
    servicio_ods.eliminar(id_ods)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 3. AREA_APLICACION ----------------
router_area_aplicacion = APIRouter(prefix="/api/area-aplicacion", tags=["v1 - Área de Aplicación"])

@router_area_aplicacion.get("", dependencies=[_SOLO_LECTURA])
def listar_areas_aplicacion(limite: int = 1000):
    return respuesta_lista("area_aplicacion", limite, servicio_area_aplicacion.listar(limite))

@router_area_aplicacion.get("/{id_aa}", dependencies=[_SOLO_LECTURA])
def obtener_area_aplicacion(id_aa: int):
    return servicio_area_aplicacion.obtener(id_aa)

@router_area_aplicacion.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_area_aplicacion(pet: AreaAplicacionCrear):
    return servicio_area_aplicacion.crear(pet)

@router_area_aplicacion.put("/{id_aa}", dependencies=[_ESCRITURA])
def reemplazar_area_aplicacion(id_aa: int, pet: AreaAplicacionReemplazo):
    return servicio_area_aplicacion.reemplazar(id_aa, pet)

@router_area_aplicacion.patch("/{id_aa}", dependencies=[_ESCRITURA])
def actualizar_area_aplicacion(id_aa: int, pet: AreaAplicacionActualizar):
    filas = servicio_area_aplicacion.actualizar_parcial(id_aa, pet)
    return {"mensaje": "Área de aplicación actualizada.", "filas_afectadas": filas}

@router_area_aplicacion.delete("/{id_aa}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_area_aplicacion(id_aa: int):
    servicio_area_aplicacion.eliminar(id_aa)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 4. TERMINO_CLAVE ----------------
router_termino_clave = APIRouter(prefix="/api/termino-clave", tags=["v1 - Término Clave"])

@router_termino_clave.get("", dependencies=[_SOLO_LECTURA])
def listar_terminos_clave(limite: int = 1000):
    return respuesta_lista("termino_clave", limite, servicio_termino_clave.listar(limite))

@router_termino_clave.get("/{termino}", dependencies=[_SOLO_LECTURA])
def obtener_termino_clave(termino: str):
    return servicio_termino_clave.obtener(termino)

@router_termino_clave.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_termino_clave(pet: TerminoClaveCrear):
    return servicio_termino_clave.crear(pet)

@router_termino_clave.put("/{termino}", dependencies=[_ESCRITURA])
def reemplazar_termino_clave(termino: str, pet: TerminoClaveReemplazo):
    return servicio_termino_clave.reemplazar(termino, pet)

@router_termino_clave.patch("/{termino}", dependencies=[_ESCRITURA])
def actualizar_termino_clave(termino: str, pet: TerminoClaveActualizar):
    filas = servicio_termino_clave.actualizar_parcial(termino, pet)
    return {"mensaje": "Término clave actualizado.", "filas_afectadas": filas}

@router_termino_clave.delete("/{termino}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_termino_clave(termino: str):
    servicio_termino_clave.eliminar(termino)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 5. LINEA_INVESTIGACION ----------------
router_linea_investigacion = APIRouter(prefix="/api/linea-investigacion", tags=["v1 - Línea de Investigación"])

@router_linea_investigacion.get("", dependencies=[_SOLO_LECTURA])
def listar_lineas_investigacion(limite: int = 1000):
    return respuesta_lista("linea_investigacion", limite, servicio_linea_investigacion.listar(limite))

@router_linea_investigacion.get("/{id_linea}", dependencies=[_SOLO_LECTURA])
def obtener_linea_investigacion(id_linea: int):
    return servicio_linea_investigacion.obtener(id_linea)

@router_linea_investigacion.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_linea_investigacion(pet: LineaInvestigacionCrear):
    return servicio_linea_investigacion.crear(pet)

@router_linea_investigacion.put("/{id_linea}", dependencies=[_ESCRITURA])
def reemplazar_linea_investigacion(id_linea: int, pet: LineaInvestigacionReemplazo):
    return servicio_linea_investigacion.reemplazar(id_linea, pet)

@router_linea_investigacion.patch("/{id_linea}", dependencies=[_ESCRITURA])
def actualizar_linea_investigacion(id_linea: int, pet: LineaInvestigacionActualizar):
    filas = servicio_linea_investigacion.actualizar_parcial(id_linea, pet)
    return {"mensaje": "Línea de investigación actualizada.", "filas_afectadas": filas}

@router_linea_investigacion.delete("/{id_linea}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_linea_investigacion(id_linea: int):
    servicio_linea_investigacion.eliminar(id_linea)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 6. ALIADO ----------------
router_aliado = APIRouter(prefix="/api/aliado", tags=["v1 - Aliado"])

@router_aliado.get("", dependencies=[_SOLO_LECTURA])
def listar_aliados(limite: int = 1000):
    return respuesta_lista("aliado", limite, servicio_aliado.listar(limite))

@router_aliado.get("/{nit}", dependencies=[_SOLO_LECTURA])
def obtener_aliado(nit: int):
    return servicio_aliado.obtener(nit)

@router_aliado.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_aliado(pet: AliadoCrear):
    return servicio_aliado.crear(pet)

@router_aliado.put("/{nit}", dependencies=[_ESCRITURA])
def reemplazar_aliado(nit: int, pet: AliadoReemplazo):
    return servicio_aliado.reemplazar(nit, pet)

@router_aliado.patch("/{nit}", dependencies=[_ESCRITURA])
def actualizar_aliado(nit: int, pet: AliadoActualizar):
    filas = servicio_aliado.actualizar_parcial(nit, pet)
    return {"mensaje": "Aliado actualizado.", "filas_afectadas": filas}

@router_aliado.delete("/{nit}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_aliado(nit: int):
    servicio_aliado.eliminar(nit)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 7. TIPO_PRODUCTO ----------------
router_tipo_producto = APIRouter(prefix="/api/tipo-producto", tags=["v1 - Tipo Producto"])

@router_tipo_producto.get("", dependencies=[_SOLO_LECTURA])
def listar_tipos_producto(limite: int = 1000):
    return respuesta_lista("tipo_producto", limite, servicio_tipo_producto.listar(limite))

@router_tipo_producto.get("/{id_tipo}", dependencies=[_SOLO_LECTURA])
def obtener_tipo_producto(id_tipo: int):
    return servicio_tipo_producto.obtener(id_tipo)

@router_tipo_producto.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_tipo_producto(pet: TipoProductoCrear):
    return servicio_tipo_producto.crear(pet)

@router_tipo_producto.put("/{id_tipo}", dependencies=[_ESCRITURA])
def reemplazar_tipo_producto(id_tipo: int, pet: TipoProductoReemplazo):
    return servicio_tipo_producto.reemplazar(id_tipo, pet)

@router_tipo_producto.patch("/{id_tipo}", dependencies=[_ESCRITURA])
def actualizar_tipo_producto(id_tipo: int, pet: TipoProductoActualizar):
    filas = servicio_tipo_producto.actualizar_parcial(id_tipo, pet)
    return {"mensaje": "Tipo de producto actualizado.", "filas_afectadas": filas}

@router_tipo_producto.delete("/{id_tipo}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_tipo_producto(id_tipo: int):
    servicio_tipo_producto.eliminar(id_tipo)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 8. PROYECTO ----------------
router_proyecto = APIRouter(prefix="/api/proyecto", tags=["v1 - Proyecto"])

@router_proyecto.get("", dependencies=[_SOLO_LECTURA])
def listar_proyectos(limite: int = 1000):
    return respuesta_lista("proyecto", limite, servicio_proyecto.listar(limite))

@router_proyecto.get("/{id_proyecto}", dependencies=[_SOLO_LECTURA])
def obtener_proyecto(id_proyecto: int):
    return servicio_proyecto.obtener(id_proyecto)

@router_proyecto.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_proyecto(pet: ProyectoCrear):
    return servicio_proyecto.crear(pet)

@router_proyecto.put("/{id_proyecto}", dependencies=[_ESCRITURA])
def reemplazar_proyecto(id_proyecto: int, pet: ProyectoReemplazo):
    return servicio_proyecto.reemplazar(id_proyecto, pet)

@router_proyecto.patch("/{id_proyecto}", dependencies=[_ESCRITURA])
def actualizar_proyecto(id_proyecto: int, pet: ProyectoActualizar):
    filas = servicio_proyecto.actualizar_parcial(id_proyecto, pet)
    return {"mensaje": "Proyecto actualizado.", "filas_afectadas": filas}

@router_proyecto.delete("/{id_proyecto}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_proyecto(id_proyecto: int):
    servicio_proyecto.eliminar(id_proyecto)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
