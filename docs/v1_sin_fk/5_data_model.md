# Modelo de Datos — Versión 1 (8 Tablas sin Clave Foránea)

> Nota metodológica (`0_METODOLOGIA.md` §7.0): El script SQL original del módulo incluía `activo` únicamente en `rol` y `usuario`. Para garantizar **borrado lógico obligatorio en todo el módulo**, [`db/init.sql`](../../db/init.sql) agrega `activo TINYINT(1) NOT NULL DEFAULT 1` a las 8 tablas de `v1` (y a las 10 de `v2`). Todas las consultas `SELECT` filtran `activo = 1` sin exponer esa columna técnica en las respuestas JSON ni formularios.

| Tabla | Clave Primaria | Auto-Inc | Columnas de Negocio |
| :--- | :--- | :---: | :--- |
| `area_conocimiento` | `id INT` | No | `gran_area`, `area`, `disciplina` |
| `objetivo_desarrollo_sostenible` | `id INT` | No | `nombre`, `categoria` (17 ODS precargados) |
| `area_aplicacion` | `id INT` | No | `nombre` |
| `termino_clave` | `termino VARCHAR(30)` | No | `termino_ingles` (nullable) |
| `linea_investigacion` | `id INT` | Sí | `nombre`, `descripcion` |
| `aliado` | `nit INT` | No | `razon_social`, `nombre_contacto`, `correo`, `telefono`, `ciudad` |
| `tipo_producto` | `id INT` | No | `categoria`, `clase`, `nombre`, `tipologia` |
| `proyecto` | `id INT` | No | `titulo`, `resumen`, `presupuesto DECIMAL(15,2)`, `tipo_financiacion`, `tipo_fondos`, `fecha_inicio`, `fecha_fin` |
