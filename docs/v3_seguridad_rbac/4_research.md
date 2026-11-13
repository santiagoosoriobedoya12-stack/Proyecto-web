# Investigación Técnica — Versión 3

1. **Almacenamiento de Tokens JWT: `localStorage` vs Cookie `HttpOnly`:**
   - Si un frontend almacena el JWT en `localStorage`, cualquier script inyectado (XSS) puede robar el token y suplantar al usuario.
   - Guardar el JWT en memoria del servidor web Flask y emitir una cookie `HttpOnly` impide que JavaScript acceda a la credencial, protegiendo la sesión del usuario.
2. **Validación de Roles en Tiempo Real:**
   - La inclusión de roles en el token JWT es estándar, pero si se revoca un rol, el usuario conserva el acceso hasta que el token expire.
   - En este proyecto, `exige_usuario_autenticado` usa el `usuario_id` del token para consultar los roles activos en la base de datos en cada petición, combinando la eficiencia de JWT con la inmediatez del control de acceso centralizado.
3. **Costo de Hash BCrypt:**
   - Se utiliza factor de trabajo 12 (`rounds=12`), balanceando tiempo de cálculo defensivo (~200ms) y rendimiento del servidor.
