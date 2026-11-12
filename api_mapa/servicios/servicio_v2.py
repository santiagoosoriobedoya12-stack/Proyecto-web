# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 2: 10 TABLAS CON CLAVE FORÁNEA
# - Reciben interfaces ABC (IRepositorioDocente, IRepositorioProducto,
#   IRepositorioDesarrolla, IRepositorioPuente) por constructor.
# ==============================================================

from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from modelos import Docente, Producto, Desarrolla
from peticiones import (
    DocenteCrear, DocenteReemplazo, DocenteActualizar,
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    DesarrollaCrear, DesarrollaReemplazo, DesarrollaActualizar,
)
from repositorios.interfaces import (
    IRepositorioDocente,
    IRepositorioProducto,
    IRepositorioDesarrolla,
    IRepositorioPuente,
)
from servicios.servicio_v1 import validar_limite


class ServicioDocente:
    def __init__(self, repo: IRepositorioDocente):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, cedula: int) -> dict:
        fila = self.repo.obtener_por_clave(cedula)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe docente con cédula = {cedula}")
        return fila

    def crear(self, pet: DocenteCrear) -> dict:
        ent = Docente(
            cedula=pet.cedula,
            nombres=pet.nombres.strip(),
            apellidos=pet.apellidos.strip(),
            genero=pet.genero.strip(),
            cargo=pet.cargo.strip(),
            fecha_nacimiento=pet.fecha_nacimiento.isoformat(),
            correo=str(pet.correo).strip(),
            telefono=pet.telefono.strip(),
            url_cvlac=pet.url_cvlac.strip(),
            fecha_actualizacion=pet.fecha_actualizacion.isoformat(),
            escalafon=pet.escalafon.strip(),
            perfil=pet.perfil.strip(),
            cat_minciencia=pet.cat_minciencia.strip() if pet.cat_minciencia and pet.cat_minciencia.strip() else None,
            conv_minciencia=pet.conv_minciencia.strip(),
            nacionalidaad=pet.nacionalidaad.strip(),
            linea_investigacion_principal=pet.linea_investigacion_principal,
        )
        datos = ent.a_diccionario()
        self.repo.crear(datos)
        return datos

    def reemplazar(self, cedula: int, pet: DocenteReemplazo) -> dict:
        self.obtener(cedula)
        ent = Docente(
            cedula=cedula,
            nombres=pet.nombres.strip(),
            apellidos=pet.apellidos.strip(),
            genero=pet.genero.strip(),
            cargo=pet.cargo.strip(),
            fecha_nacimiento=pet.fecha_nacimiento.isoformat(),
            correo=str(pet.correo).strip(),
            telefono=pet.telefono.strip(),
            url_cvlac=pet.url_cvlac.strip(),
            fecha_actualizacion=pet.fecha_actualizacion.isoformat(),
            escalafon=pet.escalafon.strip(),
            perfil=pet.perfil.strip(),
            cat_minciencia=pet.cat_minciencia.strip() if pet.cat_minciencia and pet.cat_minciencia.strip() else None,
            conv_minciencia=pet.conv_minciencia.strip(),
            nacionalidaad=pet.nacionalidaad.strip(),
            linea_investigacion_principal=pet.linea_investigacion_principal,
        )
        datos = ent.a_diccionario()
        datos_upd = {k: v for k, v in datos.items() if k != "cedula"}
        self.repo.actualizar_parcial(cedula, datos_upd)
        return datos

    def actualizar_parcial(self, cedula: int, pet: DocenteActualizar) -> int:
        datos = {}
        for campo in (
            "nombres", "apellidos", "genero", "cargo", "telefono", "url_cvlac",
            "escalafon", "perfil", "cat_minciencia", "conv_minciencia", "nacionalidaad",
        ):
            val = getattr(pet, campo)
            if val is not None:
                datos[campo] = val.strip() if val.strip() else None
        if pet.correo is not None:
            datos["correo"] = str(pet.correo).strip()
        if pet.fecha_nacimiento is not None:
            datos["fecha_nacimiento"] = pet.fecha_nacimiento.isoformat()
        if pet.fecha_actualizacion is not None:
            datos["fecha_actualizacion"] = pet.fecha_actualizacion.isoformat()
        if pet.linea_investigacion_principal is not None:
            datos["linea_investigacion_principal"] = pet.linea_investigacion_principal
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        self.obtener(cedula)
        return self.repo.actualizar_parcial(cedula, datos)

    def eliminar(self, cedula: int) -> int:
        if self.repo.eliminar(cedula) == 0:
            raise NoEncontradoExcepcion(f"No existe docente activo con cédula = {cedula}")
        return 1


