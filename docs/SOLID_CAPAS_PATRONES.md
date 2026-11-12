# Principios SOLID, Capas y Patrones en Mapa de Conocimiento

---

## 1. Arquitectura en Capas (De Arriba hacia Abajo)

| Capa | Carpeta / Archivo | Responsabilidad única | Lo que NUNCA hace |
| :--- | :--- | :--- | :--- |
| **Presentación Web** | [`front_flask/`](../front_flask/app.py) | Renderizar Jinja2, guardar sesión `mapa_sid` y llamar a `api-mapa` por `httpx` | Importar `pymysql` o conectarse a MariaDB |
| **Controladores REST** | [`api_mapa/controladores/`](../api_mapa/controladores/controlador_v1.py) | Enrutar HTTP explícito (`/api/...`), dependencias JWT/roles y códigos HTTP | Escribir SQL o lógica de negocio |
| **Peticiones (DTOs)** | [`api_mapa/peticiones/esquemas.py`](../api_mapa/peticiones/esquemas.py) | Validar estructura de entrada por verbo HTTP (`422`) | Mezclar DTOs de creación y parcheo |
| **Servicios** | [`api_mapa/servicios/`](../api_mapa/servicios/servicio_v1.py) | Reglas de negocio, validación de FK activas y coordinación de repositorios | Conocer `FastAPI`, `HTTP` o SQL |
| **Repositorios** | [`api_mapa/repositorios/`](../api_mapa/repositorios/repositorio_v1.py) | SQL parametrizado en MariaDB o almacenamiento en memoria, filtro `activo = 1` | Devolver `JSONResponse` o reglas de autorización |
| **Modelos** | [`api_mapa/modelos/entidades.py`](../api_mapa/modelos/entidades.py) | Entidades de dominio POO (`@dataclass`) para las 21 tablas + `Sesion` | Contener lógica de transporte HTTP |

---

## 2. Principios SOLID Aplicados

- **S — Responsabilidad Única:** Cada archivo cumple una sola función (`config.py` lee entorno, `excepciones.py` define errores de dominio, `semilla.py` siembra usuarios, `ensamblador.py` conecta capas).
- **O — Abierto/Cerrado:** Cambiar o agregar otro motor de base de datos o un repositorio en memoria para pruebas solo requiere implementar las interfaces de [`interfaces.py`](../api_mapa/repositorios/interfaces.py) y cambiar la inyección en [`ensamblador.py`](../api_mapa/ensamblador.py), sin tocar servicios ni controladores.
- **L — Sustitución de Liskov:** `RepositorioProyectoEnMemoria` sustituye a `RepositorioProyectoMariaDB` en [`pruebas/test_capas.py`](../api_mapa/pruebas/test_capas.py) cumpliendo exactamente el contrato `IRepositorioProyecto` (incluidas excepciones `NoEncontradoExcepcion` y `ConflictoExcepcion`).
- **I — Segregación de Interfaces y Esquemas:**
  - Interfaces separadas por agregado (`IRepositorioDocente`, `IRepositorioDesarrolla`, `IRepositorioPuente`, etc.).
  - Esquemas Pydantic segregados por operación: `*Crear` (`POST`), `*Reemplazo` (`PUT`) y `*Actualizar` (`PATCH`).
- **D — Inversión de Dependencias:** Los servicios dependen de abstracciones (`IRepositorio*`), nunca de clases concretas (`Repositorio*MariaDB`).

---

## 3. Patrones Aplicados

1. **Repository Pattern + Dependency Injection:** Desacopla el dominio del motor MariaDB y permite pruebas unitarias sin base de datos.
2. **Composition Root (`ensamblador.py`):** Centraliza la creación del grafo de dependencias en un único módulo.
3. **Router Factory explícito (`_crear_router_puente`) / Ops explícitas:** Evita rutas comodín inseguras (`/{tabla}`) manteniendo los prefijos REST declarados por recurso.
4. **Logical Delete (Soft Delete):** Todas las tablas del módulo y seguridad conservan el histórico (`activo = 0`) con `CLIENT.FOUND_ROWS` en PyMySQL para idempotencia de `PUT`.
