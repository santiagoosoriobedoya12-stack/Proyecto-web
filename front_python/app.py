# ==============================================================
# SERVIDOR WEB FRONTEND EN PYTHON — FLASK + JINJA2 + HTTPX (v1..v3)
# - Frontend 100% en Flask con plantillas renderizadas en servidor (Jinja2).
# - Jamás importa pymssql ni toca SQL Server: consume únicamente la API FastAPI vía HTTP.
# - Rutas explícitas por recurso (sin rutas genéricas /{recurso}) y funciones dedicadas
#   en servicios/servicios_recursos.py.
# - Sesiones JWT en memoria del servidor protegidas mediante cookie HttpOnly.
# - Carga paralela de catálogos foráneos con ThreadPoolExecutor.
# ==============================================================

from concurrent.futures import ThreadPoolExecutor
from urllib.parse import quote
from flask import Flask, make_response, redirect, render_template, request
from servicios import (
    COOKIE_SESION,
    cerrar_sesion_servidor,
    crear_sesion_servidor,
    obtener_sesion,
    servicios_recursos as sr,
)

app = Flask(__name__, static_folder="static", template_folder="templates")


def _sesion_actual() -> dict | None:
    """Obtiene la sesión activa almacenada en el servidor a partir de la cookie HttpOnly."""
    return obtener_sesion(request.cookies.get(COOKIE_SESION))


