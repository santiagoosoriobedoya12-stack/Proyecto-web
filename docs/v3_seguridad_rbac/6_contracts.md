# Contratos HTTP — Versión 3 (JWT, BCrypt y RBAC)

Implementado en [`controlador_v3.py`](../../api_mapa/controladores/controlador_v3.py) y [`autorizacion/dependencias.py`](../../api_mapa/autorizacion/dependencias.py).

## Autenticación
- `POST /api/login` (alias `/api/sesion`): recibe `{"usuario" | "email", "password"}`. Valida BCrypt y devuelve `{"token", "tipo": "Bearer", "expira_en_segundos", "username", "email", "roles"}` (`200 OK`) o `401 Unauthorized`.
- `GET /api/permisos/mios`: requiere `Authorization: Bearer <jwt>`; retorna `{"usuario", "roles", "puede_escribir", "es_admin"}`.

## Matriz de Autorización por Rol (RBAC)

| Operación / Recurso | Sin JWT | `Consulta` | `Investigador` | `Administrador` |
| :--- | :---: | :---: | :---: | :---: |
| `GET` en v1 y v2 | `401` | `200` / `204` | `200` / `204` | `200` / `204` |
| `POST`, `PUT`, `PATCH`, `DELETE` en v1 y v2 | `401` | `403` | `200` / `201` / `204` | `200` / `201` / `204` |
| `/api/rol`, `/api/usuario`, `/api/rol-usuario` | `401` | `403` | `403` | Acceso completo |

En el Frontend Flask (`front_flask`), el JWT se almacena en memoria del servidor (`SESIONES`) y solo viaja la cookie `HttpOnly` `mapa_sid`.
