# Flujo Completo de una Petición (FastAPI + Flask)

> Recorrido exacto de una petición desde que la persona hace clic en el
> navegador (`Flask`) hasta SQL Server y de vuelta.

---

## 1. Arquitectura de Procesos Separados

```text
[Navegador] ──HTTP HTML──► [front_python: Flask + Jinja2 (:8099)]
                                  │
                         HTTP + JSON + Bearer JWT
                                  ▼
                           [api_python: FastAPI (:8035)]
                                  │
                         SQL parametrizado / SP (PyMSSQL)
                                  ▼
                           [SQL Server 2022 (:11463)]
```

El frontend en **Flask** (`front_python/app.py`) jamás importa `pymssql` ni tiene acceso a la contraseña de SQL Server en `docker-compose.yml`. Si `api-facturas` se apaga, `front_python` sigue funcionando y muestra el aviso de error de conexión con cero filas.

---

## 2. Capas internas en `api_python/` ante una petición

```mermaid
sequenceDiagram
    autonumber
    actor Web as Front Flask (httpx)
    participant Pyd as Pydantic (peticiones/esquemas.py)
    participant Aut as Autorización (autorizacion/dependencias.py)
    participant Ctrl as Controlador (controladores/)
    participant Srv as Servicio (servicios/)
    participant Repo as IRepositorio* -> SqlServer (repositorios/)
    participant BD as SQL Server (bdfacturas)

    Web->>Pyd: POST /api/producto + Bearer Token + JSON
    Note over Pyd: Si falta un campo o tipo inválido -> HTTP 422 automático
    Pyd->>Aut: exige_usuario_autenticado() -> PyJWT (leeway=0)
    Note over Aut: Token ausente/expirado/alterado -> HTTP 401
    Aut->>BD: exige_permiso("interfaz.productos") -> EXEC verificar_acceso_ruta
    Note over Aut: Si tiene_acceso = false -> HTTP 403
    Aut->>Ctrl: Petición autorizada
    Ctrl->>Srv: servicio_producto.crear(pet)
    Note over Srv: Reglas de negocio puras y modelo Producto (modelos/)
    Srv->>Repo: repo.crear(...)
    Repo->>BD: INSERT INTO producto ... VALUES (%s, %s, %s, %s)
    Note over Repo,Ctrl: Error SQL 2627/547 -> ConflictoExcepcion -> main.py devuelve HTTP 409
    BD-->>Web: HTTP 201 Created + JSON
```

---

## 3. Regla estricta de separación por capa

| Capa | Carpeta | Qué SÍ hace | Qué NUNCA hace |
|---|---|---|---|
| **Peticiones** | `api_python/peticiones/` | Define esquemas por verbo (`*Crear`, `*Reemplazo`, `*Actualizar`) para validar forma (`422`) | Consultar BD |
| **Autorización** | `api_python/autorizacion/` | Valida JWT (`401`) y ejecuta `verificar_acceso_ruta` (`403`) | Inventar `JOIN` locales |
| **Controladores** | `api_python/controladores/` | Recibe HTTP, invoca el servicio ensamblado y devuelve `200`/`201`/`204` | Escribir SQL ni reglas de negocio |
| **Modelos** | `api_python/modelos/` | Representa entidades tipadas de dominio (`Producto`, `Factura`, `Sesion`) | Depender de FastAPI o `pymssql` |
| **Servicios** | `api_python/servicios/` | Ejecuta reglas de dominio, BCrypt y formato `OPENJSON`; lanza excepciones de dominio | Devolver códigos HTTP |
| **Repositorios** | `api_python/repositorios/` | Define interfaces `ABC` (`interfaces.py`) y ejecuta SQL / SP parametrizado | Decidir reglas de negocio ni HTTP |