# ==============================================================
# CATÁLOGO DE CONFIGURACIÓN VISUAL Y FUNCIONES POR RECURSO (v1 y v2)
# ==============================================================
CRUD_RECURSOS = {
    "productos": {
        "titulo": "Gestión de Productos",
        "titulo_tabla": "Catálogo de Productos",
        "singular": "producto",
        "recurso_url": "productos",
        "pk": "codigo",
        "pk_tipo": "str",
        "ops": {
            "listar": sr.listar_productos,
            "obtener": sr.obtener_producto,
            "crear": sr.crear_producto,
            "reemplazar": sr.reemplazar_producto,
            "actualizar": sr.actualizar_producto,
            "eliminar": sr.eliminar_producto,
        },
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
        "pk": "codigo",
        "pk_tipo": "str",
        "ops": {
            "listar": sr.listar_empresas,
            "obtener": sr.obtener_empresa,
            "crear": sr.crear_empresa,
            "reemplazar": sr.reemplazar_empresa,
            "actualizar": sr.actualizar_empresa,
            "eliminar": sr.eliminar_empresa,
        },
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
        "pk": "codigo",
        "pk_tipo": "str",
        "ops": {
            "listar": sr.listar_personas,
            "obtener": sr.obtener_persona,
            "crear": sr.crear_persona,
            "reemplazar": sr.reemplazar_persona,
            "actualizar": sr.actualizar_persona,
            "eliminar": sr.eliminar_persona,
        },
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
        "pk": "id",
        "pk_tipo": "int",
        "ops": {
            "listar": sr.listar_roles,
            "obtener": sr.obtener_rol,
            "crear": sr.crear_rol,
            "reemplazar": sr.reemplazar_rol,
            "actualizar": sr.actualizar_rol,
            "eliminar": sr.eliminar_rol,
        },
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
        "pk": "id",
        "pk_tipo": "int",
        "ops": {
            "listar": sr.listar_rutas,
            "obtener": sr.obtener_ruta,
            "crear": sr.crear_ruta,
            "reemplazar": sr.reemplazar_ruta,
            "actualizar": sr.actualizar_ruta,
            "eliminar": sr.eliminar_ruta,
        },
        "campos": [
            {"nombre": "id", "etiqueta": "ID", "tipo": "number", "pk": True, "ayuda": "Dejar vacío al crear (IDENTITY autogenerado)."},
            {"nombre": "ruta", "etiqueta": "Patrón de Ruta", "tipo": "text", "placeholder": "Ej: interfaz.reportes"},
            {"nombre": "descripcion", "etiqueta": "Descripción", "tipo": "text", "placeholder": "Acceso al módulo"},
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
        "pk": "email",
        "pk_tipo": "str",
        "ops": {
            "listar": sr.listar_usuarios,
            "obtener": sr.obtener_usuario,
            "crear": sr.crear_usuario,
            "reemplazar": sr.reemplazar_usuario,
            "actualizar": sr.actualizar_usuario,
            "eliminar": sr.eliminar_usuario,
        },
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
        "pk": "id",
        "pk_tipo": "int",
        "ops": {
            "listar": sr.listar_clientes,
            "obtener": sr.obtener_cliente,
            "crear": sr.crear_cliente,
            "reemplazar": sr.reemplazar_cliente,
            "actualizar": sr.actualizar_cliente,
            "eliminar": sr.eliminar_cliente,
        },
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
        "pk": "id",
        "pk_tipo": "int",
        "ops": {
            "listar": sr.listar_vendedores,
            "obtener": sr.obtener_vendedor,
            "crear": sr.crear_vendedor,
            "reemplazar": sr.reemplazar_vendedor,
            "actualizar": sr.actualizar_vendedor,
            "eliminar": sr.eliminar_vendedor,
        },
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


# ==============================================================
# 1. INICIO Y AUTENTICACIÓN (v1 y v3)
# ==============================================================

@app.get("/")
def inicio():
    return render_template("home.html", sesion=_sesion_actual())


@app.get("/sesion")
@app.get("/login")
def ver_sesion():
    return render_template("sesion.html", sesion=_sesion_actual(), error=None)


@app.post("/sesion")
@app.post("/login")
def iniciar_sesion():
    email = request.form.get("email", "")
    contrasena = request.form.get("contrasena", "")
    res = sr.iniciar_sesion_api(email, contrasena)
    if not res["ok"]:
        return render_template("sesion.html", sesion=None, error=res["mensaje"])

    data = res["data"]
    token = data["token"]
    roles = data.get("roles", [])
    temp_sesion = {"token": token, "email": data["email"], "roles": roles, "permisos": []}

    perm_res = sr.obtener_permisos_mios(temp_sesion)
    permisos = perm_res["data"].get("datos", []) if perm_res["ok"] and isinstance(perm_res["data"], dict) else []

    sid = crear_sesion_servidor(token, data["email"], roles, permisos)
    resp = make_response(redirect("/", code=303))
    resp.set_cookie(COOKIE_SESION, sid, httponly=True, samesite="Lax")
    return resp


@app.post("/cerrar-sesion")
def cerrar_sesion():
    cerrar_sesion_servidor(request.cookies.get(COOKIE_SESION))
    resp = make_response(redirect("/sesion", code=303))
    resp.delete_cookie(COOKIE_SESION)
    return resp


# ==============================================================
# 2. MÓDULOS ESPECIALIZADOS VERSIÓN 2 (FACTURAS, PUENTES, USUARIOS+ROLES)
# ==============================================================

@app.get("/facturas")
def ver_facturas():
    sesion = _sesion_actual()
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")
    res = sr.listar_facturas(sesion)
    facturas = (res["data"] or {}).get("datos", []) if res["ok"] and res["data"] else []
    return render_template(
        "facturas.html",
        sesion=sesion,
        facturas=facturas,
        error_conexion=res["error_conexion"],
        mensaje_error=res["mensaje"],
        sin_permiso=res["sin_permiso"],
        no_autenticado=res.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
    )


@app.post("/facturas/anular")
def anular_factura():
    sesion = _sesion_actual()
    numero = int(request.form.get("numero", "0"))
    res = sr.anular_factura_api(sesion, numero)
    ok_flag = "1" if res["ok"] else "0"
    msg = f"Factura #{numero} anulada y stock restaurado en SQL Server." if res["ok"] else res["mensaje"]
    return redirect(f"/facturas?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.get("/facturas/<int:numero>")
def ver_factura_detalle(numero: int):
    sesion = _sesion_actual()
    res = sr.obtener_factura(sesion, numero)
    return render_template(
        "factura_detalle.html",
        sesion=sesion,
        numero=numero,
        detalle=res["data"] if res["ok"] else None,
        error=None if res["ok"] else res["mensaje"],
    )


@app.get("/emitir-factura")
def ver_emitir_factura():
    sesion = _sesion_actual()
    with ThreadPoolExecutor(max_workers=4) as pool:
        f_cli = pool.submit(sr.listar_clientes, sesion)
        f_ven = pool.submit(sr.listar_vendedores, sesion)
        f_pro = pool.submit(sr.listar_productos, sesion)
        f_per = pool.submit(sr.listar_personas, sesion)
        r_cli, r_ven, r_pro, r_per = f_cli.result(), f_ven.result(), f_pro.result(), f_per.result()

    personas = {p["codigo"]: p["nombre"] for p in ((r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else [])}
    clientes = (r_cli["data"] or {}).get("datos", []) if r_cli["ok"] and r_cli["data"] else []
    for c in clientes:
        c["nombre_persona"] = personas.get(c["fkcodpersona"], c["fkcodpersona"])
    vendedores = (r_ven["data"] or {}).get("datos", []) if r_ven["ok"] and r_ven["data"] else []
    for v in vendedores:
        v["nombre_persona"] = personas.get(v["fkcodpersona"], v["fkcodpersona"])
    productos = (r_pro["data"] or {}).get("datos", []) if r_pro["ok"] and r_pro["data"] else []

    return render_template(
        "emitir_factura.html",
        sesion=sesion,
        clientes=clientes,
        vendedores=vendedores,
        productos=productos,
        error_conexion=r_cli["error_conexion"],
        mensaje_error=r_cli["mensaje"],
        sin_permiso=r_cli["sin_permiso"],
        no_autenticado=r_cli.get("no_autenticado", False),
        mensaje=None,
        exito=True,
    )


@app.post("/emitir-factura")
def procesar_emitir_factura():
    sesion = _sesion_actual()
    fkidcliente = int(request.form.get("fkidcliente"))
    fkidvendedor = int(request.form.get("fkidvendedor"))
    codigos = request.form.getlist("fkcodproducto")
    cantidades = request.form.getlist("cantidad")
    productos_payload = [{"fkcodproducto": cod, "cantidad": int(cant)} for cod, cant in zip(codigos, cantidades)]

    res = sr.crear_factura(sesion, {
        "fkidcliente": fkidcliente,
        "fkidvendedor": fkidvendedor,
        "productos": productos_payload,
    })
    if res["ok"]:
        num = res["data"]["factura"]["numero"]
        return redirect(f"/facturas/{num}", code=303)
    return redirect(f"/facturas?msg={quote(res['mensaje'])}&ok=0", code=303)


@app.get("/roles-por-usuario")
def ver_roles_por_usuario():
    sesion = _sesion_actual()
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")
    with ThreadPoolExecutor(max_workers=3) as pool:
        f_asig = pool.submit(sr.listar_rol_usuario, sesion)
        f_usr = pool.submit(sr.listar_usuarios, sesion)
        f_rol = pool.submit(sr.listar_roles, sesion)
        r_asig, r_usr, r_rol = f_asig.result(), f_usr.result(), f_rol.result()

    return render_template(
        "roles_por_usuario.html",
        sesion=sesion,
        asignaciones=(r_asig["data"] or {}).get("datos", []) if r_asig["ok"] and r_asig["data"] else [],
        usuarios=(r_usr["data"] or {}).get("datos", []) if r_usr["ok"] and r_usr["data"] else [],
        roles=(r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else [],
        error_conexion=r_asig["error_conexion"],
        mensaje_error=r_asig["mensaje"],
        sin_permiso=r_asig["sin_permiso"],
        no_autenticado=r_asig.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
    )


@app.post("/roles-por-usuario/crear")
def crear_rol_usuario_front():
    fkemail = request.form.get("fkemail", "")
    fkidrol = int(request.form.get("fkidrol"))
    res = sr.crear_rol_usuario(_sesion_actual(), fkemail, fkidrol)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Rol asignado correctamente." if res["ok"] else res["mensaje"]
    return redirect(f"/roles-por-usuario?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.post("/roles-por-usuario/eliminar")
def eliminar_rol_usuario_front():
    fkemail = request.form.get("fkemail", "")
    fkidrol = int(request.form.get("fkidrol"))
    res = sr.eliminar_rol_usuario(_sesion_actual(), fkemail, fkidrol)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Asignación removida." if res["ok"] else res["mensaje"]
    return redirect(f"/roles-por-usuario?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.get("/permisos-por-rol")
def ver_permisos_por_rol():
    sesion = _sesion_actual()
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")
    with ThreadPoolExecutor(max_workers=3) as pool:
        f_perm = pool.submit(sr.listar_rutarol, sesion)
        f_rut = pool.submit(sr.listar_rutas, sesion)
        f_rol = pool.submit(sr.listar_roles, sesion)
        r_perm, r_rut, r_rol = f_perm.result(), f_rut.result(), f_rol.result()

    return render_template(
        "permisos_por_rol.html",
        sesion=sesion,
        permisos=(r_perm["data"] or {}).get("datos", []) if r_perm["ok"] and r_perm["data"] else [],
        rutas=(r_rut["data"] or {}).get("datos", []) if r_rut["ok"] and r_rut["data"] else [],
        roles=(r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else [],
        error_conexion=r_perm["error_conexion"],
        mensaje_error=r_perm["mensaje"],
        sin_permiso=r_perm["sin_permiso"],
        no_autenticado=r_perm.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
    )


@app.post("/permisos-por-rol/crear")
def crear_permiso_front():
    fkidruta = int(request.form.get("fkidruta"))
    fkidrol = int(request.form.get("fkidrol"))
    res = sr.crear_rutarol(_sesion_actual(), fkidruta, fkidrol)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Permiso otorgado inmediatamente." if res["ok"] else res["mensaje"]
    return redirect(f"/permisos-por-rol?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.post("/permisos-por-rol/eliminar")
def eliminar_permiso_front():
    fkidruta = int(request.form.get("fkidruta"))
    fkidrol = int(request.form.get("fkidrol"))
    res = sr.eliminar_rutarol(_sesion_actual(), fkidruta, fkidrol)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Permiso revocado inmediatamente." if res["ok"] else res["mensaje"]
    return redirect(f"/permisos-por-rol?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.get("/usuarios-y-roles")
def ver_usuarios_y_roles():
    sesion = _sesion_actual()
    editar = request.args.get("editar")
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")
    with ThreadPoolExecutor(max_workers=2) as pool:
        f_usr = pool.submit(sr.listar_usuarios_con_roles, sesion)
        f_rol = pool.submit(sr.listar_roles, sesion)
        r_usr, r_rol = f_usr.result(), f_rol.result()

    usuarios = (r_usr["data"] or {}).get("datos", []) if r_usr["ok"] and r_usr["data"] else []
    roles = (r_rol["data"] or {}).get("datos", []) if r_rol["ok"] and r_rol["data"] else []
    roles_sel = []
    if editar:
        for u in usuarios:
            if u["email"] == editar:
                roles_sel = [r["idrol"] for r in u.get("roles", [])]
                break

    return render_template(
        "usuarios_y_roles.html",
        sesion=sesion,
        usuarios=usuarios,
        roles=roles,
        editando_email=editar,
        roles_seleccionados=roles_sel,
        error_conexion=r_usr["error_conexion"],
        mensaje_error=r_usr["mensaje"],
        sin_permiso=r_usr["sin_permiso"],
        no_autenticado=r_usr.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
        errores=[],
    )


@app.post("/usuarios-y-roles/guardar")
def guardar_usuario_con_roles_front():
    sesion = _sesion_actual()
    es_edicion = request.form.get("es_edicion") == "1"
    email = request.form.get("email", "").strip()
    contrasena = request.form.get("contrasena", "")
    roles_ids = [int(x) for x in request.form.getlist("roles")]

    if es_edicion:
        res = sr.actualizar_usuario_con_roles(sesion, email, {"contrasena": contrasena, "roles": roles_ids})
    else:
        res = sr.crear_usuario_con_roles(sesion, {"email": email, "contrasena": contrasena, "roles": roles_ids})
    ok_flag = "1" if res["ok"] else "0"
    msg = "Usuario y roles guardados con OPENJSON." if res["ok"] else res["mensaje"]
    return redirect(f"/usuarios-y-roles?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.post("/usuarios-y-roles/eliminar")
def eliminar_usuario_con_roles_front():
    email = request.form.get("email", "")
    res = sr.eliminar_usuario_con_roles(_sesion_actual(), email)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Usuario eliminado transaccionalmente." if res["ok"] else res["mensaje"]
    return redirect(f"/usuarios-y-roles?msg={quote(msg)}&ok={ok_flag}", code=303)


# ==============================================================
# 3. RUTAS EXPLÍCITAS POR CADA RECURSO CRUD (v1 y v2)
# Cada tabla tiene su propia ruta declarada explícitamente en Flask (/productos,
# /empresas, /personas, /roles, /rutas, /usuarios, /clientes, /vendedores),
# consumiendo sus funciones dedicadas en servicios_recursos.py.
# ==============================================================

def _cargar_selects_fk(recurso: str, sesion: dict | None) -> dict:
    """Carga desplegables FK en paralelo con ThreadPoolExecutor cuando aplica."""
    selects = {}
    if recurso == "clientes":
        with ThreadPoolExecutor(max_workers=2) as pool:
            f_per = pool.submit(sr.listar_personas, sesion)
            f_emp = pool.submit(sr.listar_empresas, sesion)
            r_per, r_emp = f_per.result(), f_emp.result()
        personas = (r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else []
        empresas = (r_emp["data"] or {}).get("datos", []) if r_emp["ok"] and r_emp["data"] else []
        selects["personas"] = [{"valor": p["codigo"], "etiqueta": f"{p['codigo']} — {p['nombre']}"} for p in personas]
        selects["empresas"] = [{"valor": e["codigo"], "etiqueta": f"{e['codigo']} — {e['nombre']}"} for e in empresas]
        selects["_map_personas"] = {p["codigo"]: p["nombre"] for p in personas}
        selects["_map_empresas"] = {e["codigo"]: e["nombre"] for e in empresas}
    elif recurso == "vendedores":
        r_per = sr.listar_personas(sesion)
        personas = (r_per["data"] or {}).get("datos", []) if r_per["ok"] and r_per["data"] else []
        selects["personas"] = [{"valor": p["codigo"], "etiqueta": f"{p['codigo']} — {p['nombre']}"} for p in personas]
        selects["_map_personas"] = {p["codigo"]: p["nombre"] for p in personas}
    return selects


def _enriquecer_filas(recurso: str, filas: list[dict], selects: dict) -> None:
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


def _render_vista_recurso(recurso: str):
    cfg = CRUD_RECURSOS[recurso]
    ops = cfg["ops"]
    sesion = _sesion_actual()
    editar = request.args.get("editar")
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")

    with ThreadPoolExecutor(max_workers=2) as pool:
        f_list = pool.submit(ops["listar"], sesion)
        f_sel = pool.submit(_cargar_selects_fk, recurso, sesion)
        res_list, selects = f_list.result(), f_sel.result()

    filas = (res_list["data"] or {}).get("datos", []) if res_list["ok"] and res_list["data"] else []
    _enriquecer_filas(recurso, filas, selects)

    valores = {}
    editando = False
    if editar:
        for f in filas:
            if str(f[cfg["pk"]]) == str(editar):
                valores = dict(f)
                editando = True
                break

    return render_template(
        "crud_recurso.html",
        sesion=sesion,
        config=cfg,
        filas=filas,
        selects=selects,
        editando=editando,
        clave_edicion=editar if editando else None,
        valores=valores,
        error_conexion=res_list["error_conexion"],
        mensaje_error=res_list["mensaje"],
        sin_permiso=res_list["sin_permiso"],
        no_autenticado=res_list.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
        errores=[],
    )


def _procesar_accion_recurso(recurso: str):
    cfg = CRUD_RECURSOS[recurso]
    ops = cfg["ops"]
    sesion = _sesion_actual()
    modo = request.form.get("modo", "crear")
    clave_edicion = request.form.get("clave_edicion", "")

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
        val = parsear_campo(c, request.form.get(nombre))
        if val is not None:
            body[nombre] = val
        elif c.get("opcional") and modo in ("crear", "put"):
            body[nombre] = None

    if recurso == "usuarios" and modo == "put" and "contrasena" not in body:
        body["contrasena"] = ""

    if modo == "crear":
        res = ops["crear"](sesion, body)
    elif modo == "put":
        res = ops["reemplazar"](sesion, str(clave_edicion), body)
    else:
        orig_res = ops["obtener"](sesion, str(clave_edicion))
        cambios = {}
        if orig_res["ok"] and isinstance(orig_res["data"], dict):
            orig = orig_res["data"]
            for k, v in body.items():
                if orig.get(k) != v:
                    cambios[k] = v
        else:
            cambios = body
        if recurso == "usuarios" and request.form.get("contrasena", "").strip():
            cambios["contrasena"] = request.form.get("contrasena", "").strip()
        res = ops["actualizar"](sesion, str(clave_edicion), cambios)

    if res["ok"]:
        return redirect(f"/{recurso}?msg={quote('Operación realizada con éxito.')}&ok=1", code=303)

    with ThreadPoolExecutor(max_workers=2) as pool:
        f_list = pool.submit(ops["listar"], sesion)
        f_sel = pool.submit(_cargar_selects_fk, recurso, sesion)
        res_list, selects = f_list.result(), f_sel.result()

    filas = (res_list["data"] or {}).get("datos", []) if res_list["ok"] and res_list["data"] else []
    _enriquecer_filas(recurso, filas, selects)

    return render_template(
        "crud_recurso.html",
        sesion=sesion,
        config=cfg,
        filas=filas,
        selects=selects,
        editando=(modo in ("put", "patch")),
        clave_edicion=clave_edicion if modo in ("put", "patch") else None,
        valores=dict(request.form),
        error_conexion=res["error_conexion"],
        mensaje_error=res["mensaje"],
        sin_permiso=res["sin_permiso"],
        no_autenticado=res.get("no_autenticado", False),
        mensaje=res["mensaje"],
        exito=False,
        errores=res["errores"],
    )


def _procesar_eliminar_recurso(recurso: str):
    ops = CRUD_RECURSOS[recurso]["ops"]
    clave = request.form.get("clave", "")
    res = ops["eliminar"](_sesion_actual(), str(clave))
    ok_flag = "1" if res["ok"] else "0"
    msg = "Registro eliminado correctamente." if res["ok"] else res["mensaje"]
    return redirect(f"/{recurso}?msg={quote(msg)}&ok={ok_flag}", code=303)


# Registro explícito en Flask de cada ruta CRUD (sin parámetro genérico /{recurso})
for _recurso in CRUD_RECURSOS:
    app.add_url_rule(
        f"/{_recurso}",
        endpoint=f"ver_{_recurso}",
        view_func=lambda r=_recurso: _render_vista_recurso(r),
        methods=["GET"],
    )
    app.add_url_rule(
        f"/{_recurso}/accion",
        endpoint=f"accion_{_recurso}",
        view_func=lambda r=_recurso: _procesar_accion_recurso(r),
        methods=["POST"],
    )
    app.add_url_rule(
        f"/{_recurso}/eliminar",
        endpoint=f"eliminar_{_recurso}",
        view_func=lambda r=_recurso: _procesar_eliminar_recurso(r),
        methods=["POST"],
    )


if __name__ == "__main__":
    # Ejecuta Flask en el puerto interno 8096 con recarga en caliente en desarrollo
    app.run(host="0.0.0.0", port=8096, debug=True)
