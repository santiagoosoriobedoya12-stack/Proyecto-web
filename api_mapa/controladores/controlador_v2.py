# ==============================================================
# CONTROLADORES HTTP VERSIÓN 2: LAS 10 TABLAS CON CLAVE FORÁNEA
# - Docente, Producto, Desarrolla y 7 tablas puente M:N.
# - Lectura (GET): Administrador, Investigador, Consulta.
# - Escritura / Retiro lógico: Administrador, Investigador.
# ==============================================================

from fastapi import APIRouter, Depends, Response, status
from autorizacion import exige_roles
from controladores.controlador_v1 import respuesta_lista
from ensamblador import (
    servicio_docente,
    servicio_producto,
    servicio_desarrolla,
    servicio_aliado_proyecto,
    servicio_palabras_clave,
    servicio_ac_proyecto,
    servicio_proyecto_linea,
    servicio_ods_proyecto,
    servicio_aa_proyecto,
    servicio_docente_producto,
)
from peticiones import (
    DocenteCrear, DocenteReemplazo, DocenteActualizar,
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    DesarrollaCrear, DesarrollaReemplazo, DesarrollaActualizar,
    AliadoProyectoCrear, PalabrasClaveCrear, AcProyectoCrear,
    ProyectoLineaCrear, OdsProyectoCrear, AaProyectoCrear, DocenteProductoCrear,
)

_SOLO_LECTURA = Depends(exige_roles("Administrador", "Investigador", "Consulta"))
_ESCRITURA = Depends(exige_roles("Administrador", "Investigador"))


