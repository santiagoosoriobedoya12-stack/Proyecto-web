# ==============================================================
# SERVIDOR WEB FRONTEND EN PYTHON — FLASK + JINJA2 + HTTPX (v1..v3)
# - Módulo: Mapa de Conocimiento (21 tablas).
# - Jamás importa pymysql ni toca MariaDB: todo viaja vía HTTP a api-mapa.
# - Rutas explícitas para cada una de las 21 vistas y carga paralela
#   de catálogos foráneos con ThreadPoolExecutor.
# ==============================================================

from concurrent.futures import ThreadPoolExecutor
from urllib.parse import quote
from flask import Flask, make_response, redirect, render_template, request
from catalogo_vistas import CRUD_VISTAS, PUENTES_VISTAS
from servicios import (
    COOKIE_SESION,
    cerrar_sesion_servidor,
    crear_sesion_servidor,
    obtener_sesion,
    operaciones_api as op,
)

app = Flask(__name__, static_folder="static", template_folder="templates")


def _sesion_actual() -> dict | None:
    return obtener_sesion(request.cookies.get(COOKIE_SESION))


def _cargar_catalogo(clave: str, sesion: dict | None) -> tuple[list[dict], dict]:
    """Obtiene opciones legibles (valor + etiqueta) de un catálogo activo para desplegables <select>."""
    fuentes = {
        "lineas": (op.ops_lineas_investigacion["listar"], lambda r: (r["id"], f"{r['id']} — {r['nombre']}")),
        "proyectos": (op.ops_proyectos["listar"], lambda r: (r["id"], f"{r['id']} — {r['titulo']}")),
        "tipos_producto": (op.ops_tipos_producto["listar"], lambda r: (r["id"], f"{r['id']} — {r['nombre']} ({r['tipologia']})")),
        "aliados": (op.ops_aliados["listar"], lambda r: (r["nit"], f"{r['nit']} — {r['razon_social']}")),
        "terminos": (op.ops_terminos_clave["listar"], lambda r: (r["termino"], f"{r['termino']}")),
        "areas_conocimiento": (op.ops_areas_conocimiento["listar"], lambda r: (r["id"], f"{r['id']} — {r['disciplina']}")),
        "ods": (op.ops_ods["listar"], lambda r: (r["id"], f"ODS {r['id']} — {r['nombre']}")),
        "areas_aplicacion": (op.ops_areas_aplicacion["listar"], lambda r: (r["id"], f"{r['id']} — {r['nombre']}")),
        "docentes": (op.ops_docentes["listar"], lambda r: (r["cedula"], f"{r['cedula']} — {r['nombres']} {r['apellidos']}")),
        "productos": (op.ops_productos["listar"], lambda r: (r["id"], f"{r['id']} — {r['nombre']}")),
        "usuarios": (op.ops_usuarios["listar"], lambda r: (r["id"], f"{r['username']} ({r['email']})")),
        "roles": (op.ops_roles["listar"], lambda r: (r["id"], f"{r['id']} — {r['nombre']}")),
    }
    fn, fmt = fuentes[clave]
    res = fn(sesion)
    filas = (res["data"] or {}).get("datos", []) if res["ok"] and res["data"] else []
    opciones = [{"valor": fmt(f)[0], "etiqueta": fmt(f)[1]} for f in filas]
    mapa = {str(fmt(f)[0]): fmt(f)[1] for f in filas}
    return opciones, mapa


def _cargar_selects_paralelo(claves: list[str], sesion: dict | None) -> tuple[dict, dict]:
    if not claves:
        return {}, {}
    selects, mapas = {}, {}
    with ThreadPoolExecutor(max_workers=len(claves)) as pool:
        futuros = {c: pool.submit(_cargar_catalogo, c, sesion) for c in claves}
        for c, fut in futuros.items():
            opc, mp = fut.result()
            selects[c] = opc
            mapas[c] = mp
    return selects, mapas


