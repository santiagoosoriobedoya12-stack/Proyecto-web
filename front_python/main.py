# ==============================================================
# SERVIDOR WEB FRONTEND EN PYTHON — FASTAPI + JINJA2 + HTTPX (v1..v3)
# Reemplaza 100% al frontend Blazor manteniendo:
# - Sesiones JWT guardadas únicamente en memoria del servidor (cookie HttpOnly).
# - Carga concurrente con asyncio.gather (equivalente a Task.WhenAll).
# - Menú dinámico según /api/permisos/mios y verificación real 401/403 desde la API.
# - Resiliencia completa ante caída de la API (banner de alerta sin colapsar la app).
# ==============================================================

import asyncio
from urllib.parse import quote
from fastapi import FastAPI, Request, Form
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from servicios import (
    COOKIE_SESION,
    obtener_sesion,
    crear_sesion_servidor,
    cerrar_sesion_servidor,
    llamar_api,
)

app = FastAPI(title="FactuCorp ERP Frontend (Python)")

# Sirve Bootstrap local y app.css desde /static sin depender de internet ni CDN
app.mount("/static", StaticFiles(directory="static"), name="static")
templates = Jinja2Templates(directory="templates")


# ==============================================================
# CATÁLOGO CONFIGURABLE DE MÓDULOS CRUD (v1 y v2)
# Evita duplicar 8 rutas y 8 plantillas casi idénticas.
# ==============================================================
CRUD_CONFIGS = {
    "productos": {
        "titulo": "Gestión de Productos",
        "titulo_tabla": "Catálogo de Productos",
        "singular": "producto",
        "recurso_url": "productos",
        "endpoint": "api/producto",
        "pk": "codigo",
        "pk_tipo": "str",
        "campos": [
            {"nombre": "codigo", "etiqueta": "Código", "tipo": "text", "pk": True, "placeholder": "Ej: PR01", "ayuda": "Máximo 10 caracteres."},
            {"nombre": "nombre", "etiqueta": "Nombre", "tipo": "text", "placeholder": "Ej: Teclado Mecánico"},
            {"nombre": "stock", "etiqueta": "Stock", "tipo": "number", "step": "1", "placeholder": "10"},
            {"nombre": "valorunitario", "etiqueta": "Valor Unitario ($)", "tipo": "number", "step": "0.01", "placeholder": "50000"},
        ],
        "columnas": [
            {"titulo": "Código", "campo": "codigo", "codigo": True},
            {"titulo": "Nombre", "campo": "nombre"},
            {"titulo": "Stock", "campo": "stock"},
            {"titulo": "Valor Unitario", "campo": "valorunitario"},
        ],
    },
    "empresas": {
        "titulo": "Gestión de Empresas",
        "titulo_tabla": "Listado de Empresas",
        "singular": "empresa",
        "recurso_url": "empresas",
        "endpoint": "api/empresa",
        "pk": "codigo",
        "pk_tipo": "str",
        "campos": [
            {"nombre": "codigo", "etiqueta": "Código", "tipo": "text", "pk": True, "placeholder": "Ej: E001"},
            {"nombre": "nombre", "etiqueta": "Razón Social / Nombre", "tipo": "text", "placeholder": "Comercial Los Andes S.A."},
        ],
        "columnas": [
            {"titulo": "Código", "campo": "codigo", "codigo": True},
            {"titulo": "Nombre", "campo": "nombre"},
        ],
    },
    "personas": {
        "titulo": "Gestión de Personas",
        "titulo_tabla": "Listado de Personas",
        "singular": "persona",
        "recurso_url": "personas",
        "endpoint": "api/persona",
        "pk": "codigo",
        "pk_tipo": "str",
        "campos": [
            {"nombre": "codigo", "etiqueta": "Código", "tipo": "text", "pk": True, "placeholder": "Ej: P001"},
            {"nombre": "nombre", "etiqueta": "Nombre Completo", "tipo": "text", "placeholder": "Ana Torres"},
            {"nombre": "email", "etiqueta": "Correo Electrónico", "tipo": "email", "placeholder": "ana@correo.com"},
            {"nombre": "telefono", "etiqueta": "Teléfono", "tipo": "text", "placeholder": "3001234567"},
        ],
        "columnas": [
            {"titulo": "Código", "campo": "codigo", "codigo": True},
            {"titulo": "Nombre", "campo": "nombre"},
            {"titulo": "Email", "campo": "email"},
            {"titulo": "Teléfono", "campo": "telefono"},
        ],
    },
    "roles": {
        "titulo": "Gestión de Roles",
        "titulo_tabla": "Catálogo de Roles",
        "singular": "rol",
        "recurso_url": "roles",
        "endpoint": "api/rol",
        "pk": "id",
        "pk_tipo": "int",
        "campos": [
            {"nombre": "id", "etiqueta": "ID", "tipo": "number", "pk": True, "ayuda": "Dejar vacío al crear (IDENTITY autogenerado)."},
            {"nombre": "nombre", "etiqueta": "Nombre del Rol", "tipo": "text", "placeholder": "Ej: Supervisor"},
        ],
        "columnas": [
            {"titulo": "ID", "campo": "id", "codigo": True},
            {"titulo": "Nombre", "campo": "nombre"},
        ],
    },
    "rutas": {
        "titulo": "Gestión de Rutas del Sistema",
        "titulo_tabla": "Rutas Protegibles (RBAC)",
        "singular": "ruta",
        "recurso_url": "rutas",
        "endpoint": "api/ruta",
        "pk": "id",
        "pk_tipo": "int",
        "campos": [
            {"nombre": "id", "etiqueta": "ID", "tipo": "number", "pk": True, "ayuda": "Dejar vacío al crear (IDENTITY autogenerado)."},
            {"nombre": "ruta", "etiqueta": "Patrón de Ruta", "tipo": "text", "placeholder": "Ej: interfaz.reportes"},
            {"nombre": "descripcion", "etiqueta": "Descripción", "tipo": "text", "placeholder": "Acceso al módulo de reportes"},
        ],
        "columnas": [
            {"titulo": "ID", "campo": "id", "codigo": True},
            {"titulo": "Ruta", "campo": "ruta", "codigo": True},
            {"titulo": "Descripción", "campo": "descripcion"},
        ],
    },
    "usuarios": {
        "titulo": "Gestión de Usuarios",
        "titulo_tabla": "Usuarios Registrados (hash BCrypt)",
        "singular": "usuario",
        "recurso_url": "usuarios",
        "endpoint": "api/usuario",
        "pk": "email",
        "pk_tipo": "str",
        "campos": [
            {"nombre": "email", "etiqueta": "Correo Electrónico", "tipo": "email", "pk": True, "placeholder": "usuario@correo.com"},
            {"nombre": "contrasena", "etiqueta": "Contraseña", "tipo": "password", "placeholder": "En edición, vacío = conservar clave actual"},
        ],
        "columnas": [
            {"titulo": "Email", "campo": "email", "codigo": True},
        ],
    },
    "clientes": {
        "titulo": "Gestión de Clientes (v2 con FK)",
        "titulo_tabla": "Clientes Registrados",
        "singular": "cliente",
        "recurso_url": "clientes",
        "endpoint": "api/cliente",
        "pk": "id",
        "pk_tipo": "int",
        "campos": [
            {"nombre": "id", "etiqueta": "ID", "tipo": "number", "pk": True, "ayuda": "Dejar vacío al crear (IDENTITY autogenerado)."},
            {"nombre": "credito", "etiqueta": "Cupo de Crédito ($)", "tipo": "number", "step": "0.01", "placeholder": "500000"},
            {"nombre": "fkcodpersona", "etiqueta": "Persona Titular (FK obligatoria)", "tipo": "select", "select_key": "personas", "opcional": False},
            {"nombre": "fkcodempresa", "etiqueta": "Empresa Asociada (FK opcional)", "tipo": "select", "select_key": "empresas", "opcional": True},
        ],
        "columnas": [
            {"titulo": "ID", "campo": "id", "codigo": True},
            {"titulo": "Persona", "campo": "persona_display"},
            {"titulo": "Empresa", "campo": "empresa_display"},
            {"titulo": "Crédito", "campo": "credito"},
        ],
    },
    "vendedores": {
        "titulo": "Gestión de Vendedores (v2 con FK)",
        "titulo_tabla": "Fuerza Comercial",
        "singular": "vendedor",
        "recurso_url": "vendedores",
        "endpoint": "api/vendedor",
        "pk": "id",
        "pk_tipo": "int",
        "campos": [
            {"nombre": "id", "etiqueta": "ID", "tipo": "number", "pk": True, "ayuda": "Dejar vacío al crear (IDENTITY autogenerado)."},
            {"nombre": "carnet", "etiqueta": "Carnet", "tipo": "number", "step": "1", "placeholder": "1005"},
            {"nombre": "direccion", "etiqueta": "Dirección", "tipo": "text", "placeholder": "Calle 10 # 20-30"},
            {"nombre": "fkcodpersona", "etiqueta": "Persona (FK obligatoria)", "tipo": "select", "select_key": "personas", "opcional": False},
        ],
        "columnas": [
            {"titulo": "ID", "campo": "id", "codigo": True},
            {"titulo": "Carnet", "campo": "carnet", "codigo": True},
            {"titulo": "Persona", "campo": "persona_display"},
            {"titulo": "Dirección", "campo": "direccion"},
        ],
    },
}


