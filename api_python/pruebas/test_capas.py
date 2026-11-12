# ==============================================================
# PRUEBAS UNITARIAS DE LA ARQUITECTURA EN 3 CAPAS (Python)
# Comprueba la lógica pura de ServicioProducto, ServicioCliente, ServicioFactura,
# ServicioUsuario y ServicioUsuarioConRoles sin depender de conexión SQL Server real.
# Ejecución: python -m unittest pruebas/test_capas.py
# ==============================================================

import unittest
from unittest.mock import MagicMock
import sys
import os

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

from excepciones import NoEncontradoExcepcion, ValidacionNegocioExcepcion
from peticiones import ProductoCrear, ClienteCrear, FacturaCrear, ItemFacturaCrear, UsuarioCrear, UsuarioConRolesCrear
from servicios import ServicioProducto, ServicioCliente, ServicioFactura, ServicioUsuario, ServicioUsuarioConRoles


class TestCapasServicios(unittest.TestCase):
    """Suite de pruebas de dominio desacoplado en Python."""

    def test_servicio_producto_limite_invalido(self):
        repo = MagicMock()
        srv = ServicioProducto(repo)
        with self.assertRaises(ValidacionNegocioExcepcion):
            srv.listar(0)

    def test_servicio_producto_no_encontrado(self):
        repo = MagicMock()
        repo.obtener_por_clave.return_value = None
        srv = ServicioProducto(repo)
        with self.assertRaises(NoEncontradoExcepcion):
            srv.obtener("NOEXISTE")

    def test_servicio_cliente_normaliza_empresa_vacia_a_none(self):
        repo = MagicMock()
        repo.crear.return_value = 10
        srv = ServicioCliente(repo)
        res = srv.crear(ClienteCrear(credito=1000, fkcodpersona="P001", fkcodempresa="   "))
        repo.crear.assert_called_once_with(1000.0, "P001", None)
        self.assertEqual(res["id"], 10)
        self.assertIsNone(res["fkcodempresa"])

    def test_servicio_factura_serializa_productos_sin_totales(self):
        repo = MagicMock()
        repo.crear.return_value = {"factura": {"numero": 1, "total": 50000}, "productos": []}
        srv = ServicioFactura(repo)
        pet = FacturaCrear(fkidcliente=1, fkidvendedor=1, productos=[ItemFacturaCrear(fkcodproducto="PR01", cantidad=2)])
        srv.crear(pet)
        repo.crear.assert_called_once_with(1, 1, '[{"fkcodproducto": "PR01", "cantidad": 2}]')

    def test_servicio_usuario_con_roles_json_openjson(self):
        repo = MagicMock()
        repo.crear.return_value = {"email": "nuevo@correo.com", "roles": [{"idrol": 1}]}
        srv = ServicioUsuarioConRoles(repo)
        # Simula hashear para que no dependa de bcrypt si corre fuera del contenedor
        import servicios.servicio_v2 as sv2
        orig = sv2._hashear_contrasena
        sv2._hashear_contrasena = lambda s: "$2b$12$FAKEHASH"
        try:
            srv.crear(UsuarioConRolesCrear(email="nuevo@correo.com", contrasena="123", roles=[1, 3]))
            repo.crear.assert_called_once_with("nuevo@correo.com", "$2b$12$FAKEHASH", '[{"fkidrol": 1}, {"fkidrol": 3}]')
        finally:
            sv2._hashear_contrasena = orig


if __name__ == "__main__":
    unittest.main()