def _enriquecer_fk(recurso: str, filas: list[dict], mapas: dict) -> None:
    if recurso == "docentes":
        mp = mapas.get("lineas", {})
        for f in filas:
            lid = f.get("linea_investigacion_principal")
            f["linea_display"] = mp.get(str(lid), str(lid)) if lid is not None else "Sin asignar"
    elif recurso == "productos":
        mp_proy = mapas.get("proyectos", {})
        mp_tipo = mapas.get("tipos_producto", {})
        for f in filas:
            pid = f.get("proyecto")
            tid = f.get("tipo_producto")
            f["proyecto_display"] = mp_proy.get(str(pid), str(pid)) if pid is not None else "Sin proyecto"
            f["tipo_display"] = mp_tipo.get(str(tid), str(tid))


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
    usuario = request.form.get("usuario", "")
    password = request.form.get("password", "")
    res = op.login_api(usuario, password)
    if not res["ok"]:
        return render_template("sesion.html", sesion=None, error=res["mensaje"])

    data = res["data"]
    token = data["token"]
    temp = {"token": token}
    perm_res = op.obtener_mis_permisos_api(temp)
    perm_data = perm_res["data"] if perm_res["ok"] and isinstance(perm_res["data"], dict) else {}

    sid = crear_sesion_servidor(
        token=token,
        username=data.get("username", usuario),
        email=data.get("email", ""),
        roles=data.get("roles", []),
        puede_escribir=bool(perm_data.get("puede_escribir", False)),
        es_admin=bool(perm_data.get("es_admin", False)),
    )
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
# 2. PARTICIPACIÓN DOCENTE <-> PROYECTO ('desarrolla', v2 con atributos)
# ==============================================================

@app.get("/desarrolla")
def ver_desarrolla():
    sesion = _sesion_actual()
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")
    edit_doc = request.args.get("docente")
    edit_proy = request.args.get("proyecto")

    with ThreadPoolExecutor(max_workers=3) as pool:
        f_list = pool.submit(op.listar_desarrolla, sesion)
        f_doc = pool.submit(_cargar_catalogo, "docentes", sesion)
        f_proy = pool.submit(_cargar_catalogo, "proyectos", sesion)
        r_list = f_list.result()
        opc_doc, map_doc = f_doc.result()
        opc_proy, map_proy = f_proy.result()

    filas = (r_list["data"] or {}).get("datos", []) if r_list["ok"] and r_list["data"] else []
    editando = None
    for f in filas:
        f["docente_display"] = map_doc.get(str(f["docente"]), str(f["docente"]))
        f["proyecto_display"] = map_proy.get(str(f["proyecto"]), str(f["proyecto"]))
        if edit_doc and edit_proy and str(f["docente"]) == edit_doc and str(f["proyecto"]) == edit_proy:
            editando = f

    return render_template(
        "desarrolla.html",
        sesion=sesion,
        filas=filas,
        docentes=opc_doc,
        proyectos=opc_proy,
        editando=editando,
        error_conexion=r_list["error_conexion"],
        mensaje_error=r_list["mensaje"],
        sin_permiso=r_list["sin_permiso"],
        no_autenticado=r_list.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
    )


@app.post("/desarrolla/guardar")
def guardar_desarrolla():
    sesion = _sesion_actual()
    es_edicion = request.form.get("es_edicion") == "1"
    docente = int(request.form.get("docente"))
    proyecto = int(request.form.get("proyecto"))
    rol = request.form.get("rol", "").strip()
    descripcion = request.form.get("descripcion", "").strip()

    if es_edicion:
        res = op.reemplazar_desarrolla(sesion, docente, proyecto, {"rol": rol, "descripcion": descripcion})
    else:
        res = op.crear_desarrolla(sesion, {"docente": docente, "proyecto": proyecto, "rol": rol, "descripcion": descripcion})
    ok_flag = "1" if res["ok"] else "0"
    msg = "Participación guardada." if res["ok"] else res["mensaje"]
    return redirect(f"/desarrolla?msg={quote(msg)}&ok={ok_flag}", code=303)


