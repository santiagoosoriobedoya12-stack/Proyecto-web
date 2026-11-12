# ==============================================================
# SERVICIOS DE DOMINIO VERSIÓN 1: 8 TABLAS SIN CLAVE FORÁNEA
# - Composición sobre herencia: reciben IRepositorio* por constructor.
# - No conocen HTTP ni MariaDB; lanzan excepciones de dominio tipadas.
# ==============================================================

import bcrypt
from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from modelos import (
    AreaConocimiento,
    ObjetivoDesarrolloSostenible,
    AreaAplicacion,
    TerminoClave,
    LineaInvestigacion,
    Aliado,
    TipoProducto,
    Proyecto,
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
from repositorios.interfaces import (
    IRepositorioAreaConocimiento,
    IRepositorioOds,
    IRepositorioAreaAplicacion,
    IRepositorioTerminoClave,
    IRepositorioLineaInvestigacion,
    IRepositorioAliado,
    IRepositorioTipoProducto,
    IRepositorioProyecto,
)


def validar_limite(limite: int) -> None:
    if limite <= 0:
        raise ValidacionNegocioExcepcion("El parámetro 'limite' debe ser mayor a 0.")


def validar_texto_clave(valor: str, etiqueta: str) -> str:
    if not valor or not valor.strip():
        raise ValidacionNegocioExcepcion(f"El {etiqueta} no puede estar vacío.")
    return valor.strip()


def hashear_password(password: str) -> str:
    """Cifra contraseñas con BCrypt factor de costo 12 y salt aleatoria."""
    return bcrypt.hashpw(password.encode("utf-8"), bcrypt.gensalt(rounds=12)).decode("utf-8")


def _extraer_patch(pet, campos_str: tuple[str, ...] = (), campos_otros: tuple[str, ...] = ()) -> dict:
    datos = {}
    for c in campos_str:
        val = getattr(pet, c)
        if val is not None:
            datos[c] = val.strip() if val.strip() else None
    for c in campos_otros:
        val = getattr(pet, c)
        if val is not None:
            datos[c] = val
    if not datos:
        raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar (PATCH vacío).")
    return datos


class ServicioAreaConocimiento:
    def __init__(self, repo: IRepositorioAreaConocimiento):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_area: int) -> dict:
        fila = self.repo.obtener_por_clave(id_area)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe area_conocimiento con id = {id_area}")
        return fila

    def crear(self, pet: AreaConocimientoCrear) -> dict:
        ent = AreaConocimiento(pet.id, pet.gran_area.strip(), pet.area.strip(), pet.disciplina.strip())
        self.repo.crear(ent.id, ent.gran_area, ent.area, ent.disciplina)
        return ent.a_diccionario()

    def reemplazar(self, id_area: int, pet: AreaConocimientoReemplazo) -> dict:
        self.obtener(id_area)
        ent = AreaConocimiento(id_area, pet.gran_area.strip(), pet.area.strip(), pet.disciplina.strip())
        self.repo.actualizar_parcial(id_area, {"gran_area": ent.gran_area, "area": ent.area, "disciplina": ent.disciplina})
        return ent.a_diccionario()

    def actualizar_parcial(self, id_area: int, pet: AreaConocimientoActualizar) -> int:
        datos = _extraer_patch(pet, ("gran_area", "area", "disciplina"))
        self.obtener(id_area)
        return self.repo.actualizar_parcial(id_area, datos)

    def eliminar(self, id_area: int) -> int:
        if self.repo.eliminar(id_area) == 0:
            raise NoEncontradoExcepcion(f"No existe area_conocimiento activa con id = {id_area}")
        return 1


