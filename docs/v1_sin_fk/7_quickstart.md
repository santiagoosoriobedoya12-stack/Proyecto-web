# Quickstart ÔÇö Versi├│n 1: arranque y smoke test (API y interfaz gr├ífica)

> **Versi├│n 1** ┬À Validaci├│n r├ípida de la versi├│n ya construida. Si a├║n no
> hay nada construido, empiece por [8_tasks.md](8_tasks.md).

---

## 1. Arranque (un solo comando)

```powershell
docker compose up -d --build
```

La primera vez tarda: descarga im├ígenes, restaura paquetes y la BD se siembra sola. Al final: base de datos (healthy) y `api-facturas` arriba. La primera compilaci├│n de `dotnet watch` toma ~30-60 segundos.

## 2. Smoke test (equivale a los 10 criterios de 2_spec.md)

```powershell
# 1. Diagn├│stico (y de paso: edite un .cs, guarde ÔÇö recompila solo)
curl.exe http://localhost:8032/
# ÔÇª y la documentaci├│n interactiva en el navegador: http://localhost:8032/swagger

# 2. Listar: 8 productos; con limite=3, exactamente 3
curl.exe http://localhost:8032/api/producto
curl.exe "http://localhost:8032/api/producto?limite=3"

# 3. Obtener: 200 con la Laptop; 404 con PR999
curl.exe http://localhost:8032/api/producto/PR001
curl.exe -i http://localhost:8032/api/producto/PR999

# 4. El ciclo de los 5 verbos
curl.exe -X POST http://localhost:8032/api/producto -H "Content-Type: application/json" -d "{\"codigo\":\"PR009\",\"nombre\":\"Webcam\",\"stock\":10,\"valorunitario\":350000}"
curl.exe -X PUT http://localhost:8032/api/producto/PR009 -H "Content-Type: application/json" -d "{\"nombre\":\"Webcam HD\",\"stock\":12,\"valorunitario\":380000}"
curl.exe -X PATCH http://localhost:8032/api/producto/PR009 -H "Content-Type: application/json" -d "{\"stock\":99}"
curl.exe http://localhost:8032/api/producto/PR009
curl.exe -X DELETE http://localhost:8032/api/producto/PR009
curl.exe -i -X DELETE http://localhost:8032/api/producto/PR009        # ÔåÆ 404

# 4b. El contraste did├íctico: MISMO body, dos verbos
curl.exe -i -X PUT http://localhost:8032/api/producto/PR001 -H "Content-Type: application/json" -d "{\"stock\":99}"     # ÔåÆ 422
curl.exe -i -X PATCH http://localhost:8032/api/producto/PR001 -H "Content-Type: application/json" -d "{\"stock\":17}"   # ÔåÆ 200

# 5. La frontera de la petici├│n ÔÇö nunca llega a la BD
curl.exe -X POST http://localhost:8032/api/producto -H "Content-Type: application/json" -d "{\"codigo\":\"PRX\",\"nombre\":\"X\",\"stock\":-5,\"valorunitario\":10}"      # ÔåÆ 422 con errores[]
curl.exe -i -X POST http://localhost:8032/api/producto -H "Content-Type: application/json" -d "{\"codigo\":\"PRY\",\"nombre\":\"Y\",\"stock\":7.5,\"valorunitario\":10}"  # ÔåÆ 422 (el tipo es regla)

# 6. La prueba de capas (sin SQL Server)
docker compose exec api-facturas dotnet run --project pruebas
# ÔåÆ CRITERIO 6 OK: el servicio funciona con el repositorio falso, sin SQL Server
```

## 2bis. La interfaz gr├ífica ÔÇö criterios 7 a 10

| | Qu├® hacer | Qu├® tiene que pasar |
|---|---|---|
| **7** | Abrir `http://localhost:8096/productos` | Lista los **8 productos**, con c├│digo, nombre, stock y valor |
| **8** | Crear uno con un c├│digo que **ya existe** | El mensaje sale **en la interfaz gr├ífica**, y **lo que escribi├│ NO se borra** |
| **9** | Editar uno y usar los **dos** botones de guardar | ┬½Guardar la ficha completa┬╗ (`PUT`) y ┬½Guardar solo lo que cambi├®┬╗ (`PATCH`) hacen cosas distintas. La interfaz gr├ífica **no dice `PUT` ni `422`** |
| **10** | Apagar la API y recargar | Ver abajo |

### El criterio 10 (prueba de resiliencia del frontend)

```powershell
docker compose stop api-facturas
```

Recargue `http://localhost:8096/productos`: debe mostrar el men├║, un aviso de que no se pudo conectar, y ninguna fila en la tabla.

Para volver a levantar la API:
```powershell
docker compose start api-facturas
```

## 3. Si algo falla

| S├¡ntoma | Causa probable |
|---|---|
| `curl` no conecta al 8032 | La compilaci├│n inicial a├║n no termina (`docker compose logs api-facturas`) |
| La API responde 500 en todo | La BD no se sembr├│ ÔÇö reset: `docker compose down -v && docker compose up -d` |
| Motor nunca queda healthy | Puerto 11463 ocupado o Docker sin espacio |
| Guardar un `.cs` no recarga | Reintentar o reiniciar con `docker compose restart api-facturas` |
