# Contratos HTTP — Versión 4: Reportes, Consultas Multitabla y Dashboard

> | | |
> |---|---|
> | **La ruta** | [Mapa de versiones](../../../README.md) |
> | **Lo anterior** | [v1](../v1_sin_fk/6_contracts.md) · [v2](../v2_con_fk/6_contracts.md) · [v3](../v3_control_acceso/6_contracts.md) — **siguen vigentes detrás del token** |
> | **La API** | `http://localhost:8035` · Swagger en `/swagger` |
> | **La interfaz gráfica** | `http://localhost:8099` |

**Aquí está solo lo NUEVO de la v4.** Los contratos CRUD de las 12 tablas y la autenticación de `/api/sesion` y `/api/permisos/mios` continúan intactos.

---

## A. Política de Seguridad Transversal en v4

Todos los endpoints analíticos de la v4 son operaciones de lectura (`GET`) y exigen autenticación estricta mediante JWT y verificación RBAC contra `interfaz.inicio`:

```http
Authorization: Bearer <jwt-token>
```

| Código | Condición | Cuerpo de Respuesta |
|---|---|---|
| **200 OK** | Petición autenticada con token válido y rol autorizado. | Arreglo o DTO JSON tipado |
| **401 Unauthorized** | Token ausente, alterado o vencido (`OnChallenge`). | `{"estado": 401, "mensaje": "No hay una sesion valida. Inicie sesion."}` |
| **403 Forbidden** | Token válido cuyo rol no tiene permiso sobre la ruta (`ExigePermiso`). | `{"estado": 403, "mensaje": "Su rol no tiene permiso para esta operacion.", "ruta": "interfaz.inicio"}` |

---

## B. Contratos de las 10 Consultas Multitabla (4 a 6 tablas)

### `GET /api/consultas/c1` — Consolidado Maestro de Facturación (6 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 1,
    "fecha": "2026-11-04T10:30:00",
    "total": 520000.00,
    "estado": "activa",
    "clienteNombre": "Ana Torres",
    "clienteTipo": "Persona y Empresa",
    "clienteContacto": "ana.torres@correo.com",
    "vendedorNombre": "Carlos Pérez",
    "vendedorCarnet": 1001
  }
]
```

### `GET /api/consultas/c2` — Ranking de Productos más Vendidos (4 tablas)
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

### `GET /api/consultas/c3` — Desempeño Comercial por Vendedor (5 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "idVendedor": 1,
    "carnet": 1001,
    "nombreVendedor": "Carlos Pérez",
    "emailVendedor": "carlos.perez@correo.com",
    "totalFacturas": 12,
    "facturasActivas": 11,
    "facturasAnuladas": 1,
    "montoTotalVendido": 85000000.00,
    "ticketPromedio": 7727272.73
  }
]
```

### `GET /api/consultas/c4` — Clientes de Alto Valor (5 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 1,
    "nombreTitular": "Ana Torres",
    "empresaAsociada": "Comercial Los Andes S.A.",
    "creditoAsignado": 520000.00,
    "totalFacturas": 6,
    "montoTotalComprado": 4200000.00,
    "variedadProductosComprados": 9,
    "fechaUltimaCompra": "2026-11-02T16:45:00"
  }
]
```

### `GET /api/consultas/c5` — Alerta de Stock Crítico (4 tablas)
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

### `GET /api/consultas/c6` — Auditoría de Seguridad y Roles (5 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "emailUsuario": "admin@correo.com",
    "rolesAsignados": "Administrador",
    "cantidadRoles": 1,
    "cantidadRutasPermitidas": 15,
    "rutasDetalle": "interfaz.inicio | interfaz.facturas | interfaz.productos"
  }
]
```

### `GET /api/consultas/c7` — Renglones Detallados de Factura (5 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 1,
    "fechaFactura": "2026-11-04T10:30:00",
    "clienteNombre": "Ana Torres",
    "codigoProducto": "PROD01",
    "nombreProducto": "Portátil ThinkPad",
    "cantidad": 2,
    "precioUnitarioCalculado": 3500000.00,
    "subtotalRenglon": 7000000.00,
    "totalFactura": 7000000.00
  }
]
```

### `GET /api/consultas/c8` — Línea de Crédito y Cartera (4 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 1,
    "personaNombre": "Ana Torres",
    "empresaNombre": "Comercial Los Andes S.A.",
    "limiteCredito": 520000.00,
    "totalFacturadoActivo": 450000.00,
    "creditoDisponible": 70000.00,
    "estadoCredito": "RIESGO ALTO"
  }
]
```

### `GET /api/consultas/c9` — Clientes Inactivos (4 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "idCliente": 5,
    "nombrePersona": "Pedro Castillo",
    "emailContacto": "pedro.castillo@correo.com",
    "telefonoContacto": "3066666666",
    "nombreEmpresa": "Comercial Los Andes S.A.",
    "credito": 700000.00,
    "facturasAsociadas": 0,
    "estadoActividad": "Sin compras activas registradas"
  }
]
```

### `GET /api/consultas/c10` — Auditoría de Facturas Anuladas (5 tablas)
- **Respuesta 200 OK:**
```json
[
  {
    "numeroFactura": 3,
    "fechaFactura": "2026-10-28T09:10:00",
    "vendedorResponsable": "Carlos Pérez",
    "carnetVendedor": 1001,
    "idClienteAfectado": 2,
    "montoAnulado": 1250000.00,
    "cantidadRenglonesRevertidos": 2
  }
]
```

---

## C. Contrato del Dashboard de KPIs

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

---

## D. Diagnóstico de la API (`GET /`)

```json
GET /  ->  200 {
  "mensaje": "API Facturas funcionando",
  "version": "v4",
  "contratos": "docs/spec_kit/versiones/v4_reportes_dashboard/6_contracts.md"
}
```
