# Paradigma de Programación Orientada a Objetos (P.O.O.) en Python

> Guía aplicada al código real del proyecto **Mapa de Conocimiento** (`api_mapa/`).

---

## 1. Python Multiparadigma en este Proyecto

Al igual que en el ejemplo del curso, en `api_mapa/` conviven de forma ordenada varios paradigmas:

- **Estructurado / Imperativo:** Dentro de métodos de validación o helpers SQL (`construir_update_parcial` en [`helpers_sql.py`](../api_mapa/repositorios/helpers_sql.py)).
- **Declarativo:** En las consultas SQL parametrizadas de MariaDB y las plantillas HTML de Jinja2 en [`front_flask/templates/`](../front_flask/templates/base.html).
- **Orientado a Objetos (P.O.O.):** Es el núcleo arquitectónico de las capas (`modelos/`, `peticiones/`, `repositorios/`, `servicios/`).

---

## 2. Los 4 Pilares de P.O.O. en `api_mapa/`

### 2.1 Abstracción (`ABC` en `repositorios/interfaces.py`)
Los servicios no conocen SQL, ni `pymysql`, ni `DictCursor`. Solo dependen de interfaces abstractas (`abc.ABC` con `@abstractmethod`) definidas en [`interfaces.py`](../api_mapa/repositorios/interfaces.py), por ejemplo:
- `IRepositorioProyecto`
- `IRepositorioDocente`
- `IRepositorioProducto`
- `IRepositorioDesarrolla`
- `IRepositorioUsuario`

### 2.2 Encapsulamiento (`modelos/entidades.py` y `excepciones.py`)
- Las 21 entidades de dominio y `Sesion` viven como `@dataclass` en [`entidades.py`](../api_mapa/modelos/entidades.py) y exponen `.a_dict()` (sin exponer `password_hash` en `Usuario`).
- El detalle de conexión, transacciones y el flag `CLIENT.FOUND_ROWS` (para que un `PUT` idempotente cuente la fila encontrada aunque los valores no cambien) queda encapsulado en [`database.py`](../api_mapa/database.py).

### 2.3 Composición sobre Herencia (`servicios/` y `ensamblador.py`)
Los servicios (`ServicioProyecto`, `ServicioDocente`, `ServicioProducto`, `ServicioUsuario`, `ServicioLogin`, etc.) **reciben por constructor** sus repositorios abstractos (`IRepositorio*`).
Por ejemplo, `ServicioDocente` recibe tanto `IRepositorioDocente` como `IRepositorioLineaInvestigacion` para verificar antes de guardar que la línea de investigación exista y esté activa (`activo = 1`).
[`ensamblador.py`](../api_mapa/ensamblador.py) es el único archivo donde se instancian las implementaciones concretas (`Repositorio*MariaDB`) y se inyectan en cada servicio.

### 2.4 Polimorfismo Real (MariaDB vs Memoria en Pruebas)
Como los servicios solo esperan `IRepositorio*`, podemos sustituir el motor MariaDB por repositorios en memoria sin modificar una sola línea de lógica de negocio. En [`pruebas/test_capas.py`](../api_mapa/pruebas/test_capas.py) se implementan:
- `RepositorioProyectoEnMemoria(IRepositorioProyecto)`
- `RepositorioLineaEnMemoria(IRepositorioLineaInvestigacion)`
- `RepositorioTipoProductoEnMemoria(IRepositorioTipoProducto)`
- `RepositorioProductoEnMemoria(IRepositorioProducto)`
- `RepositorioUsuarioEnMemoria(IRepositorioUsuario)`

Gracias al polimorfismo, las pruebas unitarias verifican creación, reemplazo idempotente, borrado lógico, validación de claves foráneas (`409`) y hashing BCrypt en milisegundos sin requerir MariaDB levantada.
