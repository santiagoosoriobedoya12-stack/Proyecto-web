# ==============================================================
# ENSAMBLADOR DE DEPENDENCIAS (INYECCIÓN DE DEPENDENCIAS MANUAL)
# Es el ÚNICO módulo de la API que conoce e instancia las clases concretas
# Repositorio*SqlServer. Los controladores y servicios trabajan únicamente contra
# interfaces abstractas (IRepositorio*).
# ==============================================================

from repositorios import (
    RepositorioProductoSqlServer,
    RepositorioEmpresaSqlServer,
    RepositorioPersonaSqlServer,
    RepositorioRolSqlServer,
    RepositorioRutaSqlServer,
    RepositorioUsuarioSqlServer,
    RepositorioClienteSqlServer,
    RepositorioVendedorSqlServer,
    RepositorioFacturaSqlServer,
    RepositorioRolUsuarioSqlServer,
    RepositorioRutaRolSqlServer,
    RepositorioUsuarioConRolesSqlServer,
    RepositorioAccesoSqlServer,
)
from repositorios.interfaces import IRepositorioAcceso
from servicios import (
    ServicioProducto,
    ServicioEmpresa,
    ServicioPersona,
    ServicioRol,
    ServicioRuta,
    ServicioUsuario,
    ServicioCliente,
    ServicioVendedor,
    ServicioFactura,
    ServicioRolUsuario,
    ServicioRutaRol,
    ServicioUsuarioConRoles,
    ServicioSesion,
)

# Instancias concretas únicas conectadas a SQL Server
_repo_acceso = RepositorioAccesoSqlServer()
_repo_usuario = RepositorioUsuarioSqlServer()

# Servicios ensamblados mediante composición e inyección por constructor
servicio_producto = ServicioProducto(RepositorioProductoSqlServer())
servicio_empresa = ServicioEmpresa(RepositorioEmpresaSqlServer())
servicio_persona = ServicioPersona(RepositorioPersonaSqlServer())
servicio_rol = ServicioRol(RepositorioRolSqlServer())
servicio_ruta = ServicioRuta(RepositorioRutaSqlServer())
servicio_usuario = ServicioUsuario(_repo_usuario)

servicio_cliente = ServicioCliente(RepositorioClienteSqlServer())
servicio_vendedor = ServicioVendedor(RepositorioVendedorSqlServer())
servicio_factura = ServicioFactura(RepositorioFacturaSqlServer())
servicio_rol_usuario = ServicioRolUsuario(RepositorioRolUsuarioSqlServer())
servicio_rutarol = ServicioRutaRol(RepositorioRutaRolSqlServer())
servicio_usuario_roles = ServicioUsuarioConRoles(RepositorioUsuarioConRolesSqlServer())

servicio_sesion = ServicioSesion(_repo_acceso, _repo_usuario)


def obtener_repositorio_acceso() -> IRepositorioAcceso:
    """Expone el contrato abstracto IRepositorioAcceso para la capa de autorización."""
    return _repo_acceso
