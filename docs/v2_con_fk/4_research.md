# Investigación y decisiones — Versión 2: las tablas con clave foránea

> **Qué es este documento:** las decisiones que se tomaron y **las
> alternativas que se descartaron, con su razón**.

---

## D1 — La clave foránea inexistente: ¿422 o 409?

| Opción | Argumento |
|---|---|
| **422** | «El dato está mal, y el 422 es para datos malos» |
| **409** ✅ | **El dato NO está mal: tiene la forma correcta.** `"NOEXISTE"` es un texto de la longitud permitida. Lo que se rompe es el **estado** de la base: esa fila no está |

**Lo que decide:** el 422 se reserva para lo que la **petición** puede
rechazar sin consultar nada —un campo que falta, un número negativo, un tipo
equivocado—. Saber si `P001` existe **requiere ir a la base**, y eso ya es
estado.

## D2 — `fkcodempresa` sin empresa: ¿cadena vacía o `null`?

**`null`.** Y conviene decir por qué `""` es tentador y está mal: un
`<select>` de HTML no sabe de `null` —su opción vacía vale `""`— así que lo
más fácil es dejar que viaje tal cual.

```
""     ->  un codigo de empresa de cero caracteres, que NO existe  ->  409
null   ->  «este cliente no tiene empresa»                         ->  200
```

**La conversión se hace en la interfaz gráfica, justo antes de enviar.**

## D3 — `factura`: ¿SQL en el repositorio, o procedimientos?

| Opción | Argumento |
|---|---|
| **SQL en C#** | Más fácil de leer para quien no sabe SQL procedimental |
| **Procedimientos** ✅ | **La base ya los trae** —seis, escritos y probados— y la transacción es suya |

## D4 — La traducción de los errores del motor

En SQL Server, las restricciones de clave foránea disparan el error 547 (`SqlException.Number == 547`), mientras que violaciones de unicidad disparan 2627 o 2601. Para los procedimientos con `THROW`, se identifican por el número (50001 stock insuficiente, 50003 no existe, 50010 ya anulada) o por el texto del mensaje («no existe», «ya está anulada»).

## D5 — Un recurso `usuario-con-roles` además de los otros dos

Ya existen `api/usuario` y `api/rol-usuario`. ¿Por qué un tercero?
Porque con dos llamadas, un fallo en la segunda deja un usuario sin roles. La base ya trae `crear_usuario_con_roles` para que sea una sola transacción atómica.

## D6 — El total de la factura: ¿lo manda el front?

**No.** Lo calcula el disparador, y el front solo lo muestra como estimado sin enviarlo.

## D7 — El detalle: ¿renglón por renglón, o todo junto?

**Todo junto, en un solo envío.** Criterio 13: agrega 3 renglones en memoria, quita 1, emite -> llegan 2.

## D8 — Las interfaces de las tablas puente: ¿se llaman como la tabla?

**No.** El nombre de un archivo `.razor` es el nombre de una clase. `RolUsuario.razor` taparía el modelo `RolUsuario` y no compilaría. De ahí: `RolesPorUsuario.razor` y `PermisosPorRol.razor`.

## D9 — Lo que NO se investigó, y por qué

- Un ORM: La constitución lo prohíbe.
- Paginación con offset/cursores: Con `?limite` alcanza.
- Bloqueo optimista: Fuera de alcance del curso.
