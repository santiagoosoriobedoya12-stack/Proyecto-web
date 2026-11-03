# Contratos de Endpoints — Versión 3: Autenticación y Seguridad

> Especificación OpenAPI / REST exacta de los nuevos endpoints y respuestas de error de la Versión 3.

---

## 1. `POST /api/auth/login` (Iniciar Sesión)

Permite al cliente identificarse y obtener un token JWT firmado.

### Petición
- **Método:** `POST`
- **Ruta:** `/api/auth/login`
- **Acceso:** **Público** (no requiere token previo)
- **Cuerpo JSON:**
```json
{
  "email": "admin@empresa.com",
  "contrasena": "Admin123*"
}
```

### Respuestas

#### `200 OK` — Autenticación exitosa
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipo": "Bearer",
  "expiraEnSegundos": 86400,
  "usuario": {
    "email": "admin@empresa.com",
    "roles": [
      {
        "id": 1,
        "nombre": "Administrador"
      }
    ]
  }
}
```

#### `401 Unauthorized` — Credenciales inválidas
```json
{
  "tipo": "seguridad",
  "codigo": 401,
  "mensaje": "Credenciales inválidas. Correo o contraseña incorrectos."
}
```

#### `422 Unprocessable Entity` — Datos mal formados o incompletos
```json
{
  "tipo": "validacion",
  "codigo": 422,
  "mensaje": "El campo email y contrasena son obligatorios."
}
```

---

## 2. `GET /api/auth/perfil` (Consultar Usuario Actual)

Retorna la información del usuario en sesión a partir de los claims del token.

### Petición
- **Método:** `GET`
- **Ruta:** `/api/auth/perfil`
- **Cabecera Requerida:** `Authorization: Bearer <token>`

### Respuestas

#### `200 OK`
```json
{
  "email": "vendedor@empresa.com",
  "roles": [
    {
      "id": 2,
      "nombre": "Vendedor"
    }
  ]
}
```

#### `401 Unauthorized` (Token ausente, corrupto o expirado)
```json
{
  "tipo": "seguridad",
  "codigo": 401,
  "mensaje": "No autenticado. Token de autorización ausente o inválido."
}
```

---

## 3. Códigos de Error Globales del Middleware de Seguridad

### `401 Unauthorized` (No autenticado)
Emitido cuando cualquier endpoint protegido es invocado sin credenciales válidas:
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "tipo": "seguridad",
  "codigo": 401,
  "mensaje": "Acceso no autorizado. Debe proporcionar un token JWT válido."
}
```

### `403 Forbidden` (Acceso denegado por RBAC)
Emitido cuando el usuario tiene un token válido, pero su rol no tiene permiso para la ruta en la base de datos:
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

