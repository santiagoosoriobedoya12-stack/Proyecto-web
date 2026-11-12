# ==============================================================
# PUNTO DE ENTRADA DE LA API REST — MAPA DE CONOCIMIENTO (FastAPI)
# Versiones acumulativas implementadas: v1 (8 sin FK) ⊂ v2 (10 con FK) ⊂ v3 (JWT + Roles)
# Expone Swagger OpenAPI en /swagger y diagnóstico en GET /.
# ==============================================================

from contextlib import asynccontextmanager
from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from controladores import (
    router_area_conocimiento,
    router_ods,
    router_area_aplicacion,
    router_termino_clave,
    router_linea_investigacion,
    router_aliado,
    router_tipo_producto,
    router_proyecto,
    router_docente,
    router_producto,
    router_desarrolla,
    router_aliado_proyecto,
    router_palabras_clave,
    router_ac_proyecto,
    router_proyecto_linea,
    router_ods_proyecto,
    router_aa_proyecto,
    router_docente_producto,
    router_login,
    router_rol,
    router_usuario,
    router_rol_usuario,
)
from excepciones import (
    ConflictoExcepcion,
    NoAutenticadoExcepcion,
    NoEncontradoExcepcion,
    SinPermisoExcepcion,
    ValidacionNegocioExcepcion,
)
from semilla import asegurar_usuarios_semilla


@asynccontextmanager
async def ciclo_vida(_app: FastAPI):
    asegurar_usuarios_semilla()
    yield


app = FastAPI(
    title="API Mapa de Conocimiento (Python / FastAPI)",
    description="API REST para las 21 tablas del módulo Mapa de Conocimiento (v1: 8 tablas sin FK, v2: 10 tablas con FK y puentes, v3: JWT y roles).",
    version="3.0.0",
    docs_url="/swagger",
    redoc_url="/redoc",
    lifespan=ciclo_vida,
)


@app.exception_handler(RequestValidationError)
async def manejar_422(_req: Request, exc: RequestValidationError):
    errores = [f"{'.'.join(str(x) for x in err['loc'])}: {err['msg']}" for err in exc.errors()]
    return JSONResponse(status_code=422, content={"estado": 422, "mensaje": "Datos inválidos.", "errores": errores})


@app.exception_handler(ValidacionNegocioExcepcion)
async def manejar_400(_req: Request, exc: ValidacionNegocioExcepcion):
    return JSONResponse(status_code=400, content={"estado": 400, "mensaje": "Solicitud inválida.", "detalle": str(exc)})


@app.exception_handler(NoAutenticadoExcepcion)
async def manejar_401(_req: Request, exc: NoAutenticadoExcepcion):
    return JSONResponse(status_code=401, content={"estado": 401, "mensaje": str(exc)})


@app.exception_handler(SinPermisoExcepcion)
async def manejar_403(_req: Request, exc: SinPermisoExcepcion):
    return JSONResponse(status_code=403, content={"estado": 403, "mensaje": str(exc), "ruta": exc.ruta})


@app.exception_handler(NoEncontradoExcepcion)
async def manejar_404(_req: Request, exc: NoEncontradoExcepcion):
    return JSONResponse(status_code=404, content={"estado": 404, "mensaje": "Recurso no encontrado.", "detalle": str(exc)})


@app.exception_handler(ConflictoExcepcion)
async def manejar_409(_req: Request, exc: ConflictoExcepcion):
    return JSONResponse(status_code=409, content={"estado": 409, "mensaje": "Conflicto de datos.", "detalle": str(exc)})


@app.exception_handler(Exception)
async def manejar_500(_req: Request, exc: Exception):
    return JSONResponse(status_code=500, content={"estado": 500, "mensaje": "Error interno del servidor.", "detalle": str(exc)})


@app.get("/", tags=["Diagnóstico"])
def diagnostico_raiz():
    return {
        "mensaje": "API Mapa de Conocimiento funcionando",
        "version": "v3",
        "stack": "Python / FastAPI + MariaDB",
        "tablas": 21,
    }


for r in (
    # v1: 8 sin FK
    router_area_conocimiento,
    router_ods,
    router_area_aplicacion,
    router_termino_clave,
    router_linea_investigacion,
    router_aliado,
    router_tipo_producto,
    router_proyecto,
    # v2: 10 con FK
    router_docente,
    router_producto,
    router_desarrolla,
    router_aliado_proyecto,
    router_palabras_clave,
    router_ac_proyecto,
    router_proyecto_linea,
    router_ods_proyecto,
    router_aa_proyecto,
    router_docente_producto,
    # v3: login + usuarios/roles
    router_login,
    router_rol,
    router_usuario,
    router_rol_usuario,
):
    app.include_router(r)
