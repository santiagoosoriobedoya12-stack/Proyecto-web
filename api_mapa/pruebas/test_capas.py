# ==============================================================
# PRUEBAS UNITARIAS DE ARQUITECTURA EN CAPAS Y POLIMORFISMO (Python)
# Implementa repositorios en memoria heredando de las interfaces ABC sin depender
# de MariaDB en ejecución (Criterio 6 de v1 y regla de POO del curso).
# ==============================================================

from datetime import date
import os
import sys
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

from excepciones import ConflictoExcepcion, NoEncontradoExcepcion, ValidacionNegocioExcepcion
from peticiones import ProyectoCrear, ProductoCrear, UsuarioCrear
from repositorios.interfaces import IRepositorioProyecto, IRepositorioProducto, IRepositorioUsuario
from servicios import ServicioProyecto, ServicioProducto, ServicioUsuario


class RepositorioProyectoEnMemoria(IRepositorioProyecto):
    """Implementación en memoria de IRepositorioProyecto con borrado lógico."""

    def __init__(self):
        self.filas: dict[int, dict] = {}

    def obtener_todos(self, limite: int) -> list[dict]:
        return [{k: v for k, v in f.items() if k != "activo"} for f in self.filas.values() if f["activo"] == 1][:limite]

    def obtener_por_clave(self, id_proyecto: int) -> dict | None:
        f = self.filas.get(id_proyecto)
        if not f or f["activo"] == 0:
            return None
        return {k: v for k, v in f.items() if k != "activo"}

    def crear(self, id_proyecto, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin) -> None:
        if id_proyecto in self.filas:
            raise ConflictoExcepcion("Duplicado")
        self.filas[id_proyecto] = {
            "id": id_proyecto, "titulo": titulo, "resumen": resumen, "presupuesto": presupuesto,
            "tipo_financiacion": tipo_financiacion, "tipo_fondos": tipo_fondos,
            "fecha_inicio": fecha_inicio, "fecha_fin": fecha_fin, "activo": 1,
        }

    def actualizar_parcial(self, id_proyecto: int, datos: dict) -> int:
        f = self.filas.get(id_proyecto)
        if not f or f["activo"] == 0:
            return 0
        f.update(datos)
        return 1

    def eliminar(self, id_proyecto: int) -> int:
        f = self.filas.get(id_proyecto)
        if not f or f["activo"] == 0:
            return 0
        f["activo"] = 0
        return 1


class RepositorioProductoEnMemoria(IRepositorioProducto):
    def __init__(self, tipos_activos: set[int]):
        self.tipos_activos = tipos_activos
        self.filas: dict[int, dict] = {}

    def obtener_todos(self, limite: int) -> list[dict]:
        return [f for f in self.filas.values() if f["activo"] == 1][:limite]

    def obtener_por_clave(self, id_prod: int) -> dict | None:
        f = self.filas.get(id_prod)
        return f if f and f["activo"] == 1 else None

    def crear(self, id_prod, nombre, categoria, fecha_entrega, proyecto, tipo_producto) -> None:
        if tipo_producto not in self.tipos_activos:
            raise ConflictoExcepcion("tipo_producto no existe")
        self.filas[id_prod] = {
            "id": id_prod, "nombre": nombre, "categoria": categoria,
            "fecha_entrega": fecha_entrega, "proyecto": proyecto, "tipo_producto": tipo_producto, "activo": 1,
        }

    def actualizar_parcial(self, id_prod: int, datos: dict) -> int:
        return 0

    def eliminar(self, id_prod: int) -> int:
        return 0


class RepositorioUsuarioEnMemoria(IRepositorioUsuario):
    def __init__(self):
        self.filas: dict[int, dict] = {}

    def obtener_todos(self, limite: int) -> list[dict]:
        return []

    def obtener_por_clave(self, id_usuario: int) -> dict | None:
        return None

    def crear(self, username: str, password_hash: str, email: str, nombre_completo: str | None) -> int:
        nid = len(self.filas) + 1
        self.filas[nid] = {"id": nid, "username": username, "password": password_hash, "email": email, "nombre_completo": nombre_completo}
        return nid

    def actualizar_parcial(self, id_usuario: int, datos: dict) -> int:
        return 0

    def eliminar(self, id_usuario: int) -> int:
        return 0


class TestCapasMapaConocimiento(unittest.TestCase):
    def test_proyecto_borrado_logico_y_reintento_404(self):
        srv = ServicioProyecto(RepositorioProyectoEnMemoria())
        srv.crear(ProyectoCrear(
            id=1, titulo="Proyecto Prueba", resumen="Resumen valido",
            presupuesto=1000, tipo_financiacion="Interno", tipo_fondos="Propios",
            fecha_inicio=date(2026, 1, 1), fecha_fin=date(2026, 12, 1),
        ))
        self.assertEqual(len(srv.listar(10)), 1)
        srv.eliminar(1)
        # Tras retirar lógicamente, listar excluye y volver a retirar falla 404
        self.assertEqual(len(srv.listar(10)), 0)
        with self.assertRaises(NoEncontradoExcepcion):
            srv.eliminar(1)

    def test_proyecto_fecha_fin_anterior_a_inicio(self):
        srv = ServicioProyecto(RepositorioProyectoEnMemoria())
        with self.assertRaises(ValidacionNegocioExcepcion):
            srv.crear(ProyectoCrear(
                id=2, titulo="Fechas Malas", resumen="Error", presupuesto=500,
                tipo_financiacion="Interno", tipo_fondos="Propios",
                fecha_inicio=date(2026, 10, 1), fecha_fin=date(2026, 1, 1),
            ))

    def test_producto_exige_tipo_producto_activo(self):
        srv = ServicioProducto(RepositorioProductoEnMemoria(tipos_activos={1}))
        with self.assertRaises(ConflictoExcepcion):
            srv.crear(ProductoCrear(
                id=10, nombre="App", categoria="Software",
                fecha_entrega=date(2026, 5, 1), proyecto=None, tipo_producto=999,
            ))

    def test_usuario_hashea_password_y_no_lo_retorna(self):
        repo = RepositorioUsuarioEnMemoria()
        srv = ServicioUsuario(repo)
        creado = srv.crear(UsuarioCrear(username="ana", password="ClaveSecreta123", email="ana@uni.edu.co"))
        self.assertNotIn("password", creado)
        self.assertTrue(repo.filas[1]["password"].startswith("$2"))


if __name__ == "__main__":
    unittest.main(verbosity=2)
