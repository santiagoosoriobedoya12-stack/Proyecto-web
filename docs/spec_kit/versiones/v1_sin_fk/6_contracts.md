# Contratos HTTP ÔÇö Versi├│n 1: seis recursos ├ù seis endpoints

> **Versi├│n 1** ┬À Base: `http://localhost:8032`. Estos contratos se cumplen
> **al pie de la letra** (constituci├│n, Art├¡culo 7): mismos verbos, rutas,
> c├│digos y formatos.

---

## 0. Convenciones globales

- Lecturas con **envoltura**: `{tabla, limite, total, datos:[ÔÇª]}`.
- Errores SIEMPRE `{estado, mensaje, detalle}`; el 422 lleva adem├ís
  `errores:[ÔÇª]` (la lista de mensajes de la validaci├│n de la petici├│n).

| Situaci├│n | HTTP |
|---|---|
| Body inv├ílido seg├║n la **petici├│n del verbo** | **422** con `errores:[ÔÇª]` |
| Regla de negocio rota (l├¡mite Ôëñ 0, PATCH sin campos) | **400** |
| El recurso no existe | **404** |
| La BD rechaza (PK duplicada) o falla | **500** (error del motor en `detalle`) |

> ## Los SEIS recursos de la v1
>
> Lo que sigue documenta `producto` **en detalle**. Los otros cinco
> ÔÇö`empresa`, `persona`, `rol`, `ruta`, `usuario`ÔÇö tienen **exactamente el
> mismo contrato**, cambiando la ruta y los campos:
>
> | Recurso | Ruta base | Clave | Campos del body |
> |---|---|---|---|
> | `producto` | `/api/producto` | `codigo` (texto) | `nombre`, `stock`, `valorunitario` |
> | `empresa` | `/api/empresa` | `codigo` (texto) | `nombre` |
> | `persona` | `/api/persona` | `codigo` (texto) | `nombre`, `email`, `telefono` |
> | `rol` | `/api/rol` | `id` (IDENTITY) | `nombre` |
> | `ruta` | `/api/ruta` | `id` (IDENTITY) | `ruta`, `descripcion` |
> | `usuario` | `/api/usuario` | `email` (texto) | `contrasena` |
>
> **Las dos de llave `IDENTITY` tienen una diferencia que importa:** el `POST`
> **no manda el `id`** ÔÇölo genera la baseÔÇö, as├¡ que su petici├│n de creaci├│n no
> lo lleva. Pedirlo obligar├¡a al cliente a inventar una llave.
>
> **Son 36 endpoints** (seis recursos ├ù seis verbos) m├ís el de diagn├│stico.

## 1. `GET /` ÔÇö Diagn├│stico

```json
{
  "mensaje": "API Facturas funcionando",
  "version": "v1",
  "contratos": "docs/spec_kit/versiones/v1_sin_fk/6_contracts.md"
}
```

Adem├ís: `GET /swagger` abre la **documentaci├│n interactiva** (Swagger UI).

## 2. `GET /api/producto[?limite=N]` ÔÇö Listar

```
GET /api/producto?limite=3
ÔåÆ 200 { "tabla":"producto", "limite":3, "total":3,
        "datos":[ {"codigo":"PR001","nombre":"Laptop Lenovo IdeaPad","stock":17,"valorunitario":2500000.00}, ÔÇª ] }
ÔåÆ 204 (sin cuerpo) si la tabla est├í vac├¡a
ÔåÆ 400 si limite <= 0
```

## 3. `GET /api/producto/{codigo}` ÔÇö Obtener uno

```
GET /api/producto/PR001
ÔåÆ 200 {"codigo":"PR001","nombre":"Laptop Lenovo IdeaPad","stock":17,"valorunitario":2500000.00}

GET /api/producto/PR999
ÔåÆ 404 {"estado":404,"mensaje":"Producto no encontrado.","detalle":"No existe un producto con codigo = PR999"}
```

## 4. `POST /api/producto` ÔÇö Crear (body completo, con c├│digo)

Body (petici├│n `ProductoCrear` ÔÇö todos obligatorios):

```
POST /api/producto   body {"codigo":"PR009","nombre":"Webcam","stock":10,"valorunitario":350000}
ÔåÆ 200 {"estado":200,"mensaje":"Producto creado exitosamente."}

body {"codigo":"PR009","stock":-5}          ÔåÉ sin nombre, stock negativo
ÔåÆ 422 {"estado":422,"mensaje":"Datos inv├ílidos.",
       "errores":["El campo nombre es obligatorio.",
                  "El campo stock debe ser un entero mayor o igual a 0."]}

body {"codigo":"PR001", ÔÇª}                  ÔåÉ c├│digo duplicado (PK)
ÔåÆ 500 con el error del motor en detalle
```

## 5. `PUT /api/producto/{codigo}` ÔÇö Reemplazo COMPLETO

Body (petici├│n `ProductoReemplazo` ÔÇö TODOS obligatorios; el c├│digo va en la URL):

```
PUT /api/producto/PR009   body {"nombre":"Webcam HD","stock":12,"valorunitario":380000}
ÔåÆ 200 {"estado":200,"mensaje":"Producto reemplazado exitosamente.","filasAfectadas":1}

body {"stock":99}                           ÔåÉ faltan campos: PUT es TODO o 422
ÔåÆ 422 con la lista de campos faltantes
ÔåÆ 404 si el c├│digo no existe
```

## 6. `PATCH /api/producto/{codigo}` ÔÇö Actualizaci├│n PARCIAL

Body (petici├│n `ProductoActualizar` ÔÇö todos opcionales; se escribe SOLO lo enviado):

```
PATCH /api/producto/PR009   body {"stock":99}      ÔåÉ el MISMO body que arriba
ÔåÆ 200 {"estado":200,"mensaje":"Producto actualizado exitosamente.","filasAfectadas":1}

body {}                                     ÔåÉ nada que actualizar
ÔåÆ 400 {"estado":400,"mensaje":"Par├ímetros inv├ílidos.","detalle":"No se envi├│ ning├║n campo para actualizar."}
ÔåÆ 404 si el c├│digo no existe
```

## 7. `DELETE /api/producto/{codigo}` ÔÇö Eliminar

```
DELETE /api/producto/PR009
ÔåÆ 200 {"estado":200,"mensaje":"Producto eliminado exitosamente.","filasEliminadas":1}
ÔåÆ 404 si no existe (incluido el segundo DELETE seguido)
```
