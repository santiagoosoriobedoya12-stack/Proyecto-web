# Bitácora de Prompts — Versión 2: Las 6 Tablas Con Clave Foránea

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar la integridad referencial, el manejo de excepciones HTTP 409 Conflict, los procedimientos almacenados, el disparador contable y los componentes Blazor con desplegables dinámicos.

---

## 1. Prompt Arquitectónico Base (Integridad Referencial y 409 Conflict)
```text
Actúa como un arquitecto de software senior en C# y SQL Server.
Implementa el manejo de claves foráneas y errores relacionales en ASP.NET Core:
1. Crea la excepción de dominio ConflictoExcepcion.cs que mapee al código HTTP 409 Conflict.
2. En los repositorios Dapper, captura SqlException con número 547 (foreign_key_violation) y 2627/2601 (unique_violation) y lánzala como ConflictoExcepcion.
3. En los controladores, traduce ConflictoExcepcion a HTTP 409 con el detalle del conflicto en JSON.
4. En los borrados de tablas padre (persona, empresa, rol, usuario), protege contra eliminación huérfana retornando 409 si tienen hijos asociados.
```

---

## 2. Prompts Específicos por Tabla y Recurso

### Tabla 7: `cliente` (FK a `persona` y `empresa`)
```text
Genera el corte vertical completo para 'cliente':
1. Modelo Cliente.cs (id IDENTITY, credito decimal, fkcodpersona NOT NULL, fkcodempresa NULLABLE).
2. Peticiones: ClienteCrear, ClienteReemplazo, ClienteActualizar.
3. Repositorio Dapper capturando error 547 si la persona o la empresa no existen (409).
4. ServicioCliente y ClienteController con rutas /api/cliente.
5. Vista Blazor Clientes.razor con menús desplegables (<select>) alimentados desde la API, mostrando nombres legibles y enviando códigos, permitiendo 'Ninguna' para persona natural.
```

### Tabla 8: `vendedor` (FK a `persona`)
```text
Genera el corte vertical completo para 'vendedor':
1. Modelo Vendedor.cs (id IDENTITY, carnet, direccion, fkcodpersona NOT NULL).
2. Peticiones segregadas para POST, PUT y PATCH.
3. Repositorio Dapper con captura de 547 -> 409.
4. ServicioVendedor y VendedorController.
5. Vista Blazor Vendedores.razor con selector dinámico de persona traído de la API.
```

### Tablas 9 y 10: `factura` y `productosporfactura` (Maestro-Detalle con SPs y Trigger)
```text
Genera el módulo de facturación operado 100% mediante Stored Procedures y Triggers:
1. Modelos: Factura.cs (con lista de productos), FacturaDetalle.cs, ProductoDeFactura.cs.
2. Peticiones: FacturaCrear e ItemFacturaCrear. REGLA ESTRICTA: El cliente NO envía totales, subtotales ni precio unitario.
3. RepositorioFacturaSqlServer que invoque exclusivamente:
   - sp_listar_facturas_y_productosporfactura
   - sp_consultar_factura_y_productosporfactura
   - sp_insertar_factura_y_productosporfactura (pasando JSON al SP)
   - sp_anular_factura (para anulación lógica estado = 0 y retorno de stock)
4. Sin endpoints DELETE físico en la API.
5. Vista Blazor EmitirFactura.razor: Maestro-detalle interactivo en memoria (agregar ítems, validar stock, eliminar renglones, emitir) y vista Facturas.razor con detalle desplegable y anulación.
```

### Tabla 11: `rol_usuario` (Tabla puente M:N)
```text
Genera el corte vertical para la tabla puente 'rol_usuario':
1. Modelo RolUsuario.cs (fkemail, fkidrol) con clave primaria compuesta.
2. Petición RolUsuarioCrear.cs.
3. RolUsuarioController en /api/rol-usuario:
   - GET /api/rol-usuario (todos)
   - GET /api/rol-usuario/usuario/{email}
   - GET /api/rol-usuario/rol/{idrol}
   - POST /api/rol-usuario (409 si duplicado)
   - DELETE /api/rol-usuario/{email}/{idrol} (eliminación por clave compuesta, sin PUT ni PATCH)
4. Vista Blazor RolesPorUsuario.razor.
```

### Tabla 12: `rutarol` (Tabla puente M:N)
```text
Genera el corte vertical para la tabla puente 'rutarol':
1. Modelo RutaRol.cs (fkidruta, fkidrol) con clave compuesta.
2. Petición RutaRolCrear.cs.
3. RutaRolController en /api/rutarol con endpoints de asociación, listado por extremos y desvinculación.
4. Vista Blazor PermisosPorRol.razor con asignación interactiva y nota didáctica de v3.
```

### Recurso Especial: `usuario-con-roles` (Transacción con 5 Stored Procedures)
```text
Genera el recurso compuesto 'usuario-con-roles':
1. Modelo UsuarioConRoles.cs y DTO UsuarioConRolesCrear.cs con [MinLength(1)] para exigir al menos un rol (422 si vacío).
2. DTO UsuarioConRolesActualizar.cs donde la contraseña vacía conserva la anterior.
3. Repositorio que ejecuta los 5 SPs: crear_usuario_con_roles, actualizar_usuario_con_roles, consultar_usuario_con_roles, listar_usuarios_con_roles, eliminar_usuario_con_roles.
4. Servicio que mapea la lista de int a JSON de objetos [{"fkidrol": 1}] para OPENJSON.
5. Vista Blazor UsuariosYRoles.razor con casillas de verificación (checkboxes) para selección múltiple de roles.
```

