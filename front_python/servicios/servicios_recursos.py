# ==============================================================
# FUNCIONES EXPLÍCITAS POR RECURSO Y OPERACIÓN (front_python)
# Cada recurso expone sus operaciones concretas (listar_*, obtener_*, crear_*, etc.)
# evitando ocultar todas las rutas detrás de llamadas anónimas en las vistas Flask.
# ==============================================================

from urllib.parse import quote
from servicios.cliente_api import peticion_api


# --- v3: SESIÓN Y PERMISOS ---
def iniciar_sesion_api(email: str, contrasena: str) -> dict:
    return peticion_api("POST", "api/sesion", json_body={"email": email.strip(), "contrasena": contrasena})


def obtener_permisos_mios(sesion: dict) -> dict:
    return peticion_api("GET", "api/permisos/mios", sesion=sesion)


# --- v1: PRODUCTO ---
def listar_productos(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/producto?limite={limite}", sesion=sesion)

def obtener_producto(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("GET", f"api/producto/{quote(codigo)}", sesion=sesion)

def crear_producto(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/producto", sesion=sesion, json_body=datos)

def reemplazar_producto(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/producto/{quote(codigo)}", sesion=sesion, json_body=datos)

def actualizar_producto(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/producto/{quote(codigo)}", sesion=sesion, json_body=datos)

def eliminar_producto(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("DELETE", f"api/producto/{quote(codigo)}", sesion=sesion)


# --- v1: EMPRESA ---
def listar_empresas(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/empresa?limite={limite}", sesion=sesion)

def obtener_empresa(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("GET", f"api/empresa/{quote(codigo)}", sesion=sesion)

def crear_empresa(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/empresa", sesion=sesion, json_body=datos)

def reemplazar_empresa(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/empresa/{quote(codigo)}", sesion=sesion, json_body=datos)

def actualizar_empresa(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/empresa/{quote(codigo)}", sesion=sesion, json_body=datos)

def eliminar_empresa(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("DELETE", f"api/empresa/{quote(codigo)}", sesion=sesion)


# --- v1: PERSONA ---
def listar_personas(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/persona?limite={limite}", sesion=sesion)

def obtener_persona(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("GET", f"api/persona/{quote(codigo)}", sesion=sesion)

def crear_persona(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/persona", sesion=sesion, json_body=datos)

def reemplazar_persona(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/persona/{quote(codigo)}", sesion=sesion, json_body=datos)

def actualizar_persona(sesion: dict | None, codigo: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/persona/{quote(codigo)}", sesion=sesion, json_body=datos)

def eliminar_persona(sesion: dict | None, codigo: str) -> dict:
    return peticion_api("DELETE", f"api/persona/{quote(codigo)}", sesion=sesion)


# --- v1: ROL ---
def listar_roles(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/rol?limite={limite}", sesion=sesion)

def obtener_rol(sesion: dict | None, id_rol: str) -> dict:
    return peticion_api("GET", f"api/rol/{quote(str(id_rol))}", sesion=sesion)

def crear_rol(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/rol", sesion=sesion, json_body=datos)

def reemplazar_rol(sesion: dict | None, id_rol: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/rol/{quote(str(id_rol))}", sesion=sesion, json_body=datos)

def actualizar_rol(sesion: dict | None, id_rol: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/rol/{quote(str(id_rol))}", sesion=sesion, json_body=datos)

def eliminar_rol(sesion: dict | None, id_rol: str) -> dict:
    return peticion_api("DELETE", f"api/rol/{quote(str(id_rol))}", sesion=sesion)


# --- v1: RUTA ---
def listar_rutas(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/ruta?limite={limite}", sesion=sesion)

def obtener_ruta(sesion: dict | None, id_ruta: str) -> dict:
    return peticion_api("GET", f"api/ruta/{quote(str(id_ruta))}", sesion=sesion)

def crear_ruta(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/ruta", sesion=sesion, json_body=datos)

def reemplazar_ruta(sesion: dict | None, id_ruta: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/ruta/{quote(str(id_ruta))}", sesion=sesion, json_body=datos)

def actualizar_ruta(sesion: dict | None, id_ruta: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/ruta/{quote(str(id_ruta))}", sesion=sesion, json_body=datos)

def eliminar_ruta(sesion: dict | None, id_ruta: str) -> dict:
    return peticion_api("DELETE", f"api/ruta/{quote(str(id_ruta))}", sesion=sesion)


# --- v1: USUARIO ---
def listar_usuarios(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/usuario?limite={limite}", sesion=sesion)

def obtener_usuario(sesion: dict | None, email: str) -> dict:
    return peticion_api("GET", f"api/usuario/{quote(email)}", sesion=sesion)

def crear_usuario(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/usuario", sesion=sesion, json_body=datos)

def reemplazar_usuario(sesion: dict | None, email: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/usuario/{quote(email)}", sesion=sesion, json_body=datos)

def actualizar_usuario(sesion: dict | None, email: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/usuario/{quote(email)}", sesion=sesion, json_body=datos)

def eliminar_usuario(sesion: dict | None, email: str) -> dict:
    return peticion_api("DELETE", f"api/usuario/{quote(email)}", sesion=sesion)


# --- v2: CLIENTE ---
def listar_clientes(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/cliente?limite={limite}", sesion=sesion)

def obtener_cliente(sesion: dict | None, id_cliente: str) -> dict:
    return peticion_api("GET", f"api/cliente/{quote(str(id_cliente))}", sesion=sesion)

def crear_cliente(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/cliente", sesion=sesion, json_body=datos)

def reemplazar_cliente(sesion: dict | None, id_cliente: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/cliente/{quote(str(id_cliente))}", sesion=sesion, json_body=datos)

def actualizar_cliente(sesion: dict | None, id_cliente: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/cliente/{quote(str(id_cliente))}", sesion=sesion, json_body=datos)

def eliminar_cliente(sesion: dict | None, id_cliente: str) -> dict:
    return peticion_api("DELETE", f"api/cliente/{quote(str(id_cliente))}", sesion=sesion)


# --- v2: VENDEDOR ---
def listar_vendedores(sesion: dict | None, limite: int = 1000) -> dict:
    return peticion_api("GET", f"api/vendedor?limite={limite}", sesion=sesion)

def obtener_vendedor(sesion: dict | None, id_vendedor: str) -> dict:
    return peticion_api("GET", f"api/vendedor/{quote(str(id_vendedor))}", sesion=sesion)

def crear_vendedor(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/vendedor", sesion=sesion, json_body=datos)

def reemplazar_vendedor(sesion: dict | None, id_vendedor: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/vendedor/{quote(str(id_vendedor))}", sesion=sesion, json_body=datos)

def actualizar_vendedor(sesion: dict | None, id_vendedor: str, datos: dict) -> dict:
    return peticion_api("PATCH", f"api/vendedor/{quote(str(id_vendedor))}", sesion=sesion, json_body=datos)

def eliminar_vendedor(sesion: dict | None, id_vendedor: str) -> dict:
    return peticion_api("DELETE", f"api/vendedor/{quote(str(id_vendedor))}", sesion=sesion)


# --- v2: FACTURA ---
def listar_facturas(sesion: dict | None) -> dict:
    return peticion_api("GET", "api/factura", sesion=sesion)

def obtener_factura(sesion: dict | None, numero: int) -> dict:
    return peticion_api("GET", f"api/factura/{numero}", sesion=sesion)

def crear_factura(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/factura", sesion=sesion, json_body=datos)

def anular_factura_api(sesion: dict | None, numero: int) -> dict:
    return peticion_api("DELETE", f"api/factura/{numero}", sesion=sesion)


# --- v2: PUENTES Y USUARIO CON ROLES ---
def listar_rol_usuario(sesion: dict | None) -> dict:
    return peticion_api("GET", "api/rol-usuario", sesion=sesion)

def crear_rol_usuario(sesion: dict | None, fkemail: str, fkidrol: int) -> dict:
    return peticion_api("POST", "api/rol-usuario", sesion=sesion, json_body={"fkemail": fkemail, "fkidrol": fkidrol})

def eliminar_rol_usuario(sesion: dict | None, fkemail: str, fkidrol: int) -> dict:
    return peticion_api("DELETE", f"api/rol-usuario/{quote(fkemail)}/{fkidrol}", sesion=sesion)

def listar_rutarol(sesion: dict | None) -> dict:
    return peticion_api("GET", "api/rutarol", sesion=sesion)

def crear_rutarol(sesion: dict | None, fkidruta: int, fkidrol: int) -> dict:
    return peticion_api("POST", "api/rutarol", sesion=sesion, json_body={"fkidruta": fkidruta, "fkidrol": fkidrol})

def eliminar_rutarol(sesion: dict | None, fkidruta: int, fkidrol: int) -> dict:
    return peticion_api("DELETE", f"api/rutarol/{fkidruta}/{fkidrol}", sesion=sesion)

def listar_usuarios_con_roles(sesion: dict | None) -> dict:
    return peticion_api("GET", "api/usuario-con-roles", sesion=sesion)

def crear_usuario_con_roles(sesion: dict | None, datos: dict) -> dict:
    return peticion_api("POST", "api/usuario-con-roles", sesion=sesion, json_body=datos)

def actualizar_usuario_con_roles(sesion: dict | None, email: str, datos: dict) -> dict:
    return peticion_api("PUT", f"api/usuario-con-roles/{quote(email)}", sesion=sesion, json_body=datos)

def eliminar_usuario_con_roles(sesion: dict | None, email: str) -> dict:
    return peticion_api("DELETE", f"api/usuario-con-roles/{quote(email)}", sesion=sesion)