class ServicioProducto:
    def __init__(self, repo: IRepositorioProducto):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_prod: int) -> dict:
        fila = self.repo.obtener_por_clave(id_prod)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe producto con id = {id_prod}")
        return fila

    def crear(self, pet: ProductoCrear) -> dict:
        ent = Producto(
            id=pet.id,
            nombre=pet.nombre.strip(),
            categoria=pet.categoria.strip(),
            fecha_entrega=pet.fecha_entrega.isoformat(),
            proyecto=pet.proyecto,
            tipo_producto=pet.tipo_producto,
        )
        self.repo.crear(ent.id, ent.nombre, ent.categoria, ent.fecha_entrega, ent.proyecto, ent.tipo_producto)
        return ent.a_diccionario()

    def reemplazar(self, id_prod: int, pet: ProductoReemplazo) -> dict:
        self.obtener(id_prod)
        ent = Producto(
            id=id_prod,
            nombre=pet.nombre.strip(),
            categoria=pet.categoria.strip(),
            fecha_entrega=pet.fecha_entrega.isoformat(),
            proyecto=pet.proyecto,
            tipo_producto=pet.tipo_producto,
        )
        self.repo.actualizar_parcial(id_prod, {
            "nombre": ent.nombre,
            "categoria": ent.categoria,
            "fecha_entrega": ent.fecha_entrega,
            "proyecto": ent.proyecto,
            "tipo_producto": ent.tipo_producto,
        })
        return ent.a_diccionario()

    def actualizar_parcial(self, id_prod: int, pet: ProductoActualizar) -> int:
        datos = {}
        if pet.nombre is not None:
            datos["nombre"] = pet.nombre.strip()
        if pet.categoria is not None:
            datos["categoria"] = pet.categoria.strip()
        if pet.fecha_entrega is not None:
            datos["fecha_entrega"] = pet.fecha_entrega.isoformat()
        if pet.proyecto is not None:
            datos["proyecto"] = pet.proyecto
        if pet.tipo_producto is not None:
            datos["tipo_producto"] = pet.tipo_producto
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        self.obtener(id_prod)
        return self.repo.actualizar_parcial(id_prod, datos)

    def eliminar(self, id_prod: int) -> int:
        if self.repo.eliminar(id_prod) == 0:
            raise NoEncontradoExcepcion(f"No existe producto activo con id = {id_prod}")
        return 1


class ServicioDesarrolla:
    """Reglas para la relación docente <-> proyecto con rol y descripción."""

    def __init__(self, repo: IRepositorioDesarrolla):
        self.repo = repo

    def listar(self) -> list[dict]:
        return self.repo.obtener_todos()

    def obtener_por_docente(self, cedula: int) -> list[dict]:
        return self.repo.obtener_por_docente(cedula)

    def obtener_por_proyecto(self, id_proyecto: int) -> list[dict]:
        return self.repo.obtener_por_proyecto(id_proyecto)

    def crear(self, pet: DesarrollaCrear) -> dict:
        ent = Desarrolla(pet.docente, pet.proyecto, pet.rol.strip(), pet.descripcion.strip())
        self.repo.crear(ent.docente, ent.proyecto, ent.rol, ent.descripcion)
        return ent.a_diccionario()

    def reemplazar(self, cedula: int, id_proyecto: int, pet: DesarrollaReemplazo) -> dict:
        if not self.repo.obtener_par(cedula, id_proyecto):
            raise NoEncontradoExcepcion(f"No existe participación activa docente={cedula}, proyecto={id_proyecto}.")
        ent = Desarrolla(cedula, id_proyecto, pet.rol.strip(), pet.descripcion.strip())
        self.repo.actualizar_parcial(cedula, id_proyecto, {"rol": ent.rol, "descripcion": ent.descripcion})
        return ent.a_diccionario()

    def actualizar_parcial(self, cedula: int, id_proyecto: int, pet: DesarrollaActualizar) -> int:
        datos = {}
        if pet.rol is not None:
            datos["rol"] = pet.rol.strip()
        if pet.descripcion is not None:
            datos["descripcion"] = pet.descripcion.strip()
        if not datos:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        if not self.repo.obtener_par(cedula, id_proyecto):
            raise NoEncontradoExcepcion(f"No existe participación activa docente={cedula}, proyecto={id_proyecto}.")
        return self.repo.actualizar_parcial(cedula, id_proyecto, datos)

    def eliminar(self, cedula: int, id_proyecto: int) -> int:
        if self.repo.eliminar(cedula, id_proyecto) == 0:
            raise NoEncontradoExcepcion(f"No existe participación activa docente={cedula}, proyecto={id_proyecto}.")
        return 1


class ServicioPuente:
    """Servicio polimórfico reutilizable para las 7 tablas puente binarias M:N."""

    def __init__(self, repo: IRepositorioPuente, col_izq: str, col_der: str):
        self.repo = repo
        self.col_izq = col_izq
        self.col_der = col_der

    def listar(self) -> list[dict]:
        return self.repo.obtener_todos()

    def obtener_por_izquierda(self, clave_izq) -> list[dict]:
        return self.repo.obtener_por_izquierda(clave_izq)

    def obtener_por_derecha(self, clave_der) -> list[dict]:
        return self.repo.obtener_por_derecha(clave_der)

    def crear(self, clave_izq, clave_der) -> dict:
        self.repo.crear(clave_izq, clave_der)
        return {self.col_izq: clave_izq, self.col_der: clave_der}

    def eliminar(self, clave_izq, clave_der) -> int:
        if self.repo.eliminar(clave_izq, clave_der) == 0:
            raise NoEncontradoExcepcion(f"No existe vínculo activo ({clave_izq}, {clave_der}).")
        return 1
