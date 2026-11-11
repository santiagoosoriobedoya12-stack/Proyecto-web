# Modelo de Datos y Consultas — Versión 4: Reportes y Analítica

> | | |
> |---|---|
> | **La especificación** | [2_spec.md](2_spec.md) |
> | **El plan técnico** | [3_plan.md](3_plan.md) |
> | **Los contratos OpenAPI** | [6_contracts.md](6_contracts.md) |

---

## 1. Grafo de Navegación de las 12 Tablas

Las 10 consultas de la v4 navegan por el grafo relacional de `bdfacturas`, cruzando entre 4 y 6 tablas en cada operación:

```
                  ┌─────────┐
                  │ empresa │
                  └────┬────┘
                       │ (fkcodempresa)
                       ▼
 ┌─────────┐      ┌─────────┐                          ┌─────────┐
 │ persona │ ───► │ cliente │ ───────────────────────► │ factura │ ◄──┐
 └────┬────┘      └─────────┘                          └────┬────┘    │
      │                                                     │         │
      │ (fkcodpersona) ┌──────────┐                         │         │
      └──────────────► │ vendedor │ ────────────────────────┘         │
                       └──────────┘                                   │
                                                                      │
                       ┌──────────┐                                   │
                       │ producto │ ◄─────────────────────────────────┤
                       └────┬─────┘                                   │
                            │                                         │
                            ▼                                         │
               ┌─────────────────────┐                                │
               │ productosporfactura │ ───────────────────────────────┘
               └─────────────────────┘
```

---

## 2. Definición Técnica de las 10 Consultas Multitabla

### C1 — Consolidado Maestro de Facturación con Detalle de Partes
- **Tablas:** `factura (f)`, `cliente (c)`, `persona (pc)`, `empresa (ec)`, `vendedor (v)`, `persona (pv)` *(6 tablas)*.
- **SQL:**
```sql
SELECT 
    f.numero AS NumeroFactura,
    f.fecha AS Fecha,
    f.total AS Total,
    f.estado AS Estado,
    COALESCE(pc.nombre, ec.nombre, 'Sin titular') AS ClienteNombre,
    CASE 
        WHEN pc.codigo IS NOT NULL AND ec.codigo IS NOT NULL THEN 'Persona y Empresa'
        WHEN pc.codigo IS NOT NULL THEN 'Persona Natural'
        ELSE 'Empresa Jurídica'
    END AS ClienteTipo,
    COALESCE(pc.email, ec.codigo) AS ClienteContacto,
    pv.nombre AS VendedorNombre,
    v.carnet AS VendedorCarnet
FROM factura f
INNER JOIN cliente c ON f.fkidcliente = c.id
LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
INNER JOIN vendedor v ON f.fkidvendedor = v.id
INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
ORDER BY f.fecha DESC;
```

---

### C2 — Ranking de Productos más Vendidos y Rentabilidad
- **Tablas:** `producto (p)`, `productosporfactura (pf)`, `factura (f)`, `cliente (c)` *(4 tablas)*.
- **SQL:**
```sql
SELECT 
    p.codigo AS CodigoProducto,
    p.nombre AS NombreProducto,
    p.valorunitario AS ValorUnitario,
    p.stock AS StockActual,
    SUM(pf.cantidad) AS UnidadesVendidas,
    SUM(pf.subtotal) AS TotalRecaudado,
    COUNT(DISTINCT f.numero) AS TotalFacturasPresente
FROM producto p
INNER JOIN productosporfactura pf ON p.codigo = pf.fkcodproducto
INNER JOIN factura f ON pf.fknumfactura = f.numero
INNER JOIN cliente c ON f.fkidcliente = c.id
WHERE f.estado = 'activa'
GROUP BY p.codigo, p.nombre, p.valorunitario, p.stock
ORDER BY TotalRecaudado DESC;
```

---

