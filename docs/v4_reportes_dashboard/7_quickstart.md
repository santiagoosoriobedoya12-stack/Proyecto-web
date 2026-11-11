# Guía de Arranque y Smoke Test — Versión 4: Reportes y Dashboard

> | | |
> |---|---|
> | **La especificación** | [2_spec.md](2_spec.md) |
> | **Los contratos** | [6_contracts.md](6_contracts.md) |
> | **El modelo de datos** | [5_data_model.md](5_data_model.md) |
> | **La API** | `http://localhost:8035` · Swagger en `/swagger` |
> | **La interfaz gráfica** | `http://localhost:8099` |

---

## 1. Arranque del Entorno Completo

Levanta los contenedores oficiales con Docker Compose:

```bash
docker compose up -d --build
```

*(Nota para Windows PowerShell: usa `curl.exe` con extensión porque `curl` a secas es un alias de `Invoke-WebRequest`).*

Verifica el diagnóstico de la API:
```bash
curl.exe -s http://localhost:8035/
# Debe responder {"mensaje":"API Facturas funcionando","version":"v4","contratos":"docs/spec_kit/versiones/v4_reportes_dashboard/6_contracts.md"}
```

---

## 2. Protocolo de Pruebas Rápidas (Smoke Test v4)

### Paso 1: Autenticación y Obtención del Token JWT
Inicia sesión con el usuario administrador semilla para obtener el token de autorización:

```bash
TOKEN=$(curl.exe -s -X POST http://localhost:8035/api/sesion \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@correo.com","contrasena":"admin123"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

echo "Token obtenido: $TOKEN"
```

---

### Paso 2: Prueba de Bloqueo Sin Token (401 Unauthorized)
Intentar consultar sin encabezado de autorización debe devolver `401`:

```bash
curl.exe -s -i http://localhost:8035/api/consultas/c1 | grep "401"
```

---

### Paso 3: Smoke Test de las 10 Consultas Multitabla (con Token)

```bash
# C1 - Consolidado de Facturas (6 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c1

# C2 - Ranking de Productos más Vendidos (4 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c2

# C3 - Desempeño Comercial por Vendedor (5 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c3

# C4 - Clientes de Alto Valor (5 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c4

# C5 - Alerta de Stock Crítico (4 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c5

# C6 - Auditoría de Seguridad y Roles (5 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c6

# C7 - Renglones Detallados de Factura (5 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c7

# C8 - Línea de Crédito y Cartera (4 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c8

# C9 - Clientes Inactivos (4 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c9

# C10 - Facturas Anuladas e Impacto Financiero (5 tablas)
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/consultas/c10
```

---

### Paso 4: Smoke Test del Dashboard de KPIs

```bash
curl.exe -s -H "Authorization: Bearer $TOKEN" http://localhost:8035/api/dashboard/kpis
```

---

## 3. Verificación en la Interfaz Gráfica (Blazor)

1. Abre tu navegador en `http://localhost:8099`.
2. Inicia sesión con `admin@correo.com` / `admin123`.
3. Navega al **Dashboard** (`/dashboard`):
   - Observa las 4 tarjetas KPI con valores actualizados en vivo.
   - Observa el ranking de productos y gráficos visuales de rendimiento.
4. Navega a **Consultas** (`/consultas`):
   - Selecciona cualquiera de las 10 opciones del menú desplegable.
   - Confirma que la tabla renderiza dinámicamente las columnas y filas con formato de moneda, fechas legibles y badges de estado.
