# Contratos OpenAPI — Versión 4: Reportes, Consultas Multitabla y Dashboard

> | | |
> |---|---|
> | **La especificación** | [2_spec.md](2_spec.md) |
> | **El plan técnico** | [3_plan.md](3_plan.md) |
> | **El modelo de datos** | [5_data_model.md](5_data_model.md) |

---

## 1. Política de Seguridad Transversal en v4

Todos los endpoints analíticos de la v4 son operaciones de lectura (`GET`) y exigen autenticación estricta:

```http
Authorization: Bearer <jwt-token>
```

| Código | Condición |
|---|---|
| **200 OK** | Petición autenticada con token válido y rol autorizado. |
| **401 Unauthorized** | Token ausente, alterado o vencido (`{"error":"Token ausente o inválido"}`). |
| **403 Forbidden** | Usuario autenticado cuyo rol no tiene asignado el permiso de la ruta. |

---

## 2. Contratos de las 10 Consultas Multitabla

### `GET /api/consultas/c1` — Consolidado Maestro de Facturación
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 1,
    "fecha": "2026-11-04T10:30:00",
    "total": 520000.00,
    "estado": "activa",
    "clienteNombre": "Carlos Pérez",
    "clienteTipo": "Persona Natural",
    "clienteContacto": "carlos.perez@email.com",
    "vendedorNombre": "Ana Gómez",
    "vendedorCarnet": 1001
  }
]
```

### `GET /api/consultas/c2` — Ranking de Productos más Vendidos
- **Respuesta 200 OK:**
```json
[
  {
    "codigoProducto": "PROD01",
    "nombreProducto": "Portátil ThinkPad",
    "valorUnitario": 3500000.00,
    "stockActual": 18,
    "unidadesVendidas": 15,
    "totalRecaudado": 52500000.00,
    "totalFacturasPresente": 8
  }
]
```

### `GET /api/consultas/c3` — Desempeño Comercial por Vendedor
- **Respuesta 200 OK:**
```json
[
  {
    "idVendedor": 1,
    "carnet": 1001,
    "nombreVendedor": "Ana Gómez",
    "emailVendedor": "ana.gomez@empresa.com",
    "totalFacturas": 12,
    "facturasActivas": 11,
    "facturasAnuladas": 1,
    "montoTotalVendido": 85000000.00,
    "ticketPromedio": 7727272.73
  }
]
```

### `GET /api/consultas/c4` — Clientes de Alto Valor
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 2,
    "nombreTitular": "Tecnología Global S.A.",
    "empresaAsociada": "Tecnología Global S.A.",
    "creditoAsignado": 50000000.00,
    "totalFacturas": 6,
    "montoTotalComprado": 42000000.00,
    "variedadProductosComprados": 9,
    "fechaUltimaCompra": "2026-11-02T16:45:00"
  }
]
```

### `GET /api/consultas/c5` — Alerta de Stock Crítico
- **Respuesta 200 OK:**
```json
[
  {
    "codigoProducto": "PROD05",
    "nombreProducto": "Mouse Inalámbrico",
    "stockActual": 3,
    "valorUnitario": 45000.00,
    "totalVendidoHistorico": 45,
    "nivelAlerta": "CRÍTICO",
    "fechaUltimaVenta": "2026-11-03T11:15:00"
  }
]
```

### `GET /api/consultas/c6` — Auditoría de Seguridad y Roles
- **Respuesta 200 OK:**
```json
[
  {
    "emailUsuario": "admin@facturas.com",
    "rolesAsignados": "Administrador",
    "cantidadRoles": 1,
    "cantidadRutasPermitidas": 15,
    "rutasDetalle": "interfaz.inicio | interfaz.facturas | interfaz.consultas"
  }
]
```

### `GET /api/consultas/c7` — Renglones Detallados de Factura
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 1,
    "fechaFactura": "2026-11-04T10:30:00",
    "clienteNombre": "Carlos Pérez",
    "codigoProducto": "PROD01",
    "nombreProducto": "Portátil ThinkPad",
    "cantidad": 2,
    "precioUnitarioCalculado": 3500000.00,
    "subtotalRenglon": 7000000.00,
    "totalFactura": 7000000.00
  }
]
```

### `GET /api/consultas/c8` — Línea de Crédito y Cartera
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 1,
    "personaNombre": "Carlos Pérez",
    "empresaNombre": "Particular",
    "limiteCredito": 10000000.00,
    "totalFacturadoActivo": 8500000.00,
    "creditoDisponible": 1500000.00,
    "estadoCredito": "RIESGO ALTO"
  }
]
```

### `GET /api/consultas/c9` — Clientes Inactivos
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 8,
    "nombrePersona": "Lucía Morales",
    "emailContacto": "lucia.m@gmail.com",
    "telefonoContacto": "3001234567",
    "nombreEmpresa": "Particular",
    "credito": 5000000.00,
    "facturasAsociadas": 0,
    "estadoActividad": "Sin compras activas registradas"
  }
]
```

### `GET /api/consultas/c10` — Auditoría de Facturas Anuladas
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 3,
    "fechaFactura": "2026-10-28T09:10:00",
    "vendedorResponsable": "Ana Gómez",
    "carnetVendedor": 1001,
    "idClienteAfectado": 4,
    "montoAnulado": 1250000.00,
    "cantidadRenglonesRevertidos": 2
  }
]
```

---

## 3. Contrato del Dashboard de KPIs

### `GET /api/dashboard/kpis` — Métricas Ejecutivas en Tiempo Real
- **Respuesta 200 OK:**
```json
{
  "ventasTotales": 145800000.00,
  "totalFacturas": 48,
  "facturasActivas": 45,
  "facturasAnuladas": 3,
  "totalClientes": 16,
  "totalVendedores": 5,
  "totalProductos": 24,
  "productosStockCritico": 4,
  "ticketPromedio": 3240000.00
}
```

