# ==============================================================
# INTERFACES DE REPOSITORIO (CONTRATOS ABSTRACTOS - P.O.O.)
# Aplican los pilares de Abstracción, Polimorfismo e Inversión de Dependencias:
# - Los servicios dependen exclusivamente de estas interfaces (ABC), nunca de
#   pymssql ni de SQL Server concreto.
# - Permite intercambiar SQL Server por repositorios en memoria en las pruebas
#   unitarias (pruebas/test_capas.py) sin tocar la capa de negocio.
# ==============================================================

from abc import ABC, abstractmethod


class IRepositorioProducto(ABC):
    """Contrato abstracto de acceso a datos para 'producto'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, codigo: str) -> dict | None: ...

    @abstractmethod
    def crear(self, codigo: str, nombre: str, stock: int, valorunitario: float) -> None: ...

    @abstractmethod
    def actualizar_parcial(self, codigo: str, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, codigo: str) -> int: ...


class IRepositorioEmpresa(ABC):
    """Contrato abstracto de acceso a datos para 'empresa'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, codigo: str) -> dict | None: ...

    @abstractmethod
    def crear(self, codigo: str, nombre: str) -> None: ...

    @abstractmethod
    def actualizar_parcial(self, codigo: str, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, codigo: str) -> int: ...


class IRepositorioPersona(ABC):
    """Contrato abstracto de acceso a datos para 'persona'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, codigo: str) -> dict | None: ...

    @abstractmethod
    def crear(self, codigo: str, nombre: str, email: str, telefono: str) -> None: ...

    @abstractmethod
    def actualizar_parcial(self, codigo: str, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, codigo: str) -> int: ...


class IRepositorioRol(ABC):
    """Contrato abstracto de acceso a datos para 'rol'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, id_rol: int) -> dict | None: ...

    @abstractmethod
    def crear(self, nombre: str) -> int: ...

    @abstractmethod
    def actualizar_parcial(self, id_rol: int, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, id_rol: int) -> int: ...


class IRepositorioRuta(ABC):
    """Contrato abstracto de acceso a datos para 'ruta'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, id_ruta: int) -> dict | None: ...

    @abstractmethod
    def crear(self, ruta: str, descripcion: str) -> int: ...

    @abstractmethod
    def actualizar_parcial(self, id_ruta: int, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, id_ruta: int) -> int: ...


class IRepositorioUsuario(ABC):
    """Contrato abstracto de acceso a datos para 'usuario'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, email: str) -> dict | None: ...

    @abstractmethod
    def crear(self, email: str, contrasena_hash: str) -> None: ...

    @abstractmethod
    def actualizar_parcial(self, email: str, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, email: str) -> int: ...


class IRepositorioCliente(ABC):
    """Contrato abstracto de acceso a datos para 'cliente'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, id_cliente: int) -> dict | None: ...

    @abstractmethod
    def crear(self, credito: float, fkcodpersona: str, fkcodempresa: str | None) -> int: ...

    @abstractmethod
    def actualizar_parcial(self, id_cliente: int, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, id_cliente: int) -> int: ...


class IRepositorioVendedor(ABC):
    """Contrato abstracto de acceso a datos para 'vendedor'."""

    @abstractmethod
    def obtener_todos(self, limite: int) -> list[dict]: ...

    @abstractmethod
    def obtener_por_clave(self, id_vendedor: int) -> dict | None: ...

    @abstractmethod
    def crear(self, carnet: int, direccion: str, fkcodpersona: str) -> int: ...

    @abstractmethod
    def actualizar_parcial(self, id_vendedor: int, datos: dict) -> int: ...

    @abstractmethod
    def eliminar(self, id_vendedor: int) -> int: ...


class IRepositorioFactura(ABC):
    """Contrato abstracto Maestro-Detalle para 'factura'."""

    @abstractmethod
    def listar(self) -> list[dict]: ...

    @abstractmethod
    def obtener_por_numero(self, numero: int) -> dict: ...

    @abstractmethod
    def crear(self, fkidcliente: int, fkidvendedor: int, productos_json: str) -> dict: ...

    @abstractmethod
    def anular(self, numero: int) -> dict: ...


class IRepositorioRolUsuario(ABC):
    """Contrato abstracto de acceso a datos para tabla puente 'rol_usuario'."""

    @abstractmethod
    def obtener_todos(self) -> list[dict]: ...

    @abstractmethod
    def obtener_por_usuario(self, email: str) -> list[dict]: ...

    @abstractmethod
    def obtener_por_rol(self, idrol: int) -> list[dict]: ...

    @abstractmethod
    def crear(self, fkemail: str, fkidrol: int) -> None: ...

    @abstractmethod
    def eliminar(self, fkemail: str, fkidrol: int) -> int: ...


class IRepositorioRutaRol(ABC):
    """Contrato abstracto de acceso a datos para tabla puente 'rutarol'."""

    @abstractmethod
    def listar(self) -> list[dict]: ...

    @abstractmethod
    def crear(self, fkidruta: int, fkidrol: int) -> dict: ...

    @abstractmethod
    def eliminar(self, fkidruta: int, fkidrol: int) -> dict: ...


class IRepositorioUsuarioConRoles(ABC):
    """Contrato abstracto para el recurso compuesto 'usuario-con-roles'."""

    @abstractmethod
    def listar(self) -> list[dict]: ...

    @abstractmethod
    def consultar(self, email: str) -> dict: ...

    @abstractmethod
    def crear(self, email: str, contrasena_hash: str, roles_json: str) -> dict: ...

    @abstractmethod
    def actualizar(self, email: str, contrasena_hash: str | None, roles_json: str) -> dict: ...

    @abstractmethod
    def eliminar(self, email: str) -> dict: ...


class IRepositorioAcceso(ABC):
    """Contrato abstracto de autenticación y verificación dinámica RBAC."""

    @abstractmethod
    def obtener_credenciales_por_email(self, email: str) -> dict | None: ...

    @abstractmethod
    def obtener_roles_por_email(self, email: str) -> list[str]: ...

    @abstractmethod
    def obtener_id_ruta_por_nombre(self, nombre_ruta: str) -> int | None: ...

    @abstractmethod
    def verificar_acceso_ruta(self, email: str, id_ruta: int) -> bool: ...

    @abstractmethod
    def obtener_rutas_permitidas_por_email(self, email: str) -> list[str]: ...
