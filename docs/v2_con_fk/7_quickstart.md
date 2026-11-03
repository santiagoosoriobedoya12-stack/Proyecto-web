# Quickstart — Versión 2: arranque, regresión y prueba de humo

> | | |
> |---|---|
> | **Qué se verifica** | Los **18** criterios de [2_spec.md](2_spec.md) §5 |
> | **Los formatos** | [6_contracts.md](6_contracts.md) |
> | **La API** | `http://localhost:8035` · Swagger en `/swagger` |
> | **La interfaz gráfica** | `http://localhost:8099` |

---

## 1. Arranque — un solo comando

```powershell
docker compose up -d --build
```

Levanta **tres** contenedores: SQL Server (que se siembra solo la primera vez), la API y la interfaz gráfica.

---

## 2. La REGRESIÓN primero — la v1 no se rompió

```powershell
# El diagnostico: lo UNICO que cambia es la version
curl.exe http://localhost:8035/
# -> {"mensaje":"API Facturas funcionando","version":"v2", ...}
```

---

## 3. La prueba de humo de la v2 — la API

```powershell
# ============================================================
# CRITERIO 2 — `cliente`, los cinco verbos
# ============================================================
curl.exe http://localhost:8035/api/cliente

# crear: el `id` NO se envia, lo pone la base (IDENTITY)
curl.exe -X POST http://localhost:8035/api/cliente -H "Content-Type: application/json" -d "{\"credito\":500000,\"fkcodpersona\":\"P001\",\"fkcodempresa\":\"E001\"}"

# la pareja didactica: el MISMO cuerpo, dos verbos, dos respuestas
curl.exe -i -X PUT   http://localhost:8035/api/cliente/1 -H "Content-Type: application/json" -d "{\"credito\":900000}"   # -> 422
curl.exe -i -X PATCH http://localhost:8035/api/cliente/1 -H "Content-Type: application/json" -d "{\"credito\":900000}"   # -> 200

curl.exe -i -X DELETE http://localhost:8035/api/cliente/99   # -> 404

# ============================================================
# CRITERIO 3 — el 409 de la clave foranea, y el nulo que SI vale
# ============================================================
curl.exe -i -X POST http://localhost:8035/api/cliente -H "Content-Type: application/json" -d "{\"credito\":1000,\"fkcodpersona\":\"NOEXISTE\"}"   # -> 409

# un cliente SIN empresa: se crea, con fkcodempresa en null
curl.exe -X POST http://localhost:8035/api/cliente -H "Content-Type: application/json" -d "{\"credito\":200000,\"fkcodpersona\":\"P002\",\"fkcodempresa\":null}"

# ============================================================
# CRITERIO 4 — `vendedor`, y la integridad referencial en accion
# ============================================================
curl.exe http://localhost:8035/api/vendedor
curl.exe -X POST http://localhost:8035/api/vendedor -H "Content-Type: application/json" -d "{\"carnet\":9001,\"direccion\":\"Calle 10 # 20-30\",\"fkcodpersona\":\"P003\"}"

# borrar la PERSONA de un vendedor: la base lo impide con 409
curl.exe -i -X DELETE http://localhost:8035/api/persona/P003

# ============================================================
# CRITERIO 5 — lectura maestro-detalle
# ============================================================
curl.exe http://localhost:8035/api/factura
curl.exe http://localhost:8035/api/factura/1
curl.exe -i http://localhost:8035/api/factura/999     # -> 404

# ============================================================
# CRITERIO 6 — el disparador trabaja
# ============================================================
curl.exe http://localhost:8035/api/producto/PR001
curl.exe http://localhost:8035/api/producto/PR003

curl.exe -X POST http://localhost:8035/api/factura -H "Content-Type: application/json" -d "{\"fkidcliente\":1,\"fkidvendedor\":1,\"productos\":[{\"codigo\":\"PR001\",\"cantidad\":2},{\"codigo\":\"PR003\",\"cantidad\":1}]}"

curl.exe http://localhost:8035/api/producto/PR001
curl.exe http://localhost:8035/api/producto/PR003

# ============================================================
# CRITERIO 7 — los errores del negocio
# ============================================================
# lista vacia: lo para la peticion -> 422
curl.exe -i -X POST http://localhost:8035/api/factura -H "Content-Type: application/json" -d "{\"fkidcliente\":1,\"fkidvendedor\":1,\"productos\":[]}"

# mas cantidad que stock -> 500
curl.exe -i -X POST http://localhost:8035/api/factura -H "Content-Type: application/json" -d "{\"fkidcliente\":1,\"fkidvendedor\":1,\"productos\":[{\"codigo\":\"PR001\",\"cantidad\":999999}]}"

# anular: devuelve stock
curl.exe -X POST http://localhost:8035/api/factura/7/anular
curl.exe http://localhost:8035/api/producto/PR001

curl.exe -i -X POST http://localhost:8035/api/factura/7/anular     # -> 409 (ya estaba)
curl.exe -i -X POST http://localhost:8035/api/factura/999/anular   # -> 404

# ============================================================
# CRITERIO 8 — las dos tablas puente
# ============================================================
curl.exe http://localhost:8035/api/rol-usuario
curl.exe http://localhost:8035/api/rol-usuario/usuario/admin@correo.com
curl.exe http://localhost:8035/api/rol-usuario/rol/1

curl.exe -X POST http://localhost:8035/api/rol-usuario -H "Content-Type: application/json" -d "{\"fkemail\":\"admin@correo.com\",\"fkidrol\":2}"
curl.exe -i -X POST http://localhost:8035/api/rol-usuario -H "Content-Type: application/json" -d "{\"fkemail\":\"admin@correo.com\",\"fkidrol\":2}"   # -> 409

curl.exe -X DELETE http://localhost:8035/api/rol-usuario/admin@correo.com/2

curl.exe http://localhost:8035/api/rutarol
curl.exe -X POST http://localhost:8035/api/rutarol -H "Content-Type: application/json" -d "{\"fkidruta\":1,\"fkidrol\":2}"
curl.exe -X DELETE http://localhost:8035/api/rutarol/1/2

# ============================================================
# CRITERIO 9 — `usuario-con-roles`
# ============================================================
curl.exe http://localhost:8035/api/usuario-con-roles

curl.exe -X POST http://localhost:8035/api/usuario-con-roles -H "Content-Type: application/json" -d "{\"email\":\"nuevo@correo.com\",\"contrasena\":\"clave123\",\"roles\":[1,2]}"
curl.exe http://localhost:8035/api/usuario-con-roles/nuevo@correo.com

curl.exe -X PUT http://localhost:8035/api/usuario-con-roles/nuevo@correo.com -H "Content-Type: application/json" -d "{\"contrasena\":\"\",\"roles\":[3]}"
curl.exe http://localhost:8035/api/usuario-con-roles/nuevo@correo.com

curl.exe -i -X POST http://localhost:8035/api/usuario-con-roles -H "Content-Type: application/json" -d "{\"email\":\"x@correo.com\",\"contrasena\":\"x\",\"roles\":[]}"

curl.exe -X DELETE http://localhost:8035/api/usuario-con-roles/nuevo@correo.com

# ============================================================
# CRITERIO 10 — prueba de capas sin SQL Server
# ============================================================
docker compose exec api-facturas dotnet run --project pruebas
```

---

## 4. Las interfaces gráficas — criterios 11 a 18 (en http://localhost:8099)

| Criterio | Acción | Resultado esperado |
|---|---|---|
| **11** | Abrir `/clientes` | El desplegable de **persona** está **lleno**, con nombres |
| **12** | Crear cliente | El usuario ve el nombre y el JSON envía el código |
| **13** | En `/emitir-factura`: agregar 3 renglones, quitar 1, emitir | Llegan exactamente 2 renglones |
| **14** | Cuerpo de la petición | NO lleva `total` ni `subtotal`. Los pone el trigger |
| **15** | Eliminar factura | No existe botón eliminar; existe botón **Anular** y el stock sube |
| **16** | En `/usuarios-y-roles`: marcar dos casillas | Un solo envío atómico con los dos roles |
| **17** | Editar usuario con contraseña vacía | Los roles cambian y la contraseña se conserva |
| **18** | Apagar la API (`docker compose stop api-facturas`) | La UI sigue en pie con su aviso y cero filas |
