# Paquete de Repositorios: exporta interfaces abstractas (ABC) y sus implementaciones SQL Server
from .interfaces import (
    IRepositorioProducto,
    IRepositorioEmpresa,
    IRepositorioPersona,
    IRepositorioRol,
    IRepositorioRuta,
    IRepositorioUsuario,
    IRepositorioCliente,
    IRepositorioVendedor,
    IRepositorioFactura,
    IRepositorioRolUsuario,
    IRepositorioRutaRol,
    IRepositorioUsuarioConRoles,
    IRepositorioAcceso,
)
from .repositorio_v1 import (
    RepositorioProductoSqlServer,
    RepositorioEmpresaSqlServer,
    RepositorioPersonaSqlServer,
    RepositorioRolSqlServer,
    RepositorioRutaSqlServer,
    RepositorioUsuarioSqlServer,
)
from .repositorio_v2 import (
    RepositorioClienteSqlServer,
    RepositorioVendedorSqlServer,
    RepositorioFacturaSqlServer,
    RepositorioRolUsuarioSqlServer,
    RepositorioRutaRolSqlServer,
    RepositorioUsuarioConRolesSqlServer,
)
from .repositorio_acceso import RepositorioAccesoSqlServer
