# Quickstart — Versión 1: arranque y smoke test (API e interfaz gráfica en Python)

> **Versión 1** · Validación rápida de la versión ya construida. Si aún no
> hay nada construido, empiece por [8_tasks.md](8_tasks.md).

---

## 1. Arranque (un solo comando)

```powershell
docker compose up -d --build
```

La primera vez descarga imágenes, instala `requirements.txt` en Python (`api_python` y `front_python`) y `sqlserver-init` siembra `bdfacturas.sql` automáticamente.

## 2. Smoke test (equivale a los criterios de `2_spec.md`)

> Nota: En v3 los endpoints CRUD requieren token Bearer en la cabecera `Authorization`. En este repositorio con v3 integrada, inicia sesión primero en `POST /api/sesion` (ver [v3 Quickstart](../v3_seguridad_rbac/7_quickstart.md)).

```powershell
# 1. Diagnóstico (al editar un .py con uvicorn --reload, recarga al instante)
curl.exe http://localhost:8035/
# … y documentación interactiva Swagger en: http://localhost:8035/swagger

# 2. Listar productos (con ?limite=3)
curl.exe http://localhost:8035/api/producto
curl.exe "http://localhost:8035/api/producto?limite=3"

# 3. Obtener por código (200 si existe, 404 si no)
curl.exe http://localhost:8035/api/producto/PR001
curl.exe -i http://localhost:8035/api/producto/PR999

# 4. Ciclo completo de los 5 verbos
curl.exe -X POST http://localhost:8035/api/producto -H "Content-Type: application/json" -d "{\"codigo\":\"PR009\",\"nombre\":\"Webcam\",\"stock\":10,\"valorunitario\":350000}"
curl.exe -X PUT http://localhost:8035/api/producto/PR009 -H "Content-Type: application/json" -d "{\"nombre\":\"Webcam HD\",\"stock\":12,\"valorunitario\":380000}"
curl.exe -X PATCH http://localhost:8035/api/producto/PR009 -H "Content-Type: application/json" -d "{\"stock\":99}"
curl.exe http://localhost:8035/api/producto/PR009
curl.exe -X DELETE http://localhost:8035/api/producto/PR009
curl.exe -i -X DELETE http://localhost:8035/api/producto/PR009        # → 404

# 5. Validación Pydantic (HTTP 422 antes de tocar la BD)
curl.exe -X POST http://localhost:8035/api/producto -H "Content-Type: application/json" -d "{\"codigo\":\"PRX\",\"nombre\":\"X\",\"stock\":-5,\"valorunitario\":10}"

# 6. Prueba unitaria de capas sin SQL Server
docker compose exec api-facturas python pruebas/test_capas.py
```

## 2bis. La interfaz gráfica (`front_python/`)

| | Qué hacer | Qué tiene que pasar |
|---|---|---|
| **7** | Abrir `http://localhost:8099/productos` | Lista los productos existentes con código, nombre, stock y valor |
| **8** | Crear uno con código duplicado | Muestra alerta HTTP 409 en la interfaz |
| **9** | Editar con PUT vs PATCH | «Guardar reemplazo (PUT)» y «Actualizar parcial (PATCH)» ejecutan cada endpoint correctamente |
| **10** | Apagar la API (`docker compose stop api-facturas`) | Al recargar `/productos`, el menú sigue visible, muestra alerta de conexión y 0 filas sin caerse |

Para volver a levantar la API:
```powershell
docker compose start api-facturas
```

## 3. Si algo falla

| Síntoma | Causa probable |
|---|---|
| `curl` no conecta al 8035 | El contenedor aún está instalando o iniciando (`docker compose logs api-facturas`) |
| La API responde 500 en todo | La BD no se sembró — reset: `docker compose down -v && docker compose up -d --build` |
| Motor nunca queda healthy | Puerto 11463 ocupado o Docker sin espacio |
| Guardar un `.py` no recarga | Reiniciar con `docker compose restart api-facturas` |
