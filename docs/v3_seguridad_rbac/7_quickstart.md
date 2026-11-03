# Guía Rápida y Pruebas de Humo — Versión 3: Seguridad y RBAC

> Procedimiento paso a paso para verificar la seguridad, la emisión de tokens y los códigos 401 y 403 con `curl`.

---

## 1. Prueba del Bloqueo 401 Unauthorized (Sin Token)

Intentar consultar productos sin proporcionar credenciales de autorización:

```powershell
curl.exe -i http://localhost:8035/api/producto
```

### Resultado esperado:
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "tipo": "seguridad",
  "codigo": 401,
  "mensaje": "Acceso no autorizado. Debe proporcionar un token JWT válido."
}
```

---

## 2. Inicio de Sesión y Obtención del Token JWT

Autenticarse con credenciales válidas:

```powershell
curl.exe -i -X POST http://localhost:8035/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{\"email\":\"admin@empresa.com\",\"contrasena\":\"Admin123*\"}'
```

### Resultado esperado:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipo": "Bearer",
  "expiraEnSegundos": 86400,
  "usuario": {
    "email": "admin@empresa.com",
    "roles": [{"id": 1, "nombre": "Administrador"}]
  }
}
```

---

## 3. Acceso Exitoso con Token JWT (`200 OK`)

Usar el token obtenido en el encabezado `Authorization`:

```powershell
# Guardar el token en una variable de PowerShell
$TOKEN = "PEGA_AQUI_EL_TOKEN_OBTENIDO"

curl.exe -i http://localhost:8035/api/producto `
  -H "Authorization: Bearer $TOKEN"
```

### Resultado esperado:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "tabla": "producto",
  "total": 8,
  "datos": [ ... ]
}
```

---

## 4. Prueba del Bloqueo 403 Forbidden (Rol no autorizado)

Autenticarse con un usuario que tenga un rol restringido (por ejemplo, `auditor` que no tiene permisos de facturación):

```powershell
$TOKEN_AUDITOR = "PEGA_AQUI_EL_TOKEN_DE_AUDITOR"

# Intentar crear una factura
curl.exe -i -X POST http://localhost:8035/api/factura `
  -H "Authorization: Bearer $TOKEN_AUDITOR" `
  -H "Content-Type: application/json" `
  -d '{\"fkidcliente\":1,\"fkidvendedor\":1,\"items\":[{\"fkcodproducto\":\"P001\",\"cantidad\":1}]}'
```

### Resultado esperado:
```http
HTTP/1.1 403 Forbidden
Content-Type: application/json

{
  "tipo": "seguridad",
  "codigo": 403,
  "mensaje": "Acceso denegado. Sus roles actuales no tienen autorización para el recurso solicitado.",
  "ruta": "/api/factura"
}
```

---

## 5. Prueba de Interfaz Gráfica (Blazor Server)

1. Abrir el navegador en `http://localhost:8099/`.
2. El sistema redirige automáticamente a `http://localhost:8099/login`.
3. Ingresar las credenciales de prueba.
4. El menú lateral en `NavMenu` se desbloquea mostrando únicamente los módulos a los que el rol del usuario tiene acceso.
5. El botón **Cerrar Sesión** en la esquina superior destruye la sesión y redirige de inmediato a la pantalla de login.