@app.post("/desarrolla/retirar")
def retirar_desarrolla_front():
    docente = int(request.form.get("docente"))
    proyecto = int(request.form.get("proyecto"))
    res = op.retirar_desarrolla(_sesion_actual(), docente, proyecto)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Participación retirada." if res["ok"] else res["mensaje"]
    return redirect(f"/desarrolla?msg={quote(msg)}&ok={ok_flag}", code=303)


# ==============================================================
# 3. VISTAS EXPLÍCITAS DE TABLAS PUENTE M:N (v2 y v3)
# ==============================================================

def _ver_puente(clave: str):
    cfg = PUENTES_VISTAS[clave]
    sesion = _sesion_actual()
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")

    with ThreadPoolExecutor(max_workers=3) as pool:
        f_list = pool.submit(cfg["ops"]["listar"], sesion)
        f_izq = pool.submit(_cargar_catalogo, cfg["cat_izq"], sesion)
        f_der = pool.submit(_cargar_catalogo, cfg["cat_der"], sesion)
        r_list = f_list.result()
        opc_izq, map_izq = f_izq.result()
        opc_der, map_der = f_der.result()

    filas = (r_list["data"] or {}).get("datos", []) if r_list["ok"] and r_list["data"] else []
    for f in filas:
        v_izq = f[cfg["col_izq"]]
        v_der = f[cfg["col_der"]]
        f["izq_display"] = map_izq.get(str(v_izq), str(v_izq))
        f["der_display"] = map_der.get(str(v_der), str(v_der))

    return render_template(
        "puente.html",
        sesion=sesion,
        cfg=cfg,
        filas=filas,
        opc_izq=opc_izq,
        opc_der=opc_der,
        error_conexion=r_list["error_conexion"],
        mensaje_error=r_list["mensaje"],
        sin_permiso=r_list["sin_permiso"],
        no_autenticado=r_list.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
    )


def _vincular_puente(clave: str):
    cfg = PUENTES_VISTAS[clave]
    val_izq = cfg["tipo_izq"](request.form.get("izq"))
    val_der = cfg["tipo_der"](request.form.get("der"))
    res = cfg["ops"]["vincular"](_sesion_actual(), val_izq, val_der)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Asociación registrada." if res["ok"] else res["mensaje"]
    return redirect(f"/{clave}?msg={quote(msg)}&ok={ok_flag}", code=303)


def _retirar_puente(clave: str):
    cfg = PUENTES_VISTAS[clave]
    val_izq = cfg["tipo_izq"](request.form.get("izq"))
    val_der = cfg["tipo_der"](request.form.get("der"))
    res = cfg["ops"]["retirar"](_sesion_actual(), val_izq, val_der)
    ok_flag = "1" if res["ok"] else "0"
    msg = "Asociación retirada." if res["ok"] else res["mensaje"]
    return redirect(f"/{clave}?msg={quote(msg)}&ok={ok_flag}", code=303)


for _p in PUENTES_VISTAS:
    app.add_url_rule(f"/{_p}", endpoint=f"ver_p_{_p}", view_func=lambda k=_p: _ver_puente(k), methods=["GET"])
    app.add_url_rule(f"/{_p}/vincular", endpoint=f"vinc_p_{_p}", view_func=lambda k=_p: _vincular_puente(k), methods=["POST"])
    app.add_url_rule(f"/{_p}/retirar", endpoint=f"ret_p_{_p}", view_func=lambda k=_p: _retirar_puente(k), methods=["POST"])


# ==============================================================
# 4. VISTAS EXPLÍCITAS DE CRUD TABULAR (v1, v2 y v3)
# ==============================================================