def _sesion_req(request: Request) -> dict | None:
    return obtener_sesion(request.cookies.get(COOKIE_SESION))


# ==============================================================
# 1. INICIO Y AUTENTICACIÓN (v1 y v3)
# ==============================================================

@app.get("/", response_class=HTMLResponse)
def inicio(request: Request):
    return templates.TemplateResponse("home.html", {"request": request, "sesion": _sesion_req(request)})


@app.get("/sesion", response_class=HTMLResponse)
@app.get("/login", response_class=HTMLResponse)
def ver_sesion(request: Request):
    return templates.TemplateResponse("sesion.html", {"request": request, "sesion": _sesion_req(request), "error": None})


@app.post("/sesion", response_class=HTMLResponse)
@app.post("/login", response_class=HTMLResponse)
async def iniciar_sesion(request: Request, email: str = Form(...), contrasena: str = Form(...)):
    res = await llamar_api("POST", "api/sesion", json_body={"email": email.strip(), "contrasena": contrasena})
    if not res["ok"]:
        return templates.TemplateResponse("sesion.html", {"request": request, "sesion": None, "error": res["mensaje"]})

    data = res["data"]
    token = data["token"]
    roles = data.get("roles", [])
    temp_sesion = {"token": token, "email": data["email"], "roles": roles, "permisos": []}

    # Consulta inmediatamente GET /api/permisos/mios para armar el menú dinámico
    perm_res = await llamar_api("GET", "api/permisos/mios", sesion=temp_sesion)
    permisos = perm_res["data"].get("datos", []) if perm_res["ok"] and isinstance(perm_res["data"], dict) else []

    sid = crear_sesion_servidor(token, data["email"], roles, permisos)
    resp = RedirectResponse(url="/", status_code=303)
    resp.set_cookie(key=COOKIE_SESION, value=sid, httponly=True, samesite="lax")
    return resp


