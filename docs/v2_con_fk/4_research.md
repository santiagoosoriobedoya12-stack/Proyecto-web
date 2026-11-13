# Investigación Técnica — Versión 2

1. **Gestión de Concurrencia en Flask (`concurrent.futures.ThreadPoolExecutor`):**
   - Flask por defecto corre sincrónicamente en un hilo de atención por petición.
   - Si una vista requiere 3 catálogos externos vía HTTP, hacerlo de forma secuencial triplica la latencia.
   - `ThreadPoolExecutor` permite disparar las llamadas `httpx` en hilos de trabajo paralelos, esperando la respuesta combinada mediante `futuro.result()`, logrando tiempos de respuesta de milisegundos.
2. **Preservación de Nombres de Columnas Especiales del DDL:**
   - La tabla `docente` incluye nombres específicos como `nacionalidaad` (con doble `'a'`), `cat_minciencia`, `conv_minciencia` y `url_cvlac`.
   - Se validó que tanto los modelos `@dataclass`, los esquemas Pydantic y las sentencias SQL respeten estos nombres exactos sin normalizaciones forzadas.
