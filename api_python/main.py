# ==============================================================
# PUNTO DE ENTRADA DE LA API REST — PYTHON + FASTAPI (v1 ⊂ v2 ⊂ v3)
# - Ensambla routers de las 3 versiones acumulativas.
# - Registra manejadores globales de excepciones para traducir errores de dominio y SQL
#   al formato JSON estándar de los contratos ({estado, mensaje, detalle/errores}).
# - Expone Swagger OpenAPI en /swagger y diagnóstico público en GET /.
# ==============================================================

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from excepciones import (
    NoEncontradoExcepcion,
    ConflictoExcepcion,
    ValidacionNegocioExcepcion,
    NoAutenticadoExcepcion,
    SinPermisoExcepcion,
)
from controladores import (
    router_producto,
    router_empresa,
    router_persona,
    router_rol,
    router_ruta,
    router_usuario,
    router_cliente,
    router_vendedor,
    router_factura,
    router_rol_usuario,
    router_rutarol,
    router_usuario_roles,
    router_sesion,
    router_permisos,
)

# Crea la instancia principal de FastAPI con Swagger en '/swagger'
app = FastAPI(
    title="API Facturas — Sistema Empresarial (Python / FastAPI)",
    description="API REST incremental (v1: 6 tablas planas, v2: 6 tablas con FK y SPs, v3: autenticación JWT + RBAC).",
    version="3.0.0",
    docs_url="/swagger",
    redoc_url="/redoc",
)


# ==============================================================
# TRADUCTORES GLOBALES DE ERRORES AL CONTRATO HTTP REST
# ==============================================================

@app.exception_handler(RequestValidationError)
async def manejar_422(_req: Request, exc: RequestValidationError):
    """Traduce fallos de esquema/validación Pydantic al formato HTTP 422 del curso."""
    lista_errores = [f"{'.'.join(str(x) for x in err['loc'])}: {err['msg']}" for err in exc.errors()]
    return JSONResponse(
        status_code=422,
        content={"estado": 422, "mensaje": "Datos inválidos.", "errores": lista_errores},
    )


@app.exception_handler(ValidacionNegocioExcepcion)
async def manejar_400(_req: Request, exc: ValidacionNegocioExcepcion):
    """Traduce errores de argumentos o reglas de negocio a HTTP 400 Bad Request."""
    return JSONResponse(status_code=400, content={"estado": 400, "mensaje": "Parámetros inválidos.", "detalle": str(exc)})


@app.exception_handler(NoAutenticadoExcepcion)
async def manejar_401(_req: Request, exc: NoAutenticadoExcepcion):
    """Traduce falta de token, firma inválida, vencimiento o credenciales malas a HTTP 401."""
    return JSONResponse(status_code=401, content={"estado": 401, "mensaje": str(exc)})


@app.exception_handler(SinPermisoExcepcion)
async def manejar_403(_req: Request, exc: SinPermisoExcepcion):
    """Traduce rechazo de verificar_acceso_ruta en SQL Server a HTTP 403 con el nombre de ruta."""
    return JSONResponse(status_code=403, content={"estado": 403, "mensaje": str(exc), "ruta": exc.ruta})


@app.exception_handler(NoEncontradoExcepcion)
async def manejar_404(_req: Request, exc: NoEncontradoExcepcion):
    """Traduce recurso inexistente a HTTP 404 Not Found."""
    return JSONResponse(status_code=404, content={"estado": 404, "mensaje": "Recurso no encontrado.", "detalle": str(exc)})


@app.exception_handler(ConflictoExcepcion)
async def manejar_409(_req: Request, exc: ConflictoExcepcion):
    """Traduce errores relacionales (547 FK, 2627/2601 duplicados, anulación doble) a HTTP 409 Conflict."""
    return JSONResponse(status_code=409, content={"estado": 409, "mensaje": "Conflicto con el estado actual de los datos.", "detalle": str(exc)})


@app.exception_handler(Exception)
async def manejar_500(_req: Request, exc: Exception):
    """Red de seguridad final ante fallos imprevistos del servidor o desconexión de BD."""
    return JSONResponse(status_code=500, content={"estado": 500, "mensaje": "Error interno del servidor.", "detalle": str(exc)})


# ==============================================================
# DIAGNÓSTICO ABIERTO EN LA RAÍZ (GET /)
# ==============================================================
@app.get("/", tags=["Diagnóstico"])
def diagnostico_raiz():
    """Endpoint público de salud para verificar versión activa y contrato OpenAPI."""
    return {
        "mensaje": "API Facturas funcionando",
        "version": "v3",
        "stack": "Python / FastAPI + SQL Server",
        "contratos": "docs/spec_kit/versiones/v3_control_acceso/6_contracts.md",
    }


# ==============================================================
# REGISTRO DE ROUTERS POR VERSIÓN
# ==============================================================
# Versión 1: Las 6 tablas independientes
for r_v1 in (router_producto, router_empresa, router_persona, router_rol, router_ruta, router_usuario):
    app.include_router(r_v1)

# Versión 2: Las 6 tablas dependientes + recurso compuesto usuario-con-roles
for r_v2 in (router_cliente, router_vendedor, router_factura, router_rol_usuario, router_rutarol, router_usuario_roles):
    app.include_router(r_v2)

# Versión 3: Autenticación (/api/sesion) y permisos propios (/api/permisos/mios)
app.include_router(router_sesion)
app.include_router(router_permisos)
