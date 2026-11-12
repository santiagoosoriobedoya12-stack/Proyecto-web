# Guía Rápida — Versión 3 (JWT + RBAC)

1. Verifica `401 Unauthorized`: sin token JWT, cualquier petición en Swagger o `curl` a `GET /api/proyecto` devuelve `401`.
2. Inicia sesión en `http://localhost:8116/sesion` con los 3 usuarios semilla:
   - **`consulta` / `consulta123` (`Consulta`):** ve las tablas `v1` y `v2` en modo solo lectura (botones de crear/editar/retirar ocultos en UI; en API cualquier `POST`/`PUT`/`PATCH`/`DELETE` devuelve `403 Forbidden`).
   - **`investigador` / `inv123` (`Investigador`):** CRUD completo sobre `v1` y `v2`, pero recibe `403 Forbidden` en `/api/usuario`, `/api/rol` y `/api/rol-usuario`.
   - **`admin` / `admin123` (`Administrador`):** acceso total a las 21 tablas, incluyendo `/usuarios`, `/roles` y `/roles-usuario`.
3. Prueba crear un usuario nuevo en `/usuarios` y asignarle rol en `/roles-usuario`; luego inicia sesión con él en `/sesion`.
