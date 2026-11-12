# Principios SOLID, Capas y Patrones Aplicados

> Cómo se reflejan los 5 principios **SOLID** y el patrón **Repository + Dependency Injection** en `api_python/` y `front_python/`.

---

## 1. Aplicación de SOLID en el proyecto Python

| Principio | Cómo se aplica en nuestro código | Archivo donde verlo |
|---|---|---|
| **S — Responsabilidad Única (SRP)** | Cada capa tiene un único motivo de cambio: HTTP en controladores, reglas en servicios, SQL en repositorios, validación de entrada en Pydantic y errores HTTP en `main.py`. | `controladores/`, `servicios/`, `repositorios/`, `main.py` |
| **O — Abierto/Cerrado (OCP)** | Nuevos recursos (v2) y seguridad transversal (v3) se agregaron sin modificar ni romper los contratos previos de v1. | `controlador_v1.py`, `controlador_v2.py`, `autorizacion/dependencias.py` |
| **L — Sustitución de Liskov (LSP)** | Cualquier clase que implemente `IRepositorioProducto` (`RepositorioProductoSqlServer` o `RepositorioProductoEnMemoria`) sustituye a la interfaz sin alterar el comportamiento de `ServicioProducto`. | `repositorios/interfaces.py`, `pruebas/test_capas.py` |
| **I — Segregación de Interfaces (ISP)** | Existe una interfaz pequeña por entidad (`IRepositorioProducto`, `IRepositorioFactura`, `IRepositorioAcceso`) en vez de una macro-interfaz genérica con métodos innecesarios. En Pydantic, cada verbo tiene su propio esquema (`Crear`, `Reemplazo`, `Actualizar`). | `repositorios/interfaces.py`, `peticiones/esquemas.py` |
| **D — Inversión de Dependencias (DIP)** | Los módulos de alto nivel (`servicios/`) dependen de abstracciones (`IRepositorio*`), nunca de clases concretas de PyMSSQL. El cableado ocurre únicamente en `ensamblador.py`. | `servicios/*.py`, `ensamblador.py` |

---

## 2. Patrones clave visibles en sustentación

1. **Repository Pattern + Interfaces `ABC`:** desacopla el motor SQL Server 2022 del dominio Python.
2. **Dependency Injection (Manual Assembler):** `ensamblador.py` crea las instancias concretas e inyecta los contratos por constructor.
3. **DTO Segregados por Verbo:** `POST` (*Crear*), `PUT` (*Reemplazo completo*), `PATCH` (*Actualizar parcial*).
4. **Server-Side Session + HttpOnly Cookie (Frontend Flask):** el token JWT vive en memoria del servidor Flask (`SESIONES` en `cliente_api.py`), protegido contra lectura por JavaScript (XSS).
