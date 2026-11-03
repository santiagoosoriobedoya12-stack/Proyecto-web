# Modelo de datos ÔÇö Versi├│n 1: la BD completa (dada) y las SEIS tablas sin FK

> **Versi├│n 1** ┬À La base de datos NO se dise├▒a en esta versi├│n: **viene
> dada** ([4_research.md](4_research.md) D4). Este documento describe lo que
> hay y lo ├║nico que la v1 puede tocar.

---

## 1. La base de datos `bdfacturas` (dada, completa)

El script **provisto** `db/bdfacturas.sql` (dialecto SQL Server) crea la
base `bdfacturas_sqlserver_local` completa. SQL Server lo ejecuta SOLO la
PRIMERA vez (est├í montado en `/docker-entrypoint-initdb.d/` o ejecutado por el inicializador).

**12 tablas** en dos m├│dulos:

```text
FACTURACI├ôN                          SEGURIDAD (RBAC)
persona ÔåÉÔöÇÔöÇ cliente ÔåÉÔöÇÔöÇ factura      usuario ÔöÇÔöÇÔöÉ
   Ôåæ          Ôåæ            Ôåæ                   Ôö£ÔöÇÔöÇ rol_usuario ÔöÇÔöÇ rol
empresa ÔöÇÔöÇÔöÇÔöÇÔöÇÔöÇÔöÿ       productosporfactura      Ôöé                   Ôöé
vendedor ÔöÇÔöÇ persona        Ôåæ                   ruta ÔöÇÔöÇ rutarol ÔöÇÔöÇÔöÇÔöÇÔöÿ
                        producto
```

Adem├ís: **triggers** que mantienen `factura.total` y `producto.stock` al
insertar/editar/borrar renglones del detalle, y **procedimientos
almacenados** de consulta ÔÇö todos esperando a las versiones siguientes.

**Datos de ejemplo:** 8 productos (PR001ÔÇªPR008), 6 personas, 6 facturas con
detalle, usuarios y roles. Credenciales de BD (did├ícticas): `sa` /
`Paradigmas123!`.

## 2. Lo ├ÜNICO que la v1 puede nombrar: la tabla `producto`

| Columna | Tipo (SQL Server) | Regla |
|---|---|---|
| `codigo` | `VARCHAR(10)` | **PK** ÔÇö texto de 1 a 10 caracteres |
| `nombre` | `VARCHAR(100)` | No nulo, no vac├¡o |
| `stock` | `INT` | No nulo, ÔëÑ 0 (regla de la API) |
| `valorunitario` | `DECIMAL(18,2)` | No nulo, ÔëÑ 0 (regla de la API) |

En C#, esa fila viaja como el modelo entidad:

```csharp
public class Producto
{
    public required string Codigo { get; set; }
    public required string Nombre { get; set; }
    public int Stock { get; set; }
    public decimal Valorunitario { get; set; }   // decimal = el tipo para dinero
}
```

## 3. Las dos murallas de validaci├│n

1. **La API** (las peticiones por verbo con anotaciones): forma, tipos y
   rangos ÔåÆ 422 con lista de errores, ANTES de tocar la BD.
2. **La BD** (PK, NOT NULL, FK y triggers): la ├║ltima l├¡nea de defensa ÔÇö
   un c├│digo duplicado viola la PK y el motor lo rechaza aunque la API
   tuviera un bug (la API lo reporta como 500 con el error del motor).

## 4. Reglas de esta versi├│n

- El c├│digo de la v1 **solo puede nombrar las SEIS tablas sin clave
  for├ínea** ÔÇö`producto`, `empresa`, `persona`, `rol`, `ruta`, `usuario`ÔÇö. Las
  otras seis existen en la base pero son territorio de la v2.
- **`usuario` y `rol` S├ì son de esta versi├│n**, aunque sean del control de
  acceso: el criterio es no tener clave for├ínea, y no la tienen. Lo que llega
  en la v3 **no es su CRUD** ÔÇöese es esteÔÇö sino la sesi├│n y el permiso.
- La BD **no se modifica**: ni columnas nuevas, ni ├¡ndices, ni datos
  semilla distintos. Si algo parece faltar, es de otra versi├│n.
- El reset completo es de Docker, no de SQL:
  `docker compose down -v && docker compose up -d` (borra el volumen y el
  inicializador vuelve a crear todo).

---

## Lo que el motor cambia, y se descubri├│ EJECUTANDO

| | En SQL Server (este proyecto) | En PostgreSQL (la v5) |
|---|---|---|
| **La llave autoincremental** | `INT IDENTITY(1,1)` | `SERIAL` |
| **El valor reci├®n insertado** | `SCOPE_IDENTITY()`, en un `SELECT` aparte | `RETURNING`, en el mismo `INSERT` |
| **La clave for├ínea violada** | error **547** | `SQLSTATE` **23503** |
| **El duplicado** | **2627** (llave primaria) y **2601** (├¡ndice ├║nico) ÔÇö **son dos** | `SQLSTATE` **23505**, uno solo |
| **Concatenar texto agrupado** | `STRING_AGG(...)` **sin `DISTINCT`**: T-SQL no lo acepta | `STRING_AGG(DISTINCT ...)` s├¡ |
| **El tipo de `COUNT()` y `SUM(int)`** | `int` | `bigint` |
