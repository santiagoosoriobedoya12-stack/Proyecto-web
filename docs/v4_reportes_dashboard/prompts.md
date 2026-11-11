# Bitácora de Prompts — Versión 4: Reportes, Consultas Multitabla y Dashboard

> Esta bitácora documenta los prompts de ingeniería utilizados para la concepción, diseño SQL, implementación en backend (.NET 10 Web API), visualización en Blazor Server y pruebas de la Versión 4.

---

## 1. Prompts de Diseño SQL — Las 10 Consultas Multitabla (4+ tablas)

### Prompt — Consulta 1: Consolidado Maestro de Facturación
```text
Genera una consulta SQL Server para el sistema bdfacturas que cruce 6 tablas: factura, cliente, persona (titular cliente), empresa (cliente corporativo), vendedor y persona (titular vendedor). La consulta debe devolver el número de factura, fecha, total, estado, el nombre del cliente unificado usando COALESCE (persona o empresa), el tipo de cliente, el contacto del cliente, el nombre del vendedor y su carnet, ordenada por fecha descendente.
```

### Prompt — Consulta 2: Ranking de Productos más Vendidos
```text
Diseña una consulta SQL Server que clasifique los productos por demanda cruzando 4 tablas: producto, productosporfactura, factura y cliente. Debe filtrar únicamente facturas activas, agrupando por código, nombre, valor unitario y stock para calcular la suma de unidades vendidas, el total de dinero recaudado y el conteo de facturas distintas donde figura el producto, ordenada de mayor a menor recaudación.
```

### Prompt — Consulta 3: Desempeño Comercial por Vendedor
```text
Crea una consulta SQL Server cruzando 5 tablas: vendedor, persona (del vendedor), factura, productosporfactura y producto. Agrupa por vendedor para calcular: total de facturas emitidas, facturas activas, facturas anuladas, monto total vendido y el ticket promedio por factura activa. Debe incluir vendedores incluso si no tienen facturas asociadas mediante LEFT JOIN.
```

### Prompt — Consulta 4: Clientes de Alto Valor y Fidelización
```text
Diseña una consulta SQL analítica que cruce cliente, persona, empresa, factura y productosporfactura (5 tablas). La consulta debe segmentar a los mejores clientes calculando: total de facturas activas, monto total comprado acumulado, conteo de productos distintos adquiridos y fecha de su última compra, ordenada por monto total comprado descendente.
```

### Prompt — Consulta 5: Alerta Preventiva de Stock Crítico
```text
Genera una consulta SQL Server que analice el inventario cruzando producto, productosporfactura, factura y vendedor (4 tablas). Debe devolver el stock actual, el total vendido histórico, la fecha de última venta y una columna calculada 'NivelAlerta' que categorice: CRÍTICO (stock <= 5), BAJO (stock <= 15) o ESTABLE, ordenada por stock ascendente.
```

### Prompt — Consulta 6: Auditoría de Seguridad y Roles
```text
Diseña una consulta SQL para auditar la seguridad del sistema cruzando 5 tablas: usuario, rol_usuario, rol, rutarol y ruta. Agrupa por email de usuario para listar en un solo campo los nombres de roles asignados (usando STRING_AGG), la cantidad de roles, la cantidad de rutas autorizadas y el listado de patrones de rutas permitidas.
```

### Prompt — Consulta 7: Renglones Detallados de Facturación
```text
Construye una consulta SQL que audite el detalle ítem por ítem de las facturas cruzando factura, productosporfactura, producto, cliente y persona (5 tablas). Debe devolver número de factura, fecha, cliente, código y nombre del producto, cantidad vendida, precio unitario calculado, subtotal del renglón y el total de la factura.
```

### Prompt — Consulta 8: Utilización de Línea de Crédito
```text
Crea una consulta SQL Server para análisis de riesgo crediticio cruzando cliente, persona, empresa y factura (4 tablas). Debe comparar el límite de crédito otorgado al cliente contra la suma de sus facturas activas, calculando el cupo disponible restante y un semáforo de riesgo: EXCEDIDO, RIESGO ALTO (si supera el 80%) o NORMAL.
```

### Prompt — Consulta 9: Detección de Clientes Inactivos
```text
Diseña una consulta SQL que identifique oportunidades comerciales cruzando cliente, persona, empresa y factura (4 tablas). Utiliza LEFT JOIN y la cláusula HAVING COUNT(facturas activas) = 0 para extraer los clientes registrados que nunca han realizado compras o están completamente inactivos.
```

### Prompt — Consulta 10: Auditoría de Facturas Anuladas
```text
Genera una consulta de control interno y pérdidas cruzando factura, vendedor, persona (del vendedor), cliente y productosporfactura (5 tablas). Debe filtrar únicamente facturas anuladas y calcular el monto total no percibido, el vendedor que la expidió, el cliente involucrado y la cantidad de renglones de producto revertidos al inventario.
```

---

## 2. Prompts de Backend (.NET 10 Web API + Dapper)

### Prompt — Repositorio y Servicio de Consultas
```text
Implementa en C# .NET 10 un repositorio IRepositorioConsultas y su implementación RepositorioConsultasSqlServer utilizando Dapper. Cada uno de los 10 métodos debe ejecutar de forma asíncrona (Task<IEnumerable<T>>) las consultas SQL multitabla optimizadas y mapear a DTOs inmutables fuertemente tipados. Agrega además un método ObtenerDashboardKpisAsync que calcule las métricas del tablero gerencial.
```

### Prompt — Controladores REST con Seguridad JWT
```text
Crea dos controladores en ASP.NET Core Web API: ConsultasController (/api/consultas) y DashboardController (/api/dashboard). Protege ambos controladores con el atributo [Authorize] para exigir el token JWT de la versión 3. Expón los endpoints GET /c1 hasta /c10 y GET /kpis devolviendo ActionResult<IEnumerable<T>> con códigos 200 OK y manejo de excepciones.
```

---

## 3. Prompts de Frontend (Blazor Server)

### Prompt — Componente Dashboard Ejecutivo
```text
Desarrolla en Blazor Server (C# / Razor) una página Dashboard.razor que consuma /api/dashboard/kpis y /api/consultas/c2. Muestra 4 tarjetas KPI con iconos, formato de moneda y porcentajes (Ventas Totales, Total Facturas, Clientes Registrados, Stock Crítico), además de una tabla con los 5 productos más vendidos y una barra visual de rotación construida puramente con CSS y Bootstrap local.
```

### Prompt — Explorador Interactivo de Consultas
```text
Crea una página Consultas.razor en Blazor Server que permita explorar dinámicamente las 10 consultas multitabla complejas. Debe tener un selector desplegable con las 10 opciones, un botón para ejecutar la consulta seleccionada, indicador de carga animado y una tabla responsiva con cabeceras dinámicas y datos formateados según el tipo de reporte.
```

---

## 4. Prompts de Identidad Corporativa y Marca

### Prompt — Manual de Marca y Estilos CSS
```text
Genera estilos CSS corporativos para la plataforma FactuCorp ERP. Aplica una paleta sobria de tipo Slate/Navy (#0f172a, #1e293b, #2563eb, #10b981), bordes redondeados sutiles, tipografía legible y diseño de tarjetas ejecutivas para modernizar la interfaz de usuario en Blazor.
```

