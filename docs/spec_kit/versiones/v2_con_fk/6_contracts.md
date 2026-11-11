# Contratos HTTP — Versión 2: los recursos con clave foránea

> | | |
> |---|---|
> | **La ruta** | [Mapa de versiones](../../../README.md) |
> | **Lo anterior** | [v1](../v1_sin_fk/6_contracts.md) — **sigue vigente sin un solo cambio** |
> | **La API** | `http://localhost:8035` · Swagger en `/swagger` |
> | **La interfaz gráfica** | `http://localhost:8099` |

**Aquí está solo lo NUEVO.** Las convenciones son las mismas de la v1 y no se
repiten: el sobre en los listados, los errores como
`{estado, mensaje, detalle}`, el 422 con `errores:[…]`.

---

## A. `api/cliente` — CRUD con DOS claves foráneas (6 endpoints)

```
GET    /api/cliente[?limite=N]   → 200 {tabla:"cliente", limite, total, datos:[…]}
                                   · 204 vacia · 400 si limite <= 0
GET    /api/cliente/{id}         → 200 {"id":1,"credito":500000,
                                        "fkcodpersona":"P001","fkcodempresa":"E001"}
                                   · 404 si no existe
POST   /api/cliente              body {credito, fkcodpersona, fkcodempresa?}
                                 → 200 {estado, mensaje} · 422 · 409 si la FK no existe
PUT    /api/cliente/{id}         body {credito, fkcodpersona, fkcodempresa?} — TODO obligatorio
                                 → 200 · 422 si falta cualquier campo · 404
PATCH  /api/cliente/{id}         body parcial
                                 → 200 · 400 si el body va vacio · 404
DELETE /api/cliente/{id}         → 200 · 404
```

## B. `api/vendedor` — CRUD con UNA clave foránea (6 endpoints)

```
GET    /api/vendedor[?limite=N]  → 200 {tabla:"vendedor", limite, total, datos:[…]}
GET    /api/vendedor/{id}        → 200 {"id":1,"carnet":1001,
                                        "direccion":"Calle 10 # 20-30","fkcodpersona":"P002"}
POST   /api/vendedor             body {carnet, direccion, fkcodpersona} — los tres obligatorios
PUT    /api/vendedor/{id}        body {carnet, direccion, fkcodpersona} — TODO obligatorio
PATCH  /api/vendedor/{id}        body parcial
DELETE /api/vendedor/{id}        → 200 · 404
```

## C. `api/factura` — maestro-detalle por procedimientos (4 endpoints)

### C1. `GET /api/factura` — listar, con el detalle anidado

```
→ 200 {tabla:"factura", total:N, datos:[
         {numero, fecha, total, estado, fkidcliente, nombre_cliente,
          fkidvendedor, nombre_vendedor,
          productos:[{codigo_producto, nombre_producto, cantidad,
                      valorunitario, subtotal}]} ]}
```

### C2. `GET /api/factura/{numero}` — una sola

```
→ 200 {numero, fecha, total, estado, …, productos:[…]}   <- SIN sobre
· 404 si no existe
```

### C3. `POST /api/factura` — crear el maestro y el detalle, UN envío

```
body { "fkidcliente": 1, "fkidvendedor": 1,
       "productos": [ {"codigo":"PR001","cantidad":2},
                      {"codigo":"PR003","cantidad":1} ] }

→ 200 la factura COMPLETA, con fecha, subtotales y total calculados por la base
· 422 si falta un campo o si `productos` llega vacío (mínimo 1)
· 409 si el cliente, el vendedor o un producto no existen
· 500 si no hay stock suficiente — el mensaje del trigger viaja en `detalle`
```

### C4. `POST /api/factura/{numero}/anular` — el borrado lógico

```
→ 200 {mensaje, numero, estado:"anulada", …}
· 404 si la factura no existe
· 409 si YA estaba anulada
```

## D. `api/rol-usuario` — una tabla puente (5 endpoints)

```
GET    /api/rol-usuario[?limite=N]        → 200 {tabla:"rol_usuario", limite, total, datos:[…]}
                                            datos: [{"fkemail":"ana@correo.com","fkidrol":2}]
GET    /api/rol-usuario/usuario/{email}   → 200 los roles de ESE usuario
GET    /api/rol-usuario/rol/{idrol}       → 200 los usuarios de ESE rol
POST   /api/rol-usuario                   body {fkemail, fkidrol}
                                          → 200 · 409 si la pareja ya existe o una FK no
DELETE /api/rol-usuario/{email}/{idrol}   → 200 · 404
```

## E. `api/rutarol` — la otra puente (5 endpoints)

```
GET    /api/rutarol[?limite=N]          → 200 {tabla:"rutarol", limite, total, datos:[…]}
GET    /api/rutarol/ruta/{idruta}       → 200 los roles que entran a ESA interfaz
GET    /api/rutarol/rol/{idrol}         → 200 las interfaces de ESE rol
POST   /api/rutarol                     body {fkidruta, fkidrol}
DELETE /api/rutarol/{idruta}/{idrol}    → 200 · 404
```

## F. `api/usuario-con-roles` — el recurso que no es una tabla (5 endpoints)

```
GET    /api/usuario-con-roles           → 200 {tabla:"usuario_con_roles", total, datos:[
                                            {"email":"ana@correo.com",
                                             "roles":[{"idrol":2,"nombre":"vendedor"}]} ]}
GET    /api/usuario-con-roles/{email}   → 200 uno solo, con sus roles · 404
POST   /api/usuario-con-roles           body {email, contrasena, roles:[2,3]}
                                        → 200 el usuario con sus roles
                                        · 422 si falta algo o `roles` va vacío (mínimo 1)
                                        · 409 si el correo ya existe o un rol no
PUT    /api/usuario-con-roles/{email}   body {contrasena?, roles:[1]}
                                        → 200 · 404 · 409
DELETE /api/usuario-con-roles/{email}   → 200 {mensaje, email_eliminado} · 404
```

## G. Diagnóstico

```
GET /  → 200 {"mensaje":"API Facturas funcionando","version":"v2",
             "contratos":"docs/spec_kit/versiones/v2_con_fk/6_contracts.md"}
```

## H. La traducción de errores, acumulada

| Situación | Excepción interna | HTTP |
|---|---|---|
| El cuerpo no cumple las anotaciones de su petición | (la responde el framework) | **422** + `errores[]` |
| Regla de negocio (`limite <= 0`, PATCH vacío, cero roles) | `ArgumentException` | **400** |
| No existe | `NoEncontradoExcepcion` | **404** |
| Clave foránea inexistente, pareja repetida, correo duplicado | `ConflictoExcepcion` | **409** |
| Ya estaba anulada | `ConflictoExcepcion` | **409** |
| Sin stock, base caída, lo imprevisto | `SqlException` y demás | **500** + el mensaje del motor en `detalle` |
