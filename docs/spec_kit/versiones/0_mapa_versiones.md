# Mapa de Versiones (SDD) — Mapa de Conocimiento

| Versión | Nombre | Tablas incluidas | Enlace Spec | Guía de Versión |
| :---: | :--- | :--- | :---: | :---: |
| **v1** | Tablas sin clave foránea | `area_conocimiento`, `objetivo_desarrollo_sostenible`, `area_aplicacion`, `termino_clave`, `linea_investigacion`, `aliado`, `tipo_producto`, `proyecto` (8) | [Contratos v1](v1_sin_fk/6_contracts.md) | [Docs v1](../../v1_sin_fk/README.md) |
| **v2** | Tablas con clave foránea y M:N | `docente`, `producto`, `desarrolla`, `aliado_proyecto`, `palabras_clave`, `ac_proyecto`, `proyecto_linea`, `ods_proyecto`, `aa_proyecto`, `docente_producto` (10) | [Contratos v2](v2_con_fk/6_contracts.md) | [Docs v2](../../v2_con_fk/README.md) |
| **v3** | Seguridad JWT, BCrypt y RBAC | `rol`, `usuario`, `rol_usuario` (3) + endpoints `/api/login`, `/api/permisos/mios` | [Contratos v3](v3_control_acceso/6_contracts.md) | [Docs v3](../../v3_seguridad_rbac/README.md) |