@app.post("/cerrar-sesion")
def cerrar_sesion(request: Request):
    cerrar_sesion_servidor(request.cookies.get(COOKIE_SESION))
    resp = RedirectResponse(url="/sesion", status_code=303)
    resp.delete_cookie(COOKIE_SESION)
    return resp


# ==============================================================
# 2. MÓDULOS ESPECIALIZADOS VERSIÓN 2 (FACTURAS, PUENTES, USUARIOS+ROLES)
#    Se registran ANTES de /{recurso} para garantizar prioridad de ruta.
# ==============================================================

@app.get("/facturas", response_class=HTMLResponse)
async def ver_facturas(request: Request, msg: str | None = None, ok: str = "1"):
    sesion = _sesion_req(request)
    res = await llamar_api("GET", "api/factura", sesion=sesion)
    facturas = (res["data"] or {}).get("datos", []) if res["ok"] and res["data"] else []
    return templates.TemplateResponse("facturas.html", {
        "request": request,
        "sesion": sesion,
        "facturas": facturas,
        "error_conexion": res["error_conexion"],
        "mensaje_error": res["mensaje"],
        "sin_permiso": res["sin_permiso"],
        "no_autenticado": res.get("no_autenticado", False),
        "mensaje": msg,
        "exito": ok == "1",
    })


@app.post("/facturas/anular")
async def anular_factura(request: Request, numero: int = Form(...)):
    sesion = _sesion_req(request)
    res = await llamar_api("DELETE", f"api/factura/{numero}", sesion=sesion)
    ok_flag = "1" if res["ok"] else "0"
    msg = f"Factura #{numero} anulada y stock restaurado en SQL Server." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/facturas?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.get("/facturas/{numero}", response_class=HTMLResponse)
