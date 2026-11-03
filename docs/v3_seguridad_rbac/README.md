# Versión 3 — Seguridad, Autenticación y Autorización Basada en Roles (RBAC)

> **Módulo:** Seguridad en Servicios Web y Control de Acceso  
> **Alcance:** Protección de la API y el Frontend mediante Tokens / Sesiones y Roles.  
> **Enfoque Acumulativo:** $v1 \subset v2 \subset v3$ (Las 12 tablas de la v1 y v2 ya existen y operan; la v3 añade la capa de seguridad que las protege).

---

## 1. Propósito de la Versión 3

En la **Versión 1** y **Versión 2**, el sistema era completamente abierto: cualquier cliente HTTP podía consultar, crear, modificar o anular facturas, clientes, productos y usuarios sin necesidad de identificarse.

La **Versión 3** introduce la **Capa Transversal de Seguridad**, que garantiza que:
1. **Identidad (Autenticación):** El usuario debe demostrar quién es mediante sus credenciales (`usuario` con email y contraseña).
2. **Permisos (Autorización RBAC):** Cada acción y ruta solicitada debe estar expresamente autorizada para los roles del usuario (`rol_usuario` → `rutarol` → `ruta`).
3. **Frontera de Seguridad:** Ni la base de datos ni los controladores del negocio cambian sus reglas; la seguridad se aplica como un **filtro/middleware interceptor**.

---

## 2. ¿Qué diferencia a la Versión 3 de la v1 y v2?

| Dimensión | Versión 1 (Sin FK) | Versión 2 (Con FK) | Versión 3 (Seguridad RBAC) |
|---|---|---|---|
| **Acceso a Endpoints** | Anónimo y público | Anónimo y público con validación de FK | **Protegido:** Requiere cabecera `Authorization: Bearer <token>` o Cookie de sesión |
| **Nuevos Códigos HTTP** | `200`, `201`, `204`, `400`, `404`, `422`, `500` | Incorpora **`409 Conflict`** (Integridad referencial y unicidad) | Incorpora **`401 Unauthorized`** (No autenticado) y **`403 Forbidden`** (Rol sin permiso) |
| **Control de Rutas** | La tabla `ruta` es solo un catálogo pasivo | La tabla `rutarol` asigna rutas a roles en base de datos | **Activo:** El middleware intercepta cada llamada y consulta `verificar_acceso_ruta` |
| **Experiencia en Frontend** | Navegación libre por todas las páginas | Desplegables dinámicos y maestro-detalle | **Pantalla de Login**, ocultamiento de menús según rol y redirección si expira la sesión |
| **Gestión de Sesión** | Inexistente (Stateless abierto) | Inexistente (Stateless abierto) | **Stateful / Token:** claims de usuario, expiración y refresh |

---

## 3. Arquitectura de Seguridad

```
   Petición HTTP entrante
          │
          ▼
   ┌────────────────────────────────────────────────────────┐
   │ 1. MIDDLEWARE DE AUTENTICACIÓN                         │
   │    ¿Trae token / cookie válido?                        │
   │    NO ──► Retorna 401 Unauthorized                     │
   │    SÍ ──► Extrae claims (Email, Roles)                 │
   └──────────────────────┬─────────────────────────────────┘
                          │
                          ▼
   ┌────────────────────────────────────────────────────────┐
   │ 2. MIDDLEWARE DE AUTORIZACIÓN (RBAC)                   │
   │    ¿El rol del usuario tiene asignada esta ruta?       │
   │    (Verifica contra la tabla rutarol en la base)       │
   │    NO ──► Retorna 403 Forbidden                        │
   │    SÍ ──► Pasa al controlador                          │
   └──────────────────────┬─────────────────────────────────┘
                          │
                          ▼
   ┌────────────────────────────────────────────────────────┐
   │ 3. CONTROLADORES DEL NEGOCIO (v1 y v2 intactos)        │
   │    Cliente, Vendedor, Factura, Producto, etc.          │
   └────────────────────────────────────────────────────────┘
```

---

## 4. Nuevos Códigos HTTP de la Versión 3

### `401 Unauthorized` (Falta de Identidad)
- **Cuándo se emite:**
  - La petición no envió el encabezado de autorización (`Authorization: Bearer ...`).
  - El token está vencido, mal firmado o corrupto.
  - El usuario ingresó credenciales incorrectas en el endpoint de login.
- **Significado pedagógico:** *"No sé quién eres o tu credencial no es válida"*.

### `403 Forbidden` (Falta de Permisos)
- **Cuándo se emite:**
  - El usuario está debidamente autenticado (sabemos quién es), pero sus roles **no tienen permiso** para acceder a la ruta o ejecutar el verbo HTTP solicitado.
- **Significado pedagógico:** *"Sé quién eres, pero tus roles no tienen autorización para acceder a este recurso"*.

---

## 5. El Procedimiento Almacenado de Validación

En `db/bdfacturas.sql` se encuentra el procedimiento base de verificación de acceso:
```sql
-- verificar_acceso_ruta: valida si un email tiene asignado un rol con permiso a una ruta
EXEC verificar_acceso_ruta @email = 'admin@correo.com', @ruta = '/api/factura';
```
El middleware de autorización utiliza este mecanismo para asegurar que los permisos configurados en la interfaz gráfica mediante `PermisosPorRol.razor` entren en efecto real en el backend.

---

## 6. Próximos Pasos para Iniciar la Versión 3

1. Crear el endpoint de autenticación `POST /api/auth/login` (recibe credenciales, valida hash y emite token/claims).
2. Configurar el middleware de autenticación y autorización en `api_facturas/Program.cs`.
3. Crear el servicio de autenticación y proveedor de estado (`AuthenticationStateProvider`) en `front_blazor`.
4. Diseñar la página `Login.razor` y envolver el layout principal en `<AuthorizeRouteView>`.

