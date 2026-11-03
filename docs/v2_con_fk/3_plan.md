# Plan técnico — Versión 2: las tablas con clave foránea

> | | |
> |---|---|
> | **Lo que hay que construir** | [2_spec.md](2_spec.md) |
> | **Los formatos exactos** | [6_contracts.md](6_contracts.md) |
> | **El orden** | [8_tasks.md](8_tasks.md) |

---

## 1. La pila, que no cambia

| | |
|---|---|
| **Lenguaje** | C# sobre ASP.NET Core (**.NET 10**) |
| **Base** | SQL Server 2022 |
| **Acceso a datos** | **Dapper** — SQL a la vista, sin ORM |
| **Interfaz gráfica** | **Blazor Server**, en su propio proyecto y su propio contenedor |
| **Todo** | Docker Compose, con un solo `up` |

> **No se cambia nada de la pila en la v2.** Si en el plan apareciera un
> paquete nuevo, un ORM o un framework de front, sería anticipación — y la
> constitución la prohíbe.

## 2. Las capas, que tampoco cambian

```
   HTTP  →  CONTROLADOR  →  SERVICIO  →  REPOSITORIO  →  SQL Server
            sabe de HTTP    sabe de      sabe de SQL
            y de nada más   reglas       y de nada más
```

| Capa | Qué sabe | Qué NO sabe |
|---|---|---|
| **Controlador** | Verbos, códigos de estado, el sobre de la respuesta | Nada de SQL |
| **Servicio** | Las reglas del dominio | **Nada de HTTP.** No conoce `IActionResult` ni los códigos |
| **Repositorio** | El SQL, o el `CALL`/`EXEC` del procedimiento | Nada de reglas |

**Y la regla que la v2 pone a prueba:** el servicio **no puede** devolver un
código HTTP. Lanza una excepción del dominio —`NoEncontradoExcepcion`,
`ConflictoExcepcion`— y **el controlador** la traduce. Si el servicio
devolviera un 409, la capa de reglas sabría de HTTP y la frontera estaría
rota.

## 3. Lo que la v2 agrega, archivo por archivo

### 3.1 Los dos recursos calcados — `cliente` y `vendedor`

Rebanada vertical completa, idéntica en forma a la de la v1:

```
Modelos/Cliente.cs                    Modelos/Vendedor.cs
Peticiones/ClienteCrear.cs            Peticiones/VendedorCrear.cs
Peticiones/ClienteReemplazo.cs        Peticiones/VendedorReemplazo.cs
Peticiones/ClienteActualizar.cs       Peticiones/VendedorActualizar.cs
Repositorios/IRepositorioCliente.cs   Repositorios/IRepositorioVendedor.cs
Repositorios/RepositorioClienteSqlServer.cs   …VendedorSqlServer.cs
Servicios/IServicioCliente.cs         Servicios/IServicioVendedor.cs
Servicios/ServicioCliente.cs          Servicios/ServicioVendedor.cs
Controllers/ClienteController.cs      Controllers/VendedorController.cs
```

### 3.2 `factura` — el repositorio que no escribe SQL de tablas

```
Modelos/Factura.cs                    el maestro, con la lista adentro
Modelos/ProductoDeFactura.cs          un renglón del detalle
Peticiones/FacturaCrear.cs            { fkidcliente, fkidvendedor, productos[] }
Repositorios/IRepositorioFactura.cs
Repositorios/RepositorioFacturaSqlServer.cs   CUATRO procedimientos, cero SELECT de tablas
Servicios/IServicioFactura.cs
Servicios/ServicioFactura.cs
Controllers/FacturaController.cs      4 endpoints, no 6
```

### 3.3 Las dos puentes — `rol_usuario` y `rutarol`

```
Modelos/RolUsuario.cs                 dos claves, ningún campo
Modelos/RutaRol.cs
Peticiones/RolUsuarioCrear.cs         sin Reemplazo ni Actualizar: NO HAY PUT NI PATCH
Peticiones/RutaRolCrear.cs
… repositorio, servicio y controlador de cada una
```

Rutas:
- `rol_usuario` -> `api/rol-usuario` (con guion)
- `rutarol` -> `api/rutarol` (sin guion)

### 3.4 `usuario-con-roles` — el recurso que no es una tabla

```
Modelos/UsuarioConRoles.cs            el usuario con sus roles anidados
Peticiones/UsuarioConRolesCrear.cs       { email, contrasena, roles:[2,3] }
Peticiones/UsuarioConRolesActualizar.cs  { contrasena?, roles:[1] }
Repositorios/RepositorioUsuarioConRolesSqlServer.cs  CINCO procedimientos
Servicios/ServicioUsuarioConRoles.cs      ← traduce la lista de enteros al JSON esperado con 'fkidrol'
Controllers/UsuarioConRolesController.cs
```

### 3.5 Las interfaces gráficas

| Archivo | Qué enseña |
|---|---|
| `Clientes.razor` | Dos desplegables, uno opcional con «(ninguna)» -> null |
| `Vendedores.razor` | Uno obligatorio: el mismo patrón, sin la opción vacía |
| `Facturas.razor` | La tabla de facturas, con «Ver» y «Anular» |
| `EmitirFactura.razor` | El maestro-detalle. La pieza central — y por eso tiene pantalla propia |
| `FacturaDetalle.razor` | Una factura con sus renglones, con los subtotales que calculó la base |
| `UsuariosYRoles.razor` | El mismo patrón con casillas |
| `RolesPorUsuario.razor` | La puente cruda: sin editar, y el quitar con dos claves |
| `PermisosPorRol.razor` | La otra puente, con el aviso de que todavía no aplica nada |
