# Plan Técnico — Versión 3: Seguridad, Autenticación y Autorización RBAC

> | | |
> |---|---|
> | **Lo que hay que construir** | [2_spec.md](2_spec.md) |
> | **Los contratos exactos** | [6_contracts.md](6_contracts.md) |
> | **El orden de tareas** | [8_tasks.md](8_tasks.md) |

---

## 1. La Pila Técnica y Paquetes de Seguridad

| Componente | Elección Técnica | Justificación |
|---|---|---|
| **Mecanismo de Autenticación** | **JWT (JSON Web Tokens)** | Estándar universal sin estado en servidor (stateless), ideal para APIs REST y arquitecturas de microservicios o Blazor. |
| **Librería de Tokens (.NET 10)** | `Microsoft.AspNetCore.Authentication.JwtBearer` | Middleware nativo de ASP.NET Core de alto rendimiento y validación criptográfica integrada. |
| **Algoritmo de Hash** | **PBKDF2 con Salt** (o SHA-256 con Salt criptográfica) | Protege contra ataques de diccionario y tablas arcoíris (*rainbow tables*). |
| **Motor de Autorización** | **Middleware RBAC personalizado** | Intercepta la petición antes de los controladores y consulta el procedimiento `verificar_acceso_ruta` de la base de datos. |
| **Seguridad en Frontend** | **`AuthenticationStateProvider` en Blazor Server** | Mecanismo oficial de Blazor para inyectar cascada de estado de autenticación a los componentes UI. |

---

## 2. Flujo de Control e Intercepción en Capas

```
   Cliente HTTP / Blazor
          │  Petición con header: Authorization: Bearer <token>
          ▼
   ┌────────────────────────────────────────────────────────────┐
   │ 1. Middleware de Autenticación (JwtBearer)                 │
   │    • ¿Firma criptográfica válida?                          │
   │    • ¿No ha expirado?                                      │
   │    NO ──► Retorna HTTP 401 Unauthorized                    │
   │    SÍ ──► Extrae Claims (Email, Id, Roles)                 │
   └─────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
   ┌────────────────────────────────────────────────────────────┐
   │ 2. Middleware de Autorización RBAC                         │
   │    • Extrae la ruta solicitada (ej: /api/factura)          │
   │    • Llama a RepositorioAutorizacion:                      │
   │         EXEC verificar_acceso_ruta @email, @ruta           │
   │    NO ──► Retorna HTTP 403 Forbidden                       │
   │    SÍ ──► Pasa la petición al Controlador                   │
   └─────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
   ┌────────────────────────────────────────────────────────────┐
   │ 3. Controladores de Negocio (v1 y v2 intactos)             │
   │    FacturaController, ClienteController, Producto, etc.    │
   └────────────────────────────────────────────────────────────┘
```

---

## 3. Nuevos Archivos y Modificaciones en la API (`api_facturas/`)

### 3.1 Modelos y DTOs de Autenticación
```text
Modelos/
└── CredencialUsuario.cs             # Datos básicos de sesión
Peticiones/
├── LoginPeticion.cs                 # Email y contraseña enviados desde el cliente
└── LoginRespuesta.cs                # Token JWT, vigencia y datos del usuario
```

### 3.2 Capa de Acceso a Datos y Repositorios
```text
Repositorios/
├── IRepositorioSeguridad.cs         # Interfaz para consultar usuario por email y validar ruta
└── RepositorioSeguridadSqlServer.cs # Ejecuta verificar_acceso_ruta y SELECT a usuario con Dapper
```

### 3.3 Capa de Dominio y Servicios
```text
Servicios/
├── IServicioCriptografia.cs         # Generación de Salt y verificación de Hash
├── ServicioCriptografia.cs          # Implementación PBKDF2 / SHA-256
├── IServicioToken.cs                # Creación y firma del JWT
├── ServicioToken.cs                 # Generador con SymmetricSecurityKey
├── IServicioSeguridad.cs            # Lógica de login y validación de permisos
└── ServicioSeguridad.cs             # Orquesta repositorio y criptografía
```

### 3.4 Controladores y Middlewares
```text
Controllers/
└── AuthController.cs                # Endpoints: POST /api/auth/login, GET /api/auth/perfil
Middlewares/
└── MiddlewareAutorizacionRbac.cs    # Interceptor de permisos de ruta contra la base
```

---

## 4. Nuevos Archivos y Modificaciones en el Frontend (`front_blazor/`)

### 4.1 Estado de Sesión y Servicios HTTP
```text
Modelos/
├── SesionUsuario.cs                 # Objeto que almacena token y claims en memoria del front
└── LoginModelo.cs                   # Modelo enlazado al formulario de login
Servicios/
├── ServicioAutenticacionFront.cs    # Llama a /api/auth/login y almacena el token
├── ProveedorAutenticacionCustom.cs  # Extiende AuthenticationStateProvider de Blazor
└── ManejadorTokenDelegado.cs        # DelegatingHandler que inyecta Authorization: Bearer a HttpClient
```

### 4.2 Componentes de Interfaz
```text
Components/Pages/
└── Login.razor                      # Vista de inicio de sesión con validaciones
Components/Layout/
└── NavMenu.razor                    # Ocultamiento de ítems según roles mediante <AuthorizeView>
Components/
└── Routes.razor                     # Envoltura con <AuthorizeRouteView> para protección de rutas
```

---

## 5. Regla de Oro: La Inmutabilidad de las Capas Previas

En la v3:
- **Los controladores de negocio no se tocan:** `ClienteController`, `FacturaController`, `ProductoController` siguen recibiendo y devolviendo lo mismo.
- **Los servicios de negocio no saben de JWT:** `ServicioFactura` sigue creando facturas con SPs; no se le inyectan tokens ni claims.
- **La seguridad es perimetral:** Vive en el middleware y en el controlador de autenticación dedicado.