def _ver_crud(recurso: str):
    cfg = CRUD_VISTAS[recurso]
    sesion = _sesion_actual()
    editar = request.args.get("editar")
    msg = request.args.get("msg")
    ok = request.args.get("ok", "1")

    with ThreadPoolExecutor(max_workers=2) as pool:
        f_list = pool.submit(cfg["ops"]["listar"], sesion)
        f_sel = pool.submit(_cargar_selects_paralelo, cfg.get("selects_req", []), sesion)
        r_list = f_list.result()
        selects, mapas = f_sel.result()

    filas = (r_list["data"] or {}).get("datos", []) if r_list["ok"] and r_list["data"] else []
    _enriquecer_fk(recurso, filas, mapas)

    valores, editando = {}, False
    if editar:
        for f in filas:
            if str(f[cfg["pk"]]) == str(editar):
                valores = dict(f)
                editando = True
                break

    return render_template(
        "recurso_crud.html",
        sesion=sesion,
        config=cfg,
        filas=filas,
        selects=selects,
        editando=editando,
        clave_edicion=editar if editando else None,
        valores=valores,
        error_conexion=r_list["error_conexion"],
        mensaje_error=r_list["mensaje"],
        sin_permiso=r_list["sin_permiso"],
        no_autenticado=r_list.get("no_autenticado", False),
        mensaje=msg,
        exito=(ok == "1"),
        errores=[],
    )


def _accion_crud(recurso: str):
    cfg = CRUD_VISTAS[recurso]
    ops = cfg["ops"]
    sesion = _sesion_actual()
    modo = request.form.get("modo", "crear")
    clave_edicion = request.form.get("clave_edicion", "")

    def parsear(campo, raw):
        if raw is None or str(raw).strip() == "":
            return None
        s = str(raw).strip()
        if campo["tipo"] == "number" or campo.get("tipo_valor") == "int":
            return float(s) if campo.get("step") == "0.01" else int(float(s))
        return s

    body = {}
    for c in cfg["campos"]:
        nom = c["nombre"]
        if modo in ("put", "patch") and c.get("pk"):
            continue
        if modo == "crear" and c.get("pk") and cfg["pk_auto"]:
            continue
        val = parsear(c, request.form.get(nom))
        if val is not None:
            body[nom] = val
        elif c.get("opcional") and modo in ("crear", "put"):
            body[nom] = None

    if recurso == "usuarios" and modo == "put" and "password" not in body:
        body["password"] = ""

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
        if recurso == "usuarios" and request.form.get("password", "").strip():
            cambios["password"] = request.form.get("password", "").strip()
        res = ops["actualizar"](sesion, str(clave_edicion), cambios)

    if res["ok"]:
        return redirect(f"/{recurso}?msg={quote('Operación realizada correctamente.')}&ok=1", code=303)

    with ThreadPoolExecutor(max_workers=2) as pool:
        f_list = pool.submit(ops["listar"], sesion)
        f_sel = pool.submit(_cargar_selects_paralelo, cfg.get("selects_req", []), sesion)
        r_list = f_list.result()
        selects, mapas = f_sel.result()

    filas = (r_list["data"] or {}).get("datos", []) if r_list["ok"] and r_list["data"] else []
    _enriquecer_fk(recurso, filas, mapas)

    return render_template(
        "recurso_crud.html",
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


def _retirar_crud(recurso: str):
    cfg = CRUD_VISTAS[recurso]
    clave = request.form.get("clave", "")
    res = cfg["ops"]["retirar"](_sesion_actual(), str(clave))
    ok_flag = "1" if res["ok"] else "0"
    msg = "Registro retirado lógicamente." if res["ok"] else res["mensaje"]
    return redirect(f"/{recurso}?msg={quote(msg)}&ok={ok_flag}", code=303)


for _rec in CRUD_VISTAS:
    app.add_url_rule(f"/{_rec}", endpoint=f"ver_c_{_rec}", view_func=lambda r=_rec: _ver_crud(r), methods=["GET"])
    app.add_url_rule(f"/{_rec}/accion", endpoint=f"acc_c_{_rec}", view_func=lambda r=_rec: _accion_crud(r), methods=["POST"])
    app.add_url_rule(f"/{_rec}/retirar", endpoint=f"ret_c_{_rec}", view_func=lambda r=_rec: _retirar_crud(r), methods=["POST"])


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8116, debug=True)
