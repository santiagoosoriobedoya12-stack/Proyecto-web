# Guía Rápida — Versión 1

1. Levanta el entorno e inicia sesión como `admin` (`admin123`) o `investigador` (`inv123`).
2. En el Frontend Flask (`http://localhost:8116`), abre **Proyectos** (`/proyectos`), **Líneas de Investigación** (`/lineas-investigacion`), **ODS** (`/ods`), etc.
3. Prueba el ciclo completo sobre cualquier catálogo `v1`:
   - Crear un registro (`POST`).
   - Editar con **Guardar la ficha completa** (`PUT` idempotente gracias a `CLIENT.FOUND_ROWS`).
   - Editar un solo campo con **Guardar solo lo que cambié** (`PATCH`).
   - Pulsar **Retirar** (`DELETE` lógico: `activo = 0`) y confirmar que desaparece de la lista y que repetir `DELETE` en Swagger devuelve `404`.
4. Ejecuta las pruebas de capas sin BD:
   ```powershell
   docker compose exec api-mapa pytest pruebas/test_capas.py -v
   ```