class ServicioOds:
    def __init__(self, repo: IRepositorioOds):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_ods: int) -> dict:
        fila = self.repo.obtener_por_clave(id_ods)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe ODS con id = {id_ods}")
        return fila

    def crear(self, pet: OdsCrear) -> dict:
        ent = ObjetivoDesarrolloSostenible(pet.id, pet.nombre.strip(), pet.categoria.strip())
        self.repo.crear(ent.id, ent.nombre, ent.categoria)
        return ent.a_diccionario()

    def reemplazar(self, id_ods: int, pet: OdsReemplazo) -> dict:
        self.obtener(id_ods)
        ent = ObjetivoDesarrolloSostenible(id_ods, pet.nombre.strip(), pet.categoria.strip())
        self.repo.actualizar_parcial(id_ods, {"nombre": ent.nombre, "categoria": ent.categoria})
        return ent.a_diccionario()

    def actualizar_parcial(self, id_ods: int, pet: OdsActualizar) -> int:
        datos = _extraer_patch(pet, ("nombre", "categoria"))
        self.obtener(id_ods)
        return self.repo.actualizar_parcial(id_ods, datos)

    def eliminar(self, id_ods: int) -> int:
        if self.repo.eliminar(id_ods) == 0:
            raise NoEncontradoExcepcion(f"No existe ODS activo con id = {id_ods}")
        return 1


class ServicioAreaAplicacion:
    def __init__(self, repo: IRepositorioAreaAplicacion):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_aa: int) -> dict:
        fila = self.repo.obtener_por_clave(id_aa)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe area_aplicacion con id = {id_aa}")
        return fila

    def crear(self, pet: AreaAplicacionCrear) -> dict:
        ent = AreaAplicacion(pet.id, pet.nombre.strip())
        self.repo.crear(ent.id, ent.nombre)
        return ent.a_diccionario()

    def reemplazar(self, id_aa: int, pet: AreaAplicacionReemplazo) -> dict:
        self.obtener(id_aa)
        ent = AreaAplicacion(id_aa, pet.nombre.strip())
        self.repo.actualizar_parcial(id_aa, {"nombre": ent.nombre})
        return ent.a_diccionario()

    def actualizar_parcial(self, id_aa: int, pet: AreaAplicacionActualizar) -> int:
        datos = _extraer_patch(pet, ("nombre",))
        self.obtener(id_aa)
        return self.repo.actualizar_parcial(id_aa, datos)

    def eliminar(self, id_aa: int) -> int:
        if self.repo.eliminar(id_aa) == 0:
            raise NoEncontradoExcepcion(f"No existe area_aplicacion activa con id = {id_aa}")
        return 1


class ServicioTerminoClave:
    def __init__(self, repo: IRepositorioTerminoClave):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, termino: str) -> dict:
        t = validar_texto_clave(termino, "término clave")
        fila = self.repo.obtener_por_clave(t)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe termino_clave '{t}'")
        return fila

    def crear(self, pet: TerminoClaveCrear) -> dict:
        ing = pet.termino_ingles.strip() if pet.termino_ingles and pet.termino_ingles.strip() else None
        ent = TerminoClave(pet.termino.strip(), ing)
        self.repo.crear(ent.termino, ent.termino_ingles)
        return ent.a_diccionario()

    def reemplazar(self, termino: str, pet: TerminoClaveReemplazo) -> dict:
        t = validar_texto_clave(termino, "término clave")
        self.obtener(t)
        ing = pet.termino_ingles.strip() if pet.termino_ingles and pet.termino_ingles.strip() else None
        self.repo.actualizar_parcial(t, {"termino_ingles": ing})
        return TerminoClave(t, ing).a_diccionario()

    def actualizar_parcial(self, termino: str, pet: TerminoClaveActualizar) -> int:
        t = validar_texto_clave(termino, "término clave")
        if pet.termino_ingles is None:
            raise ValidacionNegocioExcepcion("No se envió ningún campo para actualizar.")
        self.obtener(t)
        ing = pet.termino_ingles.strip() if pet.termino_ingles.strip() else None
        return self.repo.actualizar_parcial(t, {"termino_ingles": ing})

    def eliminar(self, termino: str) -> int:
        t = validar_texto_clave(termino, "término clave")
        if self.repo.eliminar(t) == 0:
            raise NoEncontradoExcepcion(f"No existe termino_clave activo '{t}'")
        return 1


