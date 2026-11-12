# Versión 3 — Autenticación JWT, Hashing BCrypt y Control de Acceso (RBAC)

Protege de forma acumulativa todos los endpoints `v1` y `v2` e incorpora la gestión administrativa de `rol`, `usuario` y `rol_usuario`.

- **Contratos HTTP y Matriz de Roles:** [6_contracts.md](../spec_kit/versiones/v3_control_acceso/6_contracts.md)
- **Guía rápida de prueba:** [7_quickstart.md](7_quickstart.md)

## Componentes de Seguridad
- **Hash de contraseñas:** `bcrypt` en [`servicio_v3.py`](../../api_mapa/servicios/servicio_v3.py) y [`semilla.py`](../../api_mapa/semilla.py).
- **Emisión y validación JWT:** `PyJWT` (`HS256`) en [`servicio_v3.py`](../../api_mapa/servicios/servicio_v3.py) y [`autorizacion/dependencias.py`](../../api_mapa/autorizacion/dependencias.py).
- **Sesiones en Frontend Flask:** el JWT nunca viaja a cookies del navegador ni a HTML; queda almacenado en memoria de Flask bajo el ID aleatorio de cookie `HttpOnly` `mapa_sid` ([`cliente_api.py`](../../front_flask/servicios/cliente_api.py)).
