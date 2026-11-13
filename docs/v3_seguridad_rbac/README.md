# Versión 3 — Control de Acceso (Seguridad, JWT y RBAC)

> **Módulo:** Identidad, Autenticación, Cifrado y Autorización RBAC en Python  
> **Alcance:** Las 3 tablas de seguridad (`rol`, `usuario`, `rol_usuario`), login con JWT, hash BCrypt y frontera 401 vs 403.  
> **Patrón:** Cierre del sistema ($v1 \subset v2 \subset v3$) protegiendo transversalmente todos los endpoints anteriores y aislando las sesiones en el servidor Flask con cookies `HttpOnly`.

---

## 1. Resumen Ejecutivo de la Versión 3

En la **Versión 3**, se incorpora la capa de seguridad empresarial completa:

1. **Hashing Criptográfico de Contraseñas:** `bcrypt` con 12 rondas de costo para almacenamiento seguro en `usuario.password_hash`.
2. **Autenticación con JSON Web Tokens (JWT):** Endpoint `POST /api/login` que valida credenciales y firma un token `HS256` con claims y expiración.
3. **Control de Acceso Basado en Roles (RBAC):**
   - `Administrador`: Acceso total a las 21 tablas y administración de usuarios/roles.
   - `Investigador`: CRUD sobre proyectos, docentes, productos y alianzas (v1 y v2); 403 en administración.
   - `Consulta`: Solo lectura (`GET`) en v1 y v2; 403 al intentar escribir o administrar.
4. **Frontera de Códigos HTTP:**
   - `401 Unauthorized`: No hay sesión o el token es inválido.
   - `403 Forbidden`: Token válido pero el rol no tiene permisos para esa acción.
5. **Sesiones Seguras en Flask:** El JWT se conserva en memoria del servidor (`SESIONES`) detrás de una cookie `HttpOnly` (`mapa_sid`), blindando el cliente contra ataques XSS.
6. **Semilla Idempotente:** Inicialización automática de `admin`, `investigador` y `consulta` en `api_mapa/semilla.py`.

---

## 2. Índice Completo del Spec Kit — Versión 3

| Documento | Descripción |
|---|---|
| [2_spec.md](2_spec.md) | Especificación formal: alcance, requisitos funcionales de seguridad, matriz de roles y criterios de aceptación. |
| [3_plan.md](3_plan.md) | Plan técnico: dependencias RBAC, manejo de JWT en FastAPI y arquitectura de sesiones en Flask. |
| [4_research.md](4_research.md) | Investigación técnica: vulnerabilidad de `localStorage` vs cookies `HttpOnly`, y verificación de roles en tiempo real. |
| [5_data_model.md](5_data_model.md) | Modelo relacional de las 3 tablas de seguridad (`rol`, `usuario`, `rol_usuario`) y usuarios semilla. |
| [6_contracts.md](6_contracts.md) | Especificación de contratos REST OpenAPI de `/api/login`, `/api/permisos/mios`, `/api/usuario`, `/api/rol`. |
| [7_quickstart.md](7_quickstart.md) | Guía de prueba rápida para validar login, 401, 403 y los tres perfiles de usuario. |
| [8_tasks.md](8_tasks.md) | Checklist de tareas de construcción de v3 completadas al 100%. |
| [prompts.md](prompts.md) | Bitácora de prompts de IA alineados con cada documento del Spec Kit y corte vertical de v3. |

---

## 3. Archivos del Código Fuente
- Repositorios: [`api_mapa/repositorios/repositorio_v3.py`](../../api_mapa/repositorios/repositorio_v3.py)
- Servicios: [`api_mapa/servicios/servicio_v3.py`](../../api_mapa/servicios/servicio_v3.py)
- Controladores: [`api_mapa/controladores/controlador_v3.py`](../../api_mapa/controladores/controlador_v3.py)
- Dependencias RBAC: [`api_mapa/autorizacion/dependencias.py`](../../api_mapa/autorizacion/dependencias.py)
- Semilla BCrypt: [`api_mapa/semilla.py`](../../api_mapa/semilla.py)
- Frontend Flask: [`front_flask/app.py`](../../front_flask/app.py) + [`templates/sesion.html`](../../front_flask/templates/sesion.html)
