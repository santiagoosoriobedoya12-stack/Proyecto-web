# Paquete de peticiones (DTOs de entrada validados automáticamente por Pydantic -> HTTP 422)
from .esquemas import (
    ProductoCrear, ProductoReemplazo, ProductoActualizar,
    EmpresaCrear, EmpresaReemplazo, EmpresaActualizar,
    PersonaCrear, PersonaReemplazo, PersonaActualizar,
    RolCrear, RolReemplazo, RolActualizar,
    RutaCrear, RutaReemplazo, RutaActualizar,
    UsuarioCrear, UsuarioReemplazo, UsuarioActualizar,
    ClienteCrear, ClienteReemplazo, ClienteActualizar,
    VendedorCrear, VendedorReemplazo, VendedorActualizar,
    ItemFacturaCrear, FacturaCrear,
    RolUsuarioCrear, RutaRolCrear,
    UsuarioConRolesCrear, UsuarioConRolesActualizar,
    SesionCrear,
)
