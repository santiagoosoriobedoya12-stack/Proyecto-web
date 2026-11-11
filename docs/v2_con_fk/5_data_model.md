# Modelo de datos — Versión 2: las seis tablas con clave foránea

> **Las 12 tablas existen en la base desde la v1** (según la
> [arquitectura general](../README.md)). Lo que este documento describe es
> **lo que el código de la v2 empieza a usar**, no lo que se crea: **la v2 no
> crea ni modifica una sola tabla.**
>
> La base viene **dada** en `db/bdfacturas.sql`.

---

## 1. Las seis tablas, y qué las hace distintas de las de la v1

| Tabla | Clave primaria | Sus claves foráneas |
|---|---|---|
| `cliente` | `id` **SERIAL** | `fkcodpersona` → `persona` **obligatoria** · `fkcodempresa` → `empresa` **nullable** |
| `vendedor` | `id` **SERIAL** | `fkcodpersona` → `persona` **obligatoria** |
| `factura` | `numero` **SERIAL** | `fkidcliente` → `cliente` · `fkidvendedor` → `vendedor` |
| `productosporfactura` | **compuesta** (`fknumfactura`, `fkcodproducto`) | las dos columnas son FK |
| `rol_usuario` | **compuesta** (`fkemail`, `fkidrol`) | las dos columnas son FK |
| `rutarol` | **compuesta** (`fkidruta`, `fkidrol`) | las dos columnas son FK |

**Dos cosas nuevas aparecen aquí, y ninguna existía en la v1:**

| | Qué implica en el código |
|---|---|
| **La clave la genera la BASE** (`IDENTITY`) | El `id` **no se envía** al crear. En el modelo **no** puede ser `required`, porque al construir el objeto todavía no existe |
| **La clave primaria COMPUESTA** | No hay un `id` que identifique la fila: la identifican sus dos columnas. De ahí que el borrado lleve **dos** valores en la URL, y que **no haya PUT ni PATCH** — la fila no tiene campos que cambiar |

## 2. El mapa de dependencias — y por qué la v1 era primero

```
   persona ──┬──> cliente ──┐
             │              ├──> factura ──> productosporfactura <── producto
             └──> vendedor ─┘
   empresa ──────> cliente

   usuario ──> rol_usuario <── rol ──> rutarol <── ruta
```

**A la izquierda, lo que no depende de nadie: las seis de la v1.** A la
derecha, lo que necesita que las otras existan.

> **Eso es el criterio del reparto, y no es arbitrario:** no se puede insertar
> un cliente antes de que exista su persona. La v1 construye lo que se puede
> llenar **solo**; la v2, lo que necesita a los demás.

## 3. El disparador — el que calcula

```sql
-- trg_prodfact_insert, sobre productosporfactura
```

| Cuándo | Qué hace |
|---|---|
| **Al insertar un renglón** | Valida que haya **stock suficiente** —y si no, error 50001— · toma el **precio del producto** · calcula `subtotal = cantidad × valorunitario` · **descuenta** el stock · **recalcula** el `total` de la factura |
| **Al borrar un renglón** | Devuelve el stock y recalcula el total |

**Tres consecuencias directas para la API, y las tres son requisitos:**

| | |
|---|---|
| **1** | El cuerpo de crear una factura **no lleva** `total` ni `subtotal`: los pone el disparador |
| **2** | Tampoco lleva el **precio**: lo toma del producto. Mandarlo permitiría vender a un precio inventado |
| **3** | «Stock insuficiente» **no es un error de programación**: es una regla del negocio que vive en la base, y su mensaje viaja al cliente en el `detalle` |

> **Por qué en un disparador y no en el servicio:** porque así la regla se
> cumple **sin importar quién escriba** — la API, un script de carga, alguien
> con `sqlcmd`. Una regla de integridad en la capa de aplicación solo protege a
> quien pasa por la aplicación.

## 4. Los procedimientos que la v2 usa

### 4.1 De `factura` — cuatro de los seis que existen

| Procedimiento | Qué devuelve en su `@p_resultado OUTPUT` |
|---|---|
| `sp_listar_facturas_y_productosporfactura` | Un arreglo de facturas, cada una con los **nombres** de cliente y vendedor y sus renglones **anidados** |
| `sp_consultar_factura_y_productosporfactura` | `{factura:{…}, productos:[…]}`. Inexistente → error 50003 «no existe» |
| `sp_insertar_factura_y_productosporfactura` | La factura creada, **ya calculada** por el disparador. Recibe el detalle como **JSON** |
| `sp_anular_factura` | El estado `'anulada'` y el stock devuelto. Ya anulada → error 50010 «ya está anulada» |

**Los otros dos existen en la base y la v2 NO los expone:**
`sp_actualizar_factura_y_productosporfactura` y
`sp_borrar_factura_y_productosporfactura`. La operación del negocio es **anular**.

### 4.2 De `usuario` con sus roles — los cinco

| Procedimiento | |
|---|---|
| `listar_usuarios_con_roles` | Todos, con sus roles agrupados **por la base** |
| `consultar_usuario_con_roles` | Uno. Inexistente → error 50008 «no existe» |
| `crear_usuario_con_roles` | El usuario **y** sus roles, en una transacción |
| `actualizar_usuario_con_roles` | **Solo cambia la contraseña si llega con algo**, y **reemplaza** los roles |
| `eliminar_usuario_con_roles` | Borra el detalle y el maestro juntos |

**La forma del JSON de roles:**

```
lo que el procedimiento espera:  [{"fkidrol":1},{"fkidrol":3}]
lo que el formulario tiene:      [1, 3]
                                  ↑ la traducción la hace el SERVICIO
```

> **Si se escribe `idrol` en vez de `fkidrol`**, el procedimiento no encuentra
> nada y el usuario queda **sin roles — sin un solo error**.

**Y la clave que devuelve:**
Devuelve **`idrol`**, no `id`. De ahí el `[JsonPropertyName("idrol")]`.

### 4.3 De `rutarol` — tres

`listar_rutarol`, `crear_rutarol` y `eliminar_rutarol`.

## 5. Las restricciones, y cómo se ven desde la API

| Lo que la base impide | Cómo llega al cliente |
|---|---|
| Insertar un cliente con una `persona` que no existe | **409** (`el error 547`) |
| Repetir una pareja en una tabla puente | **409** (`los errores 2627 y 2601`) |
| Borrar una `persona` que es cliente o vendedor | **409** — con el **nombre de la restricción** del motor en el `detalle` |
| Vender más de lo que hay en stock | **500**, con el mensaje del **disparador** en el `detalle` |