async def ver_factura_detalle(request: Request, numero: int):
    sesion = _sesion_req(request)
    res = await llamar_api("GET", f"api/factura/{numero}", sesion=sesion)
    return templates.TemplateResponse("factura_detalle.html", {
        "request": request,
        "sesion": sesion,
        "numero": numero,
        "detalle": res["data"] if res["ok"] else None,
        "error": None if res["ok"] else res["mensaje"],
    })


@app.get("/emitir-factura", response_class=HTMLResponse)
async def ver_emitir_factura(request: Request):
    sesion = _sesion_req(request)
    r_cli, r_ven, r_pro, r_per = await asyncio.gather(
        llamar_api("GET", "api/cliente?limite=1000", sesion=sesion),
        llamar_api("GET", "api/vendedor?limite=1000", sesion=sesion),
        llamar_api("GET", "api/producto?limite=1000", sesion=sesion),
        llamar_api("GET", "api/persona?limite=1000", sesion=sesion),
    )
    personas = {p["codigo"]: p["nombre"] for p in ((r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else [])}
    clientes = (r_cli["data"] or {}).get("datos", []) if r_cli["ok"] and r_cli["data"] else []
    for c in clientes:
        c["nombre_persona"] = personas.get(c["fkcodpersona"], c["fkcodpersona"])
    vendedores = (r_ven["data"] or {}).get("datos", []) if r_ven["ok"] and r_ven["data"] else []
    for v in vendedores:
        v["nombre_persona"] = personas.get(v["fkcodpersona"], v["fkcodpersona"])
    productos = (r_pro["data"] or {}).get("datos", []) if r_pro["ok"] and r_pro["data"] else []

    return templates.TemplateResponse("emitir_factura.html", {
        "request": request,
        "sesion": sesion,
        "clientes": clientes,
        "vendedores": vendedores,
        "productos": productos,
        "error_conexion": r_cli["error_conexion"],
        "mensaje_error": r_cli["mensaje"],
        "sin_permiso": r_cli["sin_permiso"],
        "no_autenticado": r_cli.get("no_autenticado", False),
        "mensaje": None,
        "exito": True,
    })


@app.post("/emitir-factura")
async def procesar_emitir_factura(request: Request):
    sesion = _sesion_req(request)
    form = await request.form()
    fkidcliente = int(form.get("fkidcliente"))
    fkidvendedor = int(form.get("fkidvendedor"))
    codigos = form.getlist("fkcodproducto")
    cantidades = form.getlist("cantidad")

    productos_payload = [{"fkcodproducto": cod, "cantidad": int(cant)} for cod, cant in zip(codigos, cantidades)]
    res = await llamar_api("POST", "api/factura", sesion=sesion, json_body={
        "fkidcliente": fkidcliente,
        "fkidvendedor": fkidvendedor,
        "productos": productos_payload,
    })
    if res["ok"]:
        num = res["data"]["factura"]["numero"]
        return RedirectResponse(url=f"/facturas/{num}", status_code=303)
    return RedirectResponse(url=f"/facturas?msg={quote(res['mensaje'])}&ok=0", status_code=303)


@app.get("/roles-por-usuario", response_class=HTMLResponse)
async def ver_roles_por_usuario(request: Request, msg: str | None = None, ok: str = "1"):
    sesion = _sesion_req(request)
    r_asig, r_usr, r_rol = await asyncio.gather(
        llamar_api("GET", "api/rol-usuario", sesion=sesion),
        llamar_api("GET", "api/usuario?limite=1000", sesion=sesion),
        llamar_api("GET", "api/rol?limite=1000", sesion=sesion),
    )
    return templates.TemplateResponse("roles_por_usuario.html", {
        "request": request,
        "sesion": sesion,
        "asignaciones": (r_asig["data"] or {}).get("datos", []) if r_asig["ok"] and r_asig["data"] else [],
        "usuarios": (r_usr["data"] or {}).get("datos", []) if r_usr["ok"] and r_usr["data"] else [],
        "roles": (r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else [],
        "error_conexion": r_asig["error_conexion"],
        "mensaje_error": r_asig["mensaje"],
        "sin_permiso": r_asig["sin_permiso"],
        "no_autenticado": r_asig.get("no_autenticado", False),
        "mensaje": msg,
        "exito": ok == "1",
    })


@app.post("/roles-por-usuario/crear")
async def crear_rol_usuario_front(request: Request, fkemail: str = Form(...), fkidrol: int = Form(...)):
    res = await llamar_api("POST", "api/rol-usuario", sesion=_sesion_req(request), json_body={"fkemail": fkemail, "fkidrol": fkidrol})
    ok_flag = "1" if res["ok"] else "0"
    msg = "Rol asignado correctamente." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/roles-por-usuario?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.post("/roles-por-usuario/eliminar")
async def eliminar_rol_usuario_front(request: Request, fkemail: str = Form(...), fkidrol: int = Form(...)):
    res = await llamar_api("DELETE", f"api/rol-usuario/{quote(fkemail)}/{fkidrol}", sesion=_sesion_req(request))
    ok_flag = "1" if res["ok"] else "0"
    msg = "Asignación removida." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/roles-por-usuario?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.get("/permisos-por-rol", response_class=HTMLResponse)
async def ver_permisos_por_rol(request: Request, msg: str | None = None, ok: str = "1"):
    sesion = _sesion_req(request)
    r_perm, r_rut, r_rol = await asyncio.gather(
        llamar_api("GET", "api/rutarol", sesion=sesion),
        llamar_api("GET", "api/ruta?limite=1000", sesion=sesion),
        llamar_api("GET", "api/rol?limite=1000", sesion=sesion),
    )
    return templates.TemplateResponse("permisos_por_rol.html", {
        "request": request,
        "sesion": sesion,
        "permisos": (r_perm["data"] or {}).get("datos", []) if r_perm["ok"] and r_perm["data"] else [],
        "rutas": (r_rut["data"] or {}).get("datos", []) if r_rut["ok"] and r_rut["data"] else [],
        "roles": (r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else [],
        "error_conexion": r_perm["error_conexion"],
        "mensaje_error": r_perm["mensaje"],
        "sin_permiso": r_perm["sin_permiso"],
        "no_autenticado": r_perm.get("no_autenticado", False),
        "mensaje": msg,
        "exito": ok == "1",
    })


@app.post("/permisos-por-rol/crear")
async def crear_permiso_front(request: Request, fkidruta: int = Form(...), fkidrol: int = Form(...)):
    res = await llamar_api("POST", "api/rutarol", sesion=_sesion_req(request), json_body={"fkidruta": fkidruta, "fkidrol": fkidrol})
    ok_flag = "1" if res["ok"] else "0"
    msg = "Permiso otorgado inmediatamente." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/permisos-por-rol?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.post("/permisos-por-rol/eliminar")
async def eliminar_permiso_front(request: Request, fkidruta: int = Form(...), fkidrol: int = Form(...)):
    res = await llamar_api("DELETE", f"api/rutarol/{fkidruta}/{fkidrol}", sesion=_sesion_req(request))
    ok_flag = "1" if res["ok"] else "0"
    msg = "Permiso revocado inmediatamente." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/permisos-por-rol?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.get("/usuarios-y-roles", response_class=HTMLResponse)
async def ver_usuarios_y_roles(request: Request, editar: str | None = None, msg: str | None = None, ok: str = "1"):
    sesion = _sesion_req(request)
    r_usr, r_rol = await asyncio.gather(
        llamar_api("GET", "api/usuario-con-roles", sesion=sesion),
        llamar_api("GET", "api/rol?limite=1000", sesion=sesion),
    )
    usuarios = (r_usr["data"] or {}).get("datos", []) if r_usr["ok"] and r_usr["data"] else []
    roles = (r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else []
    roles_sel = []
    if editar:
        for u in usuarios:
            if u["email"] == editar:
                roles_sel = [r["idrol"] for r in u.get("roles", [])]
                break

    return templates.TemplateResponse("usuarios_y_roles.html", {
        "request": request,
        "sesion": sesion,
        "usuarios": usuarios,
        "roles": roles,
        "editando_email": editar,
        "roles_seleccionados": roles_sel,
        "error_conexion": r_usr["error_conexion"],
        "mensaje_error": r_usr["mensaje"],
        "sin_permiso": r_usr["sin_permiso"],
        "no_autenticado": r_usr.get("no_autenticado", False),
        "mensaje": msg,
        "exito": ok == "1",
        "errores": [],
    })


@app.post("/usuarios-y-roles/guardar")
async def guardar_usuario_con_roles_front(request: Request):
    sesion = _sesion_req(request)
    form = await request.form()
    es_edicion = form.get("es_edicion") == "1"
    email = form.get("email", "").strip()
    contrasena = form.get("contrasena", "")
    roles_ids = [int(x) for x in form.getlist("roles")]

    if es_edicion:
        res = await llamar_api("PUT", f"api/usuario-con-roles/{quote(email)}", sesion=sesion, json_body={
            "contrasena": contrasena,
            "roles": roles_ids,
        })
    else:
        res = await llamar_api("POST", "api/usuario-con-roles", sesion=sesion, json_body={
            "email": email,
            "contrasena": contrasena,
            "roles": roles_ids,
        })
    ok_flag = "1" if res["ok"] else "0"
    msg = "Usuario y roles guardados con OPENJSON." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/usuarios-y-roles?msg={quote(msg)}&ok={ok_flag}", status_code=303)


@app.post("/usuarios-y-roles/eliminar")
async def eliminar_usuario_con_roles_front(request: Request, email: str = Form(...)):
    res = await llamar_api("DELETE", f"api/usuario-con-roles/{quote(email)}", sesion=_sesion_req(request))
    ok_flag = "1" if res["ok"] else "0"
    msg = "Usuario eliminado transaccionalmente." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/usuarios-y-roles?msg={quote(msg)}&ok={ok_flag}", status_code=303)


# ==============================================================
# 3. RUTAS GENÉRICAS PARA LOS 8 CRUDs TABULARES (SIEMPRE AL FINAL)
# ==============================================================

async def _cargar_selects_crud(recurso: str, sesion: dict | None) -> dict:
    """Usa asyncio.gather (equivalente a Task.WhenAll) para cargar FK selects en paralelo."""
    selects = {}
    if recurso == "clientes":
        r_per, r_emp = await asyncio.gather(
            llamar_api("GET", "api/persona?limite=1000", sesion=sesion),
            llamar_api("GET", "api/empresa?limite=1000", sesion=sesion),
        )
        personas = (r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else []
        empresas = (r_emp["data"] or {}).get("datos", []) if r_emp["ok"] and r_emp["data"] else []
        selects["personas"] = [{"valor": p["codigo"], "etiqueta": f"{p['codigo']} — {p['nombre']}"} for p in personas]
        selects["empresas"] = [{"valor": e["codigo"], "etiqueta": f"{e['codigo']} — {e['nombre']}"} for e in empresas]
        selects["_map_personas"] = {p["codigo"]: p["nombre"] for p in personas}
        selects["_map_empresas"] = {e["codigo"]: e["nombre"] for e in empresas}
    elif recurso == "vendedores":
        r_per = await llamar_api("GET", "api/persona?limite=1000", sesion=sesion)
        personas = (r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else []
        selects["personas"] = [{"valor": p["codigo"], "etiqueta": f"{p['codigo']} — {p['nombre']}"} for p in personas]
        selects["_map_personas"] = {p["codigo"]: p["nombre"] for p in personas}
    return selects


@app.get("/{recurso}", response_class=HTMLResponse)
async def ver_crud(request: Request, recurso: str, editar: str | None = None, msg: str | None = None, ok: str = "1"):
    if recurso not in CRUD_CONFIGS:
        return HTMLResponse("Recurso no encontrado", status_code=404)

    cfg = CRUD_CONFIGS[recurso]
    sesion = _sesion_req(request)

    res_list, selects = await asyncio.gather(
        llamar_api("GET", f"{cfg['endpoint']}?limite=1000", sesion=sesion),
        _cargar_selects_crud(recurso, sesion),
    )

    filas = (res_list["data"] or {}).get("datos", []) if res_list["ok"] and res_list["data"] else []
    if recurso == "clientes":
        mp = selects.get("_map_personas", {})
        me = selects.get("_map_empresas", {})
        for f in filas:
            f["persona_display"] = f"{mp.get(f['fkcodpersona'], '')} ({f['fkcodpersona']})"
            f["empresa_display"] = f"{me.get(f['fkcodempresa'], '')} ({f['fkcodempresa']})" if f.get("fkcodempresa") else "Particular"
    elif recurso == "vendedores":
        mp = selects.get("_map_personas", {})
        for f in filas:
            f["persona_display"] = f"{mp.get(f['fkcodpersona'], '')} ({f['fkcodpersona']})"

    valores = {}
    editando = False
    if editar:
        for f in filas:
            if str(f[cfg["pk"]]) == str(editar):
                valores = dict(f)
                editando = True
                break

    return templates.TemplateResponse("crud_generico.html", {
        "request": request,
        "sesion": sesion,
        "config": cfg,
        "filas": filas,
        "selects": selects,
        "editando": editando,
        "clave_edicion": editar if editando else None,
        "valores": valores,
        "error_conexion": res_list["error_conexion"],
        "mensaje_error": res_list["mensaje"],
        "sin_permiso": res_list["sin_permiso"],
        "no_autenticado": res_list.get("no_autenticado", False),
        "mensaje": msg,
        "exito": ok == "1",
        "errores": [],
    })


@app.post("/{recurso}/accion", response_class=HTMLResponse)
async def accion_crud(request: Request, recurso: str):
    if recurso not in CRUD_CONFIGS:
        return HTMLResponse("Recurso no encontrado", status_code=404)
    cfg = CRUD_CONFIGS[recurso]
    sesion = _sesion_req(request)
    form = await request.form()
    modo = form.get("modo", "crear")
    clave_edicion = form.get("clave_edicion", "")

    def parsear_campo(campo, raw_val):
        if raw_val is None or str(raw_val).strip() == "":
            return None
        s = str(raw_val).strip()
        if campo["tipo"] == "number":
            return float(s) if campo.get("step") == "0.01" else int(float(s))
        return s

    body = {}
    for c in cfg["campos"]:
        nombre = c["nombre"]
        if modo in ("put", "patch") and c.get("pk"):
            continue
        if modo == "crear" and c.get("pk") and cfg["pk_tipo"] == "int":
            continue
        val = parsear_campo(c, form.get(nombre))
        if val is not None:
            body[nombre] = val
        elif c.get("opcional") and modo in ("crear", "put"):
            body[nombre] = None

    if recurso == "usuarios" and modo == "put" and "contrasena" not in body:
        body["contrasena"] = ""

    if modo == "crear":
        res = await llamar_api("POST", cfg["endpoint"], sesion=sesion, json_body=body)
    elif modo == "put":
        res = await llamar_api("PUT", f"{cfg['endpoint']}/{quote(str(clave_edicion))}", sesion=sesion, json_body=body)
    else:
        orig_res = await llamar_api("GET", f"{cfg['endpoint']}/{quote(str(clave_edicion))}", sesion=sesion)
        cambios = {}
        if orig_res["ok"] and isinstance(orig_res["data"], dict):
            orig = orig_res["data"]
            for k, v in body.items():
                if orig.get(k) != v:
                    cambios[k] = v
        else:
            cambios = body
        if recurso == "usuarios" and form.get("contrasena", "").strip():
            cambios["contrasena"] = form.get("contrasena", "").strip()
        res = await llamar_api("PATCH", f"{cfg['endpoint']}/{quote(str(clave_edicion))}", sesion=sesion, json_body=cambios)

    if res["ok"]:
        return RedirectResponse(url=f"/{recurso}?msg={quote('Operación realizada con éxito.')}&ok=1", status_code=303)

    res_list, selects = await asyncio.gather(
        llamar_api("GET", f"{cfg['endpoint']}?limite=1000", sesion=sesion),
        _cargar_selects_crud(recurso, sesion),
    )
    filas = (res_list["data"] or {}).get("datos", []) if res_list["ok"] and res_list["data"] else []
    return templates.TemplateResponse("crud_generico.html", {
        "request": request,
        "sesion": sesion,
        "config": cfg,
        "filas": filas,
        "selects": selects,
        "editando": modo in ("put", "patch"),
        "clave_edicion": clave_edicion if modo in ("put", "patch") else None,
        "valores": dict(form),
        "error_conexion": res["error_conexion"],
        "mensaje_error": res["mensaje"],
        "sin_permiso": res["sin_permiso"],
        "no_autenticado": res.get("no_autenticado", False),
        "mensaje": res["mensaje"],
        "exito": False,
        "errores": res["errores"],
    })


@app.post("/{recurso}/eliminar")
async def eliminar_crud(request: Request, recurso: str, clave: str = Form(...)):
    if recurso not in CRUD_CONFIGS:
        return HTMLResponse("Recurso no encontrado", status_code=404)
    cfg = CRUD_CONFIGS[recurso]
    sesion = _sesion_req(request)
    res = await llamar_api("DELETE", f"{cfg['endpoint']}/{quote(str(clave))}", sesion=sesion)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Registro eliminado correctamente." if res["ok"] else res["mensaje"]
    return RedirectResponse(url=f"/{recurso}?msg={quote(msg)}&ok={ok_flag}", status_code=303)
