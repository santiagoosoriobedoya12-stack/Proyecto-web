# Modelo de Datos — Versión 3: Tablas de Seguridad

Esquema de las 3 tablas de control de acceso en MariaDB (`db/init.sql`):

| Tabla | Clave Primaria | Auto-Inc | Columnas de Seguridad |
| :--- | :--- | :---: | :--- |
| `rol` | `id INT` | Sí | `nombre VARCHAR(100) NOT NULL`, `descripcion VARCHAR(255)`, `activo TINYINT(1) DEFAULT 1` |
| `usuario` | `id INT` | Sí | `username VARCHAR(100) UNIQUE`, `password_hash VARCHAR(255) NOT NULL`, `email VARCHAR(150) UNIQUE`, `nombre_completo VARCHAR(200)`, `activo TINYINT(1) DEFAULT 1` |
| `rol_usuario` | `(usuario_id, rol_id)` | No | PK compuesta, FK a `usuario(id)` y FK a `rol(id)` con `ON DELETE CASCADE` |

## Usuarios Semilla Oficiales

| Usuario | Contraseña en Claro | Hash Almacenado | Rol |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | BCrypt ($2b$12$...) | `Administrador` |
| `investigador` | `inv123` | BCrypt ($2b$12$...) | `Investigador` |
| `consulta` | `consulta123` | BCrypt ($2b$12$...) | `Consulta` |