### C3 — Desempeño Comercial y Productividad por Vendedor
- **Tablas:** `vendedor (v)`, `persona (pv)`, `factura (f)`, `productosporfactura (pf)`, `producto (p)` *(5 tablas)*.
- **SQL:**
```sql
SELECT 
    v.id AS IdVendedor,
    v.carnet AS Carnet,
    pv.nombre AS NombreVendedor,
    pv.email AS EmailVendedor,
    COUNT(DISTINCT f.numero) AS TotalFacturas,
    SUM(CASE WHEN f.estado = 'activa' THEN 1 ELSE 0 END) AS FacturasActivas,
    SUM(CASE WHEN f.estado = 'anulada' THEN 1 ELSE 0 END) AS FacturasAnuladas,
    COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS MontoTotalVendido,
    COALESCE(AVG(CASE WHEN f.estado = 'activa' THEN f.total ELSE NULL END), 0) AS TicketPromedio
FROM vendedor v
INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
LEFT JOIN factura f ON v.id = f.fkidvendedor
LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
LEFT JOIN producto p ON pf.fkcodproducto = p.codigo
GROUP BY v.id, v.carnet, pv.nombre, pv.email
ORDER BY MontoTotalVendido DESC;
```

---

### C4 — Segmentación de Clientes de Alto Valor y Fidelización
- **Tablas:** `cliente (c)`, `persona (pc)`, `empresa (ec)`, `factura (f)`, `productosporfactura (pf)` *(5 tablas)*.
- **SQL:**
```sql
SELECT 
    c.id AS IdCliente,
    COALESCE(pc.nombre, ec.nombre, 'Sin nombre') AS NombreTitular,
    COALESCE(ec.nombre, 'Particular') AS EmpresaAsociada,
    c.credito AS CreditoAsignado,
    COUNT(DISTINCT f.numero) AS TotalFacturas,
    COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS MontoTotalComprado,
    COUNT(DISTINCT pf.fkcodproducto) AS VariedadProductosComprados,
    MAX(f.fecha) AS FechaUltimaCompra
FROM cliente c
LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
INNER JOIN factura f ON c.id = f.fkidcliente
LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
WHERE f.estado = 'activa'
GROUP BY c.id, pc.nombre, ec.nombre, c.credito
ORDER BY MontoTotalComprado DESC;
```

---

### C5 — Alerta Preventiva de Stock Crítico y Rotación de Demanda
- **Tablas:** `producto (p)`, `productosporfactura (pf)`, `factura (f)`, `vendedor (v)` *(4 tablas)*.
- **SQL:**
```sql
SELECT 
    p.codigo AS CodigoProducto,
    p.nombre AS NombreProducto,
    p.stock AS StockActual,
    p.valorunitario AS ValorUnitario,
    COALESCE(SUM(pf.cantidad), 0) AS TotalVendidoHistorico,
    CASE 
        WHEN p.stock <= 5 THEN 'CRÍTICO'
        WHEN p.stock <= 15 THEN 'BAJO'
        ELSE 'ESTABLE'
    END AS NivelAlerta,
    MAX(f.fecha) AS FechaUltimaVenta
FROM producto p
LEFT JOIN productosporfactura pf ON p.codigo = pf.fkcodproducto
LEFT JOIN factura f ON pf.fknumfactura = f.numero
LEFT JOIN vendedor v ON f.fkidvendedor = v.id
GROUP BY p.codigo, p.nombre, p.stock, p.valorunitario
ORDER BY p.stock ASC, TotalVendidoHistorico DESC;
```

---

### C6 — Auditoría de Seguridad, Roles y Trazabilidad de Accesos
- **Tablas:** `usuario (u)`, `rol_usuario (ru)`, `rol (r)`, `rutarol (rr)`, `ruta (rt)` *(5 tablas)*.
- **SQL:**
```sql
SELECT 
    u.email AS EmailUsuario,
    STRING_AGG(r.nombre, ', ') AS RolesAsignados,
    COUNT(DISTINCT r.id) AS CantidadRoles,
    COUNT(DISTINCT rt.id) AS CantidadRutasPermitidas,
    STRING_AGG(rt.ruta, ' | ') AS RutasDetalle
FROM usuario u
INNER JOIN rol_usuario ru ON u.email = ru.fkemail
INNER JOIN rol r ON ru.fkidrol = r.id
LEFT JOIN rutarol rr ON r.id = rr.fkidrol
LEFT JOIN ruta rt ON rr.fkidruta = rt.id
GROUP BY u.email
ORDER BY CantidadRutasPermitidas DESC;
```

---