def _crear_router_puente(
    prefijo: str,
    etiqueta: str,
    tabla: str,
    servicio,
    modelo_crear,
    clave_izq: str,
    clave_der: str,
    tipo_izq: type,
    tipo_der: type,
) -> APIRouter:
    """Construye los 5 endpoints REST explícitos de una tabla puente M:N."""
    router = APIRouter(prefix=prefijo, tags=[etiqueta])

    @router.get("", dependencies=[_SOLO_LECTURA])
    def listar_todos():
        datos = servicio.listar()
        if not datos:
            return Response(status_code=status.HTTP_204_NO_CONTENT)
        return {"tabla": tabla, "total": len(datos), "datos": datos}

    @router.get(f"/{clave_izq}/{{valor_izq}}", dependencies=[_SOLO_LECTURA])
    def listar_por_izquierda(valor_izq: tipo_izq):
        datos = servicio.obtener_por_izquierda(valor_izq)
        return {clave_izq: valor_izq, "total": len(datos), "datos": datos}

    @router.get(f"/{clave_der}/{{valor_der}}", dependencies=[_SOLO_LECTURA])
    def listar_por_derecha(valor_der: tipo_der):
        datos = servicio.obtener_por_derecha(valor_der)
        return {clave_der: valor_der, "total": len(datos), "datos": datos}

    @router.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
    def asociar(pet: modelo_crear):
        return servicio.crear(getattr(pet, clave_izq), getattr(pet, clave_der))

    @router.delete("/{valor_izq}/{valor_der}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
    def desasociar(valor_izq: tipo_izq, valor_der: tipo_der):
        servicio.eliminar(valor_izq, valor_der)
        return Response(status_code=status.HTTP_204_NO_CONTENT)

    return router


# ---------------- 9. DOCENTE (/api/docente) ----------------
router_docente = APIRouter(prefix="/api/docente", tags=["v2 - Docente"])

@router_docente.get("", dependencies=[_SOLO_LECTURA])
def listar_docentes(limite: int = 1000):
    return respuesta_lista("docente", limite, servicio_docente.listar(limite))

@router_docente.get("/{cedula}", dependencies=[_SOLO_LECTURA])
def obtener_docente(cedula: int):
    return servicio_docente.obtener(cedula)

@router_docente.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_docente(pet: DocenteCrear):
    return servicio_docente.crear(pet)

@router_docente.put("/{cedula}", dependencies=[_ESCRITURA])
def reemplazar_docente(cedula: int, pet: DocenteReemplazo):
    return servicio_docente.reemplazar(cedula, pet)

@router_docente.patch("/{cedula}", dependencies=[_ESCRITURA])
def actualizar_docente(cedula: int, pet: DocenteActualizar):
    filas = servicio_docente.actualizar_parcial(cedula, pet)
    return {"mensaje": "Docente actualizado.", "filas_afectadas": filas}

@router_docente.delete("/{cedula}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_docente(cedula: int):
    servicio_docente.eliminar(cedula)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 10. PRODUCTO (/api/producto) ----------------
router_producto = APIRouter(prefix="/api/producto", tags=["v2 - Producto"])

@router_producto.get("", dependencies=[_SOLO_LECTURA])
def listar_productos(limite: int = 1000):
    return respuesta_lista("producto", limite, servicio_producto.listar(limite))

@router_producto.get("/{id_prod}", dependencies=[_SOLO_LECTURA])
def obtener_producto(id_prod: int):
    return servicio_producto.obtener(id_prod)

@router_producto.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_producto(pet: ProductoCrear):
    return servicio_producto.crear(pet)

@router_producto.put("/{id_prod}", dependencies=[_ESCRITURA])
def reemplazar_producto(id_prod: int, pet: ProductoReemplazo):
    return servicio_producto.reemplazar(id_prod, pet)

@router_producto.patch("/{id_prod}", dependencies=[_ESCRITURA])
def actualizar_producto(id_prod: int, pet: ProductoActualizar):
    filas = servicio_producto.actualizar_parcial(id_prod, pet)
    return {"mensaje": "Producto actualizado.", "filas_afectadas": filas}

@router_producto.delete("/{id_prod}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_producto(id_prod: int):
    servicio_producto.eliminar(id_prod)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 11. DESARROLLA (/api/desarrolla) ----------------
router_desarrolla = APIRouter(prefix="/api/desarrolla", tags=["v2 - Desarrolla"])

@router_desarrolla.get("", dependencies=[_SOLO_LECTURA])
def listar_desarrolla():
    datos = servicio_desarrolla.listar()
    if not datos:
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    return {"tabla": "desarrolla", "total": len(datos), "datos": datos}

@router_desarrolla.get("/docente/{cedula}", dependencies=[_SOLO_LECTURA])
def desarrollos_de_docente(cedula: int):
    datos = servicio_desarrolla.obtener_por_docente(cedula)
    return {"docente": cedula, "total": len(datos), "datos": datos}

@router_desarrolla.get("/proyecto/{id_proyecto}", dependencies=[_SOLO_LECTURA])
def desarrollos_de_proyecto(id_proyecto: int):
    datos = servicio_desarrolla.obtener_por_proyecto(id_proyecto)
    return {"proyecto": id_proyecto, "total": len(datos), "datos": datos}

@router_desarrolla.post("", status_code=status.HTTP_201_CREATED, dependencies=[_ESCRITURA])
def crear_desarrolla(pet: DesarrollaCrear):
    return servicio_desarrolla.crear(pet)

@router_desarrolla.put("/{cedula}/{id_proyecto}", dependencies=[_ESCRITURA])
def reemplazar_desarrolla(cedula: int, id_proyecto: int, pet: DesarrollaReemplazo):
    return servicio_desarrolla.reemplazar(cedula, id_proyecto, pet)

@router_desarrolla.patch("/{cedula}/{id_proyecto}", dependencies=[_ESCRITURA])
def actualizar_desarrolla(cedula: int, id_proyecto: int, pet: DesarrollaActualizar):
    filas = servicio_desarrolla.actualizar_parcial(cedula, id_proyecto, pet)
    return {"mensaje": "Participación actualizada.", "filas_afectadas": filas}

@router_desarrolla.delete("/{cedula}/{id_proyecto}", status_code=status.HTTP_204_NO_CONTENT, dependencies=[_ESCRITURA])
def eliminar_desarrolla(cedula: int, id_proyecto: int):
    servicio_desarrolla.eliminar(cedula, id_proyecto)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


# ---------------- 12..18. LAS 7 TABLAS PUENTE M:N ----------------
router_aliado_proyecto = _crear_router_puente(
    "/api/aliado-proyecto", "v2 - Aliado Proyecto", "aliado_proyecto",
    servicio_aliado_proyecto, AliadoProyectoCrear, "aliado", "proyecto", int, int,
)
router_palabras_clave = _crear_router_puente(
    "/api/palabras-clave", "v2 - Palabras Clave", "palabras_clave",
    servicio_palabras_clave, PalabrasClaveCrear, "proyecto", "termino_clave", int, str,
)
router_ac_proyecto = _crear_router_puente(
    "/api/ac-proyecto", "v2 - Área Conocimiento Proyecto", "ac_proyecto",
    servicio_ac_proyecto, AcProyectoCrear, "proyecto", "area_conocimiento", int, int,
)
router_proyecto_linea = _crear_router_puente(
    "/api/proyecto-linea", "v2 - Proyecto Línea", "proyecto_linea",
    servicio_proyecto_linea, ProyectoLineaCrear, "proyecto", "linea_investigacion", int, int,
)
router_ods_proyecto = _crear_router_puente(
    "/api/ods-proyecto", "v2 - ODS Proyecto", "ods_proyecto",
    servicio_ods_proyecto, OdsProyectoCrear, "proyecto", "ods", int, int,
)
router_aa_proyecto = _crear_router_puente(
    "/api/aa-proyecto", "v2 - Área Aplicación Proyecto", "aa_proyecto",
    servicio_aa_proyecto, AaProyectoCrear, "proyecto", "area_aplicacion", int, int,
)
router_docente_producto = _crear_router_puente(
    "/api/docente-producto", "v2 - Docente Producto", "docente_producto",
    servicio_docente_producto, DocenteProductoCrear, "docente", "producto", int, int,
)