class ServicioLineaInvestigacion:
    def __init__(self, repo: IRepositorioLineaInvestigacion):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_linea: int) -> dict:
        fila = self.repo.obtener_por_clave(id_linea)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe linea_investigacion con id = {id_linea}")
        return fila

    def crear(self, pet: LineaInvestigacionCrear) -> dict:
        nuevo_id = self.repo.crear(pet.nombre.strip(), pet.descripcion.strip())
        return LineaInvestigacion(nuevo_id, pet.nombre.strip(), pet.descripcion.strip()).a_diccionario()

    def reemplazar(self, id_linea: int, pet: LineaInvestigacionReemplazo) -> dict:
        self.obtener(id_linea)
        ent = LineaInvestigacion(id_linea, pet.nombre.strip(), pet.descripcion.strip())
        self.repo.actualizar_parcial(id_linea, {"nombre": ent.nombre, "descripcion": ent.descripcion})
        return ent.a_diccionario()

    def actualizar_parcial(self, id_linea: int, pet: LineaInvestigacionActualizar) -> int:
        datos = _extraer_patch(pet, ("nombre", "descripcion"))
        self.obtener(id_linea)
        return self.repo.actualizar_parcial(id_linea, datos)

    def eliminar(self, id_linea: int) -> int:
        if self.repo.eliminar(id_linea) == 0:
            raise NoEncontradoExcepcion(f"No existe linea_investigacion activa con id = {id_linea}")
        return 1


class ServicioAliado:
    def __init__(self, repo: IRepositorioAliado):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, nit: int) -> dict:
        fila = self.repo.obtener_por_clave(nit)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe aliado con NIT = {nit}")
        return fila

    def crear(self, pet: AliadoCrear) -> dict:
        ent = Aliado(
            pet.nit, pet.razon_social.strip(), pet.nombre_contacto.strip(),
            str(pet.correo).strip(), pet.telefono.strip(), pet.ciudad.strip(),
        )
        self.repo.crear(ent.nit, ent.razon_social, ent.nombre_contacto, ent.correo, ent.telefono, ent.ciudad)
        return ent.a_diccionario()

    def reemplazar(self, nit: int, pet: AliadoReemplazo) -> dict:
        self.obtener(nit)
        ent = Aliado(
            nit, pet.razon_social.strip(), pet.nombre_contacto.strip(),
            str(pet.correo).strip(), pet.telefono.strip(), pet.ciudad.strip(),
        )
        self.repo.actualizar_parcial(nit, {
            "razon_social": ent.razon_social,
            "nombre_contacto": ent.nombre_contacto,
            "correo": ent.correo,
            "telefono": ent.telefono,
            "ciudad": ent.ciudad,
        })
        return ent.a_diccionario()

    def actualizar_parcial(self, nit: int, pet: AliadoActualizar) -> int:
        datos = _extraer_patch(pet, ("razon_social", "nombre_contacto", "telefono", "ciudad"), ("correo",))
        if "correo" in datos:
            datos["correo"] = str(datos["correo"]).strip()
        self.obtener(nit)
        return self.repo.actualizar_parcial(nit, datos)

    def eliminar(self, nit: int) -> int:
        if self.repo.eliminar(nit) == 0:
            raise NoEncontradoExcepcion(f"No existe aliado activo con NIT = {nit}")
        return 1


class ServicioTipoProducto:
    def __init__(self, repo: IRepositorioTipoProducto):
        self.repo = repo

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_tipo: int) -> dict:
        fila = self.repo.obtener_por_clave(id_tipo)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe tipo_producto con id = {id_tipo}")
        return fila

    def crear(self, pet: TipoProductoCrear) -> dict:
        ent = TipoProducto(pet.id, pet.categoria.strip(), pet.clase.strip(), pet.nombre.strip(), pet.tipologia.strip())
        self.repo.crear(ent.id, ent.categoria, ent.clase, ent.nombre, ent.tipologia)
        return ent.a_diccionario()

    def reemplazar(self, id_tipo: int, pet: TipoProductoReemplazo) -> dict:
        self.obtener(id_tipo)
        ent = TipoProducto(id_tipo, pet.categoria.strip(), pet.clase.strip(), pet.nombre.strip(), pet.tipologia.strip())
        self.repo.actualizar_parcial(id_tipo, {
            "categoria": ent.categoria, "clase": ent.clase, "nombre": ent.nombre, "tipologia": ent.tipologia,
        })
        return ent.a_diccionario()

    def actualizar_parcial(self, id_tipo: int, pet: TipoProductoActualizar) -> int:
        datos = _extraer_patch(pet, ("categoria", "clase", "nombre", "tipologia"))
        self.obtener(id_tipo)
        return self.repo.actualizar_parcial(id_tipo, datos)

    def eliminar(self, id_tipo: int) -> int:
        if self.repo.eliminar(id_tipo) == 0:
            raise NoEncontradoExcepcion(f"No existe tipo_producto activo con id = {id_tipo}")
        return 1