### C7 — Renglones Detallados de Facturación con Trazabilidad Completa
- **Tablas:** `factura (f)`, `productosporfactura (pf)`, `producto (p)`, `cliente (c)`, `persona (pc)` *(5 tablas)*.
- **SQL:**
```sql
SELECT 
    f.numero AS NumeroFactura,
    f.fecha AS FechaFactura,
    COALESCE(pc.nombre, 'Empresa Jurídica') AS ClienteNombre,
    p.codigo AS CodigoProducto,
    p.nombre AS NombreProducto,
    pf.cantidad AS Cantidad,
    pf.subtotal / NULLIF(pf.cantidad, 0) AS PrecioUnitarioCalculado,
    pf.subtotal AS SubtotalRenglon,
    f.total AS TotalFactura
FROM factura f
INNER JOIN productosporfactura pf ON f.numero = pf.fknumfactura
INNER JOIN producto p ON pf.fkcodproducto = p.codigo
INNER JOIN cliente c ON f.fkidcliente = c.id
LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
ORDER BY f.numero DESC, pf.cantidad DESC;
```

---

### C8 — Utilización de Línea de Crédito y Exposición Financiera
- **Tablas:** `cliente (c)`, `persona (pc)`, `empresa (ec)`, `factura (f)` *(4 tablas)*.
- **SQL:**
```sql
SELECT 
    c.id AS IdCliente,
    COALESCE(pc.nombre, 'Sin persona') AS PersonaNombre,
    COALESCE(ec.nombre, 'Sin empresa') AS EmpresaNombre,
    c.credito AS LimiteCredito,
    COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) AS TotalFacturadoActivo,
    (c.credito - COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0)) AS CreditoDisponible,
    CASE 
        WHEN COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) > c.credito THEN 'EXCEDIDO'
        WHEN COALESCE(SUM(CASE WHEN f.estado = 'activa' THEN f.total ELSE 0 END), 0) >= (c.credito * 0.8) THEN 'RIESGO ALTO'
        ELSE 'NORMAL'
    END AS EstadoCredito
FROM cliente c
LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
LEFT JOIN factura f ON c.id = f.fkidcliente
GROUP BY c.id, pc.nombre, ec.nombre, c.credito
ORDER BY TotalFacturadoActivo DESC;
```

---

### C9 — Detección de Clientes Inactivos sin Compras Recientes
- **Tablas:** `cliente (c)`, `persona (pc)`, `empresa (ec)`, `factura (f)` *(4 tablas)*.
- **SQL:**
```sql
SELECT 
    c.id AS IdCliente,
    COALESCE(pc.nombre, 'Persona no registrada') AS NombrePersona,
    pc.email AS EmailContacto,
    pc.telefono AS TelefonoContacto,
    COALESCE(ec.nombre, 'Particular') AS NombreEmpresa,
    c.credito AS Credito,
    COUNT(f.numero) AS FacturasAsociadas,
    'Sin compras activas registradas' AS EstadoActividad
FROM cliente c
LEFT JOIN persona pc ON c.fkcodpersona = pc.codigo
LEFT JOIN empresa ec ON c.fkcodempresa = ec.codigo
LEFT JOIN factura f ON c.id = f.fkidcliente AND f.estado = 'activa'
GROUP BY c.id, pc.nombre, pc.email, pc.telefono, ec.nombre, c.credito
HAVING COUNT(f.numero) = 0
ORDER BY c.id ASC;
```

---

### C10 — Auditoría de Facturas Anuladas e Impacto Financiero
- **Tablas:** `factura (f)`, `vendedor (v)`, `persona (pv)`, `cliente (c)`, `productosporfactura (pf)` *(5 tablas)*.
- **SQL:**
```sql
SELECT 
    f.numero AS NumeroFactura,
    f.fecha AS FechaFactura,
    pv.nombre AS VendedorResponsable,
    v.carnet AS CarnetVendedor,
    c.id AS IdClienteAfectado,
    f.total AS MontoAnulado,
    COUNT(pf.fkcodproducto) AS CantidadRenglonesRevertidos
FROM factura f
INNER JOIN vendedor v ON f.fkidvendedor = v.id
INNER JOIN persona pv ON v.fkcodpersona = pv.codigo
INNER JOIN cliente c ON f.fkidcliente = c.id
LEFT JOIN productosporfactura pf ON f.numero = pf.fknumfactura
WHERE f.estado = 'anulada'
GROUP BY f.numero, f.fecha, pv.nombre, v.carnet, c.id, f.total
ORDER BY f.fecha DESC;
```

