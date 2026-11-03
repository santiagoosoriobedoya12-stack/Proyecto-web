# Modelo de Datos y Seguridad — Versión 3: Tablas y Procedimiento RBAC

> En la Versión 3 no se altera la estructura de las 12 tablas. Se aprovechan las tablas de seguridad y el procedimiento almacenado que ya existen en `db/bdfacturas.sql`.

---

## 1. El Grafo Relacional de Seguridad

```
   ┌─────────┐
   │ usuario │ (email, contrasena)
   └────┬────┘
        │ 1:N
        ▼
   ┌─────────────┐               ┌─────────┐
   │ rol_usuario │ M:N puente ──►│   rol   │ (id, nombre)
   └─────────────┘               └────┬────┘
                                      │ 1:N
                                      ▼
   ┌─────────┐                   ┌─────────┐
   │  ruta   │ ◄── M:N puente ── │ rutarol │
   └─────────┘                   └─────────┘
   (id, patron)
```

### Relaciones:
1. Un **`usuario`** tiene uno o varios **`roles`** a través de la tabla puente `rol_usuario`.
2. Un **`rol`** tiene asignadas varias **`rutas`** a través de la tabla puente `rutarol`.
3. Una **`ruta`** define el patrón de URL que un rol tiene permitido acceder (ej: `/api/factura`, `/api/producto`).

---

## 2. Detalle de las Tablas Involucradas

### Tabla `usuario`
```sql
CREATE TABLE usuario (
    email VARCHAR(100) PRIMARY KEY,
    contrasena VARCHAR(255) NOT NULL -- En v3 almacena el Hash criptográfico con Salt
);
```

### Tabla `rol`
```sql
CREATE TABLE rol (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);
```

### Tabla puente `rol_usuario`
```sql
CREATE TABLE rol_usuario (
    fkemail VARCHAR(100) NOT NULL,
    fkidrol INT NOT NULL,
    PRIMARY KEY (fkemail, fkidrol),
    FOREIGN KEY (fkemail) REFERENCES usuario(email) ON DELETE CASCADE,
    FOREIGN KEY (fkidrol) REFERENCES rol(id) ON DELETE CASCADE
);
```

### Tabla `ruta`
```sql
CREATE TABLE ruta (
    id INT IDENTITY(1,1) PRIMARY KEY,
    patron VARCHAR(200) NOT NULL UNIQUE,
    descripcion VARCHAR(255) NULL
);
```

### Tabla puente `rutarol`
```sql
CREATE TABLE rutarol (
    fkidruta INT NOT NULL,
    fkidrol INT NOT NULL,
    PRIMARY KEY (fkidruta, fkidrol),
    FOREIGN KEY (fkidruta) REFERENCES ruta(id) ON DELETE CASCADE,
    FOREIGN KEY (fkidrol) REFERENCES rol(id) ON DELETE CASCADE
);
```

---

## 3. El Procedimiento Almacenado de Verificación (`verificar_acceso_ruta`)

Este procedimiento almacenado se encuentra compilado en SQL Server y constituye el corazón del control de acceso dinámico:

```sql
CREATE OR ALTER PROCEDURE verificar_acceso_ruta
    @email VARCHAR(100),
    @ruta VARCHAR(200),
    @tiene_acceso BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Si el usuario es Administrador (Rol id = 1 o nombre = 'Administrador'), acceso total
    IF EXISTS (
        SELECT 1 
        FROM rol_usuario ru 
        INNER JOIN rol r ON ru.fkidrol = r.id 
        WHERE ru.fkemail = @email AND (r.id = 1 OR r.nombre = 'Administrador')
    )
    BEGIN
        SET @tiene_acceso = 1;
        RETURN;
    END

    -- Para los demás roles, verifica si alguno tiene asignada la ruta solicitada
    IF EXISTS (
        SELECT 1
        FROM rol_usuario ru
        INNER JOIN rutarol rr ON ru.fkidrol = rr.fkidrol
        INNER JOIN ruta rt ON rr.fkidruta = rt.id
        WHERE ru.fkemail = @email 
          AND (@ruta LIKE rt.patron OR rt.patron = @ruta)
    )
    BEGIN
        SET @tiene_acceso = 1;
    END
    ELSE
    BEGIN
        SET @tiene_acceso = 0;
    END
END;
```