class ServicioProyecto:
    """Reglas de negocio para 'proyecto' (valida coherencia de fechas inicio <= fin)."""

    def __init__(self, repo: IRepositorioProyecto):
        self.repo = repo

    @staticmethod
    def _validar_fechas(fecha_inicio, fecha_fin) -> None:
        if fecha_fin is not None and fecha_fin < fecha_inicio:
            raise ValidacionNegocioExcepcion("La fecha_fin no puede ser anterior a fecha_inicio.")

    def listar(self, limite: int) -> list[dict]:
        validar_limite(limite)
        return self.repo.obtener_todos(limite)

    def obtener(self, id_proyecto: int) -> dict:
        fila = self.repo.obtener_por_clave(id_proyecto)
        if not fila:
            raise NoEncontradoExcepcion(f"No existe proyecto con id = {id_proyecto}")
        return fila

    def crear(self, pet: ProyectoCrear) -> dict:
        self._validar_fechas(pet.fecha_inicio, pet.fecha_fin)
        ent = Proyecto(
            pet.id,
            pet.titulo.strip(),
            pet.resumen.strip(),
            pet.presupuesto,
            pet.tipo_financiacion.strip(),
            pet.tipo_fondos.strip(),
            pet.fecha_inicio.isoformat(),
            pet.fecha_fin.isoformat() if pet.fecha_fin else None,
        )
        self.repo.crear(
            ent.id, ent.titulo, ent.resumen, ent.presupuesto,
            ent.tipo_financiacion, ent.tipo_fondos, ent.fecha_inicio, ent.fecha_fin,
        )
        return ent.a_diccionario()

    def reemplazar(self, id_proyecto: int, pet: ProyectoReemplazo) -> dict:
        self.obtener(id_proyecto)
        self._validar_fechas(pet.fecha_inicio, pet.fecha_fin)
        ent = Proyecto(
            id_proyecto,
            pet.titulo.strip(),
            pet.resumen.strip(),
            pet.presupuesto,
            pet.tipo_financiacion.strip(),
            pet.tipo_fondos.strip(),
            pet.fecha_inicio.isoformat(),
            pet.fecha_fin.isoformat() if pet.fecha_fin else None,
        )
        self.repo.actualizar_parcial(id_proyecto, {
            "titulo": ent.titulo,
            "resumen": ent.resumen,
            "presupuesto": ent.presupuesto,
            "tipo_financiacion": ent.tipo_financiacion,
            "tipo_fondos": ent.tipo_fondos,
            "fecha_inicio": ent.fecha_inicio,
            "fecha_fin": ent.fecha_fin,
        })
        return ent.a_diccionario()

    def actualizar_parcial(self, id_proyecto: int, pet: ProyectoActualizar) -> int:
        datos = _extraer_patch(pet, ("titulo", "resumen", "tipo_financiacion", "tipo_fondos"), ("presupuesto", "fecha_inicio", "fecha_fin"))
        actual = self.obtener(id_proyecto)
        f_ini = datos.get("fecha_inicio") or actual["fecha_inicio"]
        f_fin = datos.get("fecha_fin") if "fecha_fin" in datos else actual.get("fecha_fin")
        if hasattr(f_ini, "isoformat"):
            datos["fecha_inicio"] = f_ini.isoformat()
            f_ini = datos["fecha_inicio"]
        if hasattr(f_fin, "isoformat"):
            datos["fecha_fin"] = f_fin.isoformat()
            f_fin = datos["fecha_fin"]
        if f_fin and str(f_fin) < str(f_ini):
            raise ValidacionNegocioExcepcion("La fecha_fin no puede ser anterior a fecha_inicio.")
        return self.repo.actualizar_parcial(id_proyecto, datos)

    def eliminar(self, id_proyecto: int) -> int:
        if self.repo.eliminar(id_proyecto) == 0:
            raise NoEncontradoExcepcion(f"No existe proyecto activo con id = {id_proyecto}")
        return 1
