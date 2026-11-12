# Bitácora de Prompts — Versión 2: Las 6 Tablas Con Clave Foránea (Python)

> Este documento recopila los prompts de ingeniería de software asistida por IA utilizados para generar en **Python (FastAPI + PyMSSQL + Jinja2)** la integridad referencial, el manejo de `ConflictoExcepcion` (`HTTP 409`), la integración con procedimientos almacenados, `OPENJSON` y las vistas web con desplegables dinámicos.

---

## 1. Prompt Arquitectónico Base (Integridad Referencial y 409 Conflict)
```text
Actúa como un arquitecto de software senior en Python y SQL Server.
Implementa el manejo de claves foráneas y errores relacionales en api_python/:
1. Agrega la excepción de dominio ConflictoExcepcion en api_python/excepciones.py y mapea su handler global en main.py a HTTP 409 Conflict.
2. En api_python/database.py implementa extraer_codigo_sql() para detectar códigos 547 (violación FK), 2627/2601 (unicidad duplicada) y 50000+ (RAISERROR/THROW de negocio) desde PyMSSQL y elevarlos como ConflictoExcepcion.
3. Protege las eliminaciones de tablas padre (persona, empresa, producto, rol, usuario) retornando 409 si existen registros dependientes.
```

---

## 2. Prompts Específicos por Tabla y Recurso

### Tabla 7: `cliente` (FK a `persona` y `empresa`)
```text
Genera el corte vertical en Python para 'cliente':
1. Esquemas ClienteCrear, ClienteReemplazo, ClienteActualizar en api_python/peticiones/esquemas.py (credito, fkcodpersona obligatoria, fkcodempresa nullable/opcional).
2. RepositorioCliente en repositorio_v2.py con JOIN a persona y empresa para devolver nombres descriptivos y traducir error 547 a 409.
3. ServicioCliente en servicio_v2.py y endpoints /api/cliente en controlador_v2.py.
4. En front_python/app.py (Flask) carga personas y empresas en paralelo con ThreadPoolExecutor y muéstralas en <select> dentro de crud_recurso.html permitiendo 'Ninguna' -> null para persona natural.
```

### Tabla 8: `vendedor` (FK a `persona`)
```text
Genera el corte vertical en Python para 'vendedor':
1. Esquemas VendedorCrear, VendedorReemplazo, VendedorActualizar en esquemas.py (carnet, direccion, fkcodpersona).
2. RepositorioVendedor, ServicioVendedor y rutas /api/vendedor.
3. Vista /vendedores en front_python con selector dinámico de persona.
```

### Tablas 9 y 10: `factura` y `productosporfactura` (Maestro-Detalle con SPs y Trigger)
```text
Genera el módulo de facturación operado 100% mediante Stored Procedures y Trigger:
1. Esquemas FacturaCrear e ItemFacturaCrear en api_python/peticiones/esquemas.py. REGLA ESTRICTA: No recibir subtotal, total ni precio unitario desde el cliente.
2. RepositorioFactura en repositorio_v2.py invocando exclusivamente:
   - sp_listar_facturas_y_productosporfactura
   - sp_consultar_factura_y_productosporfactura
   - sp_insertar_factura_y_productosporfactura (pasando JSON de detalle al SP)
   - sp_anular_factura (anulación lógica estado = 'anulada' devolviendo stock)
3. DELETE /api/factura/{numero} realiza anulación lógica (nunca borrado físico).
4. Plantillas facturas.html, emitir_factura.html y factura_detalle.html en front_python/templates/ conectadas por httpx.
```

### Tabla 11: `rol_usuario` (Tabla puente M:N)
```text
Genera el corte vertical para la tabla puente 'rol_usuario':
1. Esquema RolUsuarioCrear en esquemas.py (fkemail, fkidrol). Sin PUT ni PATCH.
2. Endpoints en controlador_v2.py bajo /api/rol-usuario:
   - GET /api/rol-usuario
   - GET /api/rol-usuario/usuario/{email}
   - GET /api/rol-usuario/rol/{idrol}
   - POST /api/rol-usuario (409 si duplicado)
   - DELETE /api/rol-usuario/{email}/{idrol}
3. Vista roles_por_usuario.html en front_python/templates/.
```

### Tabla 12: `rutarol` (Tabla puente M:N)
```text
Genera el corte vertical para la tabla puente 'rutarol':
1. Esquema RutaRolCrear (fkidruta, fkidrol).
2. Endpoints /api/rutarol con listado general, filtrado por extremo, asignación y borrado compuesto.
3. Vista permisos_por_rol.html en front_python/templates/.
```

### Recurso Especial: `usuario-con-roles` (5 Stored Procedures + OPENJSON)
```text
Genera el recurso compuesto 'usuario-con-roles' en Python:
1. Esquemas UsuarioConRolesCrear (min_length=1 en lista roles -> 422 si vacía) y UsuarioConRolesActualizar (contrasena opcional).
2. RepositorioUsuarioConRoles invocando listar_usuarios_con_roles, consultar_usuario_con_roles, crear_usuario_con_roles (@p_roles_json), actualizar_usuario_con_roles (@p_roles) y eliminar_usuario_con_roles.
3. ServicioUsuarioConRoles hasheando contraseña con BCrypt costo 12 y transformando [1, 2] en JSON [{"fkidrol": 1}, {"fkidrol": 2}] para OPENJSON.
4. Plantilla usuarios_y_roles.html con checkboxes de roles.
```
