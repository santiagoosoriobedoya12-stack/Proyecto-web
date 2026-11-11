# Investigación Técnica — Versión 4: Reportes, Consultas Multitabla y Rendimiento

> | | |
> |---|---|
> | **El marco técnico** | [3_plan.md](3_plan.md) |
> | **La especificación** | [2_spec.md](2_spec.md) |
> | **Los contratos** | [6_contracts.md](6_contracts.md) |

---

## 1. Problema de Investigación: Rendimiento en Consultas Multitabla

En sistemas relacionales empresariales donde una consulta involucra entre 4 y 6 tablas con operaciones de agregación (`SUM`, `AVG`, `COUNT`) y agrupamiento (`GROUP BY`), se presentan dos desafíos críticos:
1. **Sobrecarga del ORM vs SQL Expresivo:** ¿Conviene usar Entity Framework Core (LINQ to Entities) o sentencias SQL nativas optimizadas mediante Dapper?
2. **Vistas Indexadas vs Consultas en Capa de Repositorio:** ¿Dónde debe residir la definición de las 10 consultas complejas?

---

## 2. Evaluación Comparativa

### 2.1. Dapper vs Entity Framework Core para Analítica

| Criterio | Entity Framework Core (LINQ) | Dapper (Micro-ORM) | Decisión v4 |
|---|---|---|---|
| **Mapeo de resultados** | Requiere entidades o proyecciones anónimas | Mapeo directo y liviano a POCO/DTO | **Dapper** |
| **Tiempo de compilación de consulta** | Alto (árbol de expresión LINQ -> AST -> SQL) | Cero (SQL plano directo al driver) | **Dapper** |
| **Consumo de memoria** | Alto (instancias de DbContext y Change Tracker) | Mínimo (lectura en streaming con `IDataReader`) | **Dapper** |
| **Control del plan de ejecución** | Difícil (el motor ORM decide cómo hacer los JOINs) | Total (se eligen los tipos exactos de JOIN y orden) | **Dapper** |
| **Transparencia en sustentación** | El alumno no ve el SQL real generado | El alumno explica cada cláusula SQL en su código | **Dapper** |

**Conclusión:** Se selecciona **Dapper**, manteniendo coherencia con las versiones 1, 2 y 3. Dapper ofrece tiempos de respuesta sub-15ms y permite justificar técnicamente cada cláusula relacional ante el docente evaluador.

### 2.2. Manejo de Integridad y Agregaciones en Tiempo Real

Para el Tablero de Control (Dashboard), se evaluaron dos enfoques:
- **Enfoque A: Tablas sumarias / Caché precalculada:** Escribir en una tabla de resumen en cada factura.
  - *Desventaja:* Riesgo de desincronización contable y sobrecarga en escrituras.
- **Enfoque B: Agregación en tiempo real en SQL Server:**
  - *Ventaja:* La base de datos siempre refleja el estado exacto sin desfase.
  - *Optimización:* Índices sobre `factura.estado` y las claves foráneas permiten calcular los KPIs en menos de 5 milisegundos para miles de registros.

---

## 3. Estrategia de Identidad Corporativa (Manual de Marca)

Para cumplir el estándar de calidad visual y corporativa:
1. **Paleta de Colores Institucional:**
   - **Azul Corporativo Primario (`#0f172a` y `#2563eb`):** Transmite estabilidad, seriedad y solidez contable.
   - **Verde Financiero (`#059669`):** Representa liquidez, ventas aprobadas y saldos a favor.
   - **Ámbar / Naranja (`#d97706`):** Indica alertas operativas (stock bajo, facturas por vencer).
   - **Rojo / Coral (`#dc2626`):** Señaliza facturas anuladas o bloqueos de crédito.
2. **Componentes visuales puros:** Se utilizan barras de porcentaje y tarjetas métricas implementadas en CSS moderno con flexbox y grid, garantizando 100% de responsividad en escritorio y móviles sin depender de scripts pesados.

