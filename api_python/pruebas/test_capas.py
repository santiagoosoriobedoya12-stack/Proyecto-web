# ==============================================================
# PRUEBAS UNITARIAS DE ARQUITECTURA EN 3 CAPAS Y POLIMORFISMO (Python)
# Implementa repositorios falsos en memoria que heredan de las interfaces ABC
# (IRepositorioProducto, IRepositorioCliente, IRepositorioFactura, IRepositorioUsuarioConRoles)
# demostrando el pilar de Polimorfismo (Cara B) sin conexión a SQL Server.
# ==============================================================

import os
import sys
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

from excepciones import ConflictoExcepcion, NoEncontradoExcepcion, ValidacionNegocioExcepcion
from peticiones import (
    ProductoCrear,
    ClienteCrear,
    FacturaCrear,
    ItemFacturaCrear,
    UsuarioConRolesCrear,
)
from repositorios.interfaces import (
    IRepositorioProducto,
    IRepositorioCliente,
    IRepositorioFactura,
    IRepositorioUsuarioConRoles,
)
from servicios import (
    ServicioProducto,
    ServicioCliente,
    ServicioFactura,
    ServicioUsuarioConRoles,
)


class RepositorioProductoEnMemoria(IRepositorioProducto):
    """Implementación en RAM de IRepositorioProducto (polimorfismo para pruebas sin BD)."""

    def __init__(self):
        self.filas: dict[str, dict] = {}

    def obtener_todos(self, limite: int) -> list[dict]:
        return list(self.filas.values())[:limite]

    def obtener_por_clave(self, codigo: str) -> dict | None:
        return self.filas.get(codigo)

    def crear(self, codigo: str, nombre: str, stock: int, valorunitario: float) -> None:
        if codigo in self.filas:
            raise ConflictoExcepcion(f"Ya existe {codigo}")
        self.filas[codigo] = {"codigo": codigo, "nombre": nombre, "stock": stock, "valorunitario": valorunitario}

    def actualizar_parcial(self, codigo: str, datos: dict) -> int:
        if codigo not in self.filas:
            return 0
        self.filas[codigo].update(datos)
        return 1

    def eliminar(self, codigo: str) -> int:
        return 1 if self.filas.pop(codigo, None) is not None else 0


class RepositorioClienteEnMemoria(IRepositorioCliente):
    """Implementación en RAM de IRepositorioCliente."""

    def __init__(self):
        self.ultimo_id = 0
        self.ultimo_registro: tuple | None = None

    def obtener_todos(self, limite: int) -> list[dict]:
        return []

    def obtener_por_clave(self, id_cliente: int) -> dict | None:
        return None

    def crear(self, credito: float, fkcodpersona: str, fkcodempresa: str | None) -> int:
        self.ultimo_id += 1
        self.ultimo_registro = (credito, fkcodpersona, fkcodempresa)
        return self.ultimo_id

    def actualizar_parcial(self, id_cliente: int, datos: dict) -> int:
        return 0

    def eliminar(self, id_cliente: int) -> int:
        return 0


class RepositorioFacturaEnMemoria(IRepositorioFactura):
    """Implementación en RAM de IRepositorioFactura."""

    def __init__(self):
        self.ultimo_json: str = ""

    def listar(self) -> list[dict]:
        return []

    def obtener_por_numero(self, numero: int) -> dict:
        raise NoEncontradoExcepcion("No existe")

    def crear(self, fkidcliente: int, fkidvendedor: int, productos_json: str) -> dict:
        self.ultimo_json = productos_json
        return {"factura": {"numero": 1, "total": 50000}, "productos": []}

    def anular(self, numero: int) -> dict:
        return {"numero": numero, "estado": "anulada"}


class RepositorioUsuarioConRolesEnMemoria(IRepositorioUsuarioConRoles):
    """Implementación en RAM de IRepositorioUsuarioConRoles."""

    def __init__(self):
        self.ultimo_hash: str = ""
        self.ultimo_roles_json: str = ""

    def listar(self) -> list[dict]:
        return []

    def consultar(self, email: str) -> dict:
        raise NoEncontradoExcepcion("No existe")

    def crear(self, email: str, contrasena_hash: str, roles_json: str) -> dict:
        self.ultimo_hash = contrasena_hash
        self.ultimo_roles_json = roles_json
        return {"email": email}

    def actualizar(self, email: str, contrasena_hash: str | None, roles_json: str) -> dict:
        return {"email": email}

    def eliminar(self, email: str) -> dict:
        return {"email": email}


class TestCapasServicios(unittest.TestCase):
    """Comprueba que los servicios operan igual sobre repositorios falsos en memoria."""

    def test_servicio_producto_limite_invalido(self):
        srv = ServicioProducto(RepositorioProductoEnMemoria())
        with self.assertRaises(ValidacionNegocioExcepcion):
            srv.listar(0)

    def test_servicio_producto_ciclo_en_memoria(self):
        repo = RepositorioProductoEnMemoria()
        srv = ServicioProducto(repo)
        creado = srv.crear(ProductoCrear(codigo="PR01", nombre="Monitor", stock=5, valorunitario=750000))
        self.assertEqual(creado["codigo"], "PR01")
        self.assertEqual(srv.obtener("PR01")["nombre"], "Monitor")
        with self.assertRaises(NoEncontradoExcepcion):
            srv.obtener("NOEXISTE")

    def test_servicio_cliente_normaliza_empresa_vacia_a_none(self):
        repo = RepositorioClienteEnMemoria()
        srv = ServicioCliente(repo)
        res = srv.crear(ClienteCrear(credito=1000, fkcodpersona="P001", fkcodempresa="   "))
        self.assertEqual(repo.ultimo_registro, (1000.0, "P001", None))
        self.assertEqual(res["id"], 1)
        self.assertIsNone(res["fkcodempresa"])

    def test_servicio_factura_serializa_productos_sin_totales(self):
        repo = RepositorioFacturaEnMemoria()
        srv = ServicioFactura(repo)
        pet = FacturaCrear(fkidcliente=1, fkidvendedor=1, productos=[ItemFacturaCrear(fkcodproducto="PR01", cantidad=2)])
        srv.crear(pet)
        self.assertEqual(repo.ultimo_json, '[{"fkcodproducto": "PR01", "cantidad": 2}]')

    def test_servicio_usuario_con_roles_hashea_bcrypt_y_arma_openjson(self):
        repo = RepositorioUsuarioConRolesEnMemoria()
        srv = ServicioUsuarioConRoles(repo)
        pet = UsuarioConRolesCrear(email="nuevo@correo.com", contrasena="ClaveSegura123", roles=[1, 3])
        srv.crear(pet)
        self.assertTrue(repo.ultimo_hash.startswith("$2"))
        self.assertNotEqual(repo.ultimo_hash, "ClaveSegura123")
        self.assertEqual(repo.ultimo_roles_json, '[{"fkidrol": 1}, {"fkidrol": 3}]')


if __name__ == "__main__":
    unittest.main(verbosity=2)
