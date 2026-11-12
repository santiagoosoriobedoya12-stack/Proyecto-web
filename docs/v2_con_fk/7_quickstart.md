# Guía Rápida — Versión 2

1. Abre **Docentes** (`http://localhost:8116/docentes`) y crea o edita un docente seleccionando una `linea_investigacion_principal` existente.
2. Abre **Productos** (`/productos`) y registra un producto enlazado a un `proyecto` y a un `tipo_producto`.
3. Abre **Participación Docente–Proyecto** (`/desarrolla`) para asignar un `rol` y `descripcion`, editar la participación o retirarla lógicamente.
4. Prueba los vínculos M:N en cualquiera de las 7 vistas puente (por ejemplo `/ods-proyecto` o `/palabras-clave`):
   - Asociar dos registros activos (`POST`).
   - Intentar asociar el mismo par dos veces -> la API responde **HTTP `409 Conflict`** y el formulario muestra el aviso sin romperse.
   - Si en API (`Swagger`) envías un ID de proyecto retirado lógicamente (`activo = 0`) o inexistente, `ServicioProducto` / `ServicioDocente` / `ServicioPuente` responde **HTTP `409 Conflict`**.
