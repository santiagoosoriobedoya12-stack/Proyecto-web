# Lista de Tareas (Tasks) — Versión 3

- [x] **T3.1:** Crear tablas `rol`, `usuario` y `rol_usuario` en `db/init.sql`.
- [x] **T3.2:** Implementar semilla automática idempotente con BCrypt en `api_mapa/semilla.py`.
- [x] **T3.3:** Crear DTOs de seguridad `LoginPeticion`, `UsuarioCrear`, `RolCrear`, etc., en `api_mapa/peticiones/esquemas.py`.
- [x] **T3.4:** Crear interfaces `IRepositorioRol`, `IRepositorioUsuario`, `IRepositorioRolUsuario`, `IRepositorioAcceso` en `api_mapa/repositorios/interfaces.py`.
- [x] **T3.5:** Implementar repositorios MariaDB en `api_mapa/repositorios/repositorio_v3.py`.
- [x] **T3.6:** Implementar `ServicioRol`, `ServicioUsuario`, `ServicioRolUsuario` y `ServicioLogin` con emisión de JWT (`HS256`) en `api_mapa/servicios/servicio_v3.py`.
- [x] **T3.7:** Implementar dependencias RBAC `exige_usuario_autenticado` y `exige_roles` en `api_mapa/autorizacion/dependencias.py`.
- [x] **T3.8:** Crear routers `/api/login`, `/api/rol`, `/api/usuario`, `/api/rol-usuario` en `api_mapa/controladores/controlador_v3.py`.
- [x] **T3.9:** Proteger routers de v1 y v2 con dependencias RBAC (`_SOLO_LECTURA` y `_ESCRITURA`).
- [x] **T3.10:** Implementar sesiones en servidor Flask con cookie `HttpOnly` `mapa_sid` y protección de vistas en `front_flask/app.py`.
- [x] **T3.11:** Escribir pruebas unitarias de autenticación, hashing BCrypt y roles en `api_mapa/pruebas/test_capas.py`.
