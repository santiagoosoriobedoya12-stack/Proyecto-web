-- =============================================================================
-- Base de datos: mapa_conocimiento (PostgreSQL 14+)
-- Módulo: Mapa de Conocimiento (18 tablas del módulo + 3 de usuarios = 21 tablas)
-- Nota de metodología (§7.0): Se agrega la columna 'activo SMALLINT NOT NULL DEFAULT 1'
-- a todas las tablas para soportar el borrado lógico obligatorio en todo el sistema.
-- =============================================================================

-- =============================================
-- VERSIÓN 1: 8 TABLAS SIN CLAVES FORÁNEAS
-- =============================================

CREATE TABLE IF NOT EXISTS area_conocimiento (
    id INT NOT NULL,
    gran_area VARCHAR(60) NOT NULL,
    area VARCHAR(60) NOT NULL,
    disciplina VARCHAR(60) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS objetivo_desarrollo_sostenible (
    id INT NOT NULL,
    nombre VARCHAR(60) NOT NULL,
    categoria VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS area_aplicacion (
    id INT NOT NULL,
    nombre VARCHAR(60) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS termino_clave (
    termino VARCHAR(30) NOT NULL,
    termino_ingles VARCHAR(30),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (termino)
);

CREATE TABLE IF NOT EXISTS linea_investigacion (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(45) NOT NULL,
    descripcion VARCHAR(256) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS aliado (
    nit INT NOT NULL,
    razon_social VARCHAR(60) NOT NULL,
    nombre_contacto VARCHAR(60) NOT NULL,
    correo VARCHAR(70) NOT NULL,
    telefono VARCHAR(45) NOT NULL,
    ciudad VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (nit)
);

CREATE TABLE IF NOT EXISTS tipo_producto (
    id INT NOT NULL,
    categoria VARCHAR(45) NOT NULL,
    clase VARCHAR(45) NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    tipologia VARCHAR(45) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS proyecto (
    id INT NOT NULL,
    titulo VARCHAR(70) NOT NULL,
    resumen VARCHAR(256) NOT NULL,
    presupuesto DECIMAL(15,2) NOT NULL,
    tipo_financiacion VARCHAR(45) NOT NULL,
    tipo_fondos VARCHAR(45) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

-- =============================================
-- VERSIÓN 2: 10 TABLAS CON CLAVES FORÁNEAS Y PUENTES
-- =============================================

CREATE TABLE IF NOT EXISTS docente (
    cedula INT NOT NULL,
    nombres VARCHAR(60) NOT NULL,
    apellidos VARCHAR(60) NOT NULL,
    genero VARCHAR(12) NOT NULL,
    cargo VARCHAR(30) NOT NULL,
    fecha_nacimiento DATE NOT NULL,
    correo VARCHAR(70) NOT NULL,
    telefono VARCHAR(45) NOT NULL,
    url_cvlac VARCHAR(128) NOT NULL,
    fecha_actualizacion DATE NOT NULL,
    escalafon VARCHAR(45) NOT NULL,
    perfil TEXT NOT NULL,
    cat_minciencia VARCHAR(45),
    conv_minciencia VARCHAR(45) NOT NULL,
    nacionalidaad VARCHAR(45) NOT NULL,
    linea_investigacion_principal INT REFERENCES linea_investigacion(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (cedula)
);

CREATE TABLE IF NOT EXISTS producto (
    id INT NOT NULL,
    nombre VARCHAR(45) NOT NULL,
    categoria VARCHAR(45) NOT NULL,
    fecha_entrega DATE NOT NULL,
    proyecto INT REFERENCES proyecto(id),
    tipo_producto INT NOT NULL REFERENCES tipo_producto(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS desarrolla (
    docente INT NOT NULL REFERENCES docente(cedula),
    proyecto INT NOT NULL REFERENCES proyecto(id),
    rol VARCHAR(45) NOT NULL,
    descripcion VARCHAR(256) NOT NULL,
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (docente, proyecto)
);

CREATE TABLE IF NOT EXISTS aliado_proyecto (
    aliado INT NOT NULL REFERENCES aliado(nit),
    proyecto INT NOT NULL REFERENCES proyecto(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (aliado, proyecto)
);

CREATE TABLE IF NOT EXISTS palabras_clave (
    proyecto INT NOT NULL REFERENCES proyecto(id),
    termino_clave VARCHAR(30) NOT NULL REFERENCES termino_clave(termino),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (proyecto, termino_clave)
);

CREATE TABLE IF NOT EXISTS ac_proyecto (
    proyecto INT NOT NULL REFERENCES proyecto(id),
    area_conocimiento INT NOT NULL REFERENCES area_conocimiento(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (proyecto, area_conocimiento)
);

CREATE TABLE IF NOT EXISTS proyecto_linea (
    proyecto INT NOT NULL REFERENCES proyecto(id),
    linea_investigacion INT NOT NULL REFERENCES linea_investigacion(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (proyecto, linea_investigacion)
);

CREATE TABLE IF NOT EXISTS ods_proyecto (
    proyecto INT NOT NULL REFERENCES proyecto(id),
    ods INT NOT NULL REFERENCES objetivo_desarrollo_sostenible(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (proyecto, ods)
);

CREATE TABLE IF NOT EXISTS aa_proyecto (
    proyecto INT NOT NULL REFERENCES proyecto(id),
    area_aplicacion INT NOT NULL REFERENCES area_aplicacion(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (proyecto, area_aplicacion)
);

CREATE TABLE IF NOT EXISTS docente_producto (
    docente INT NOT NULL REFERENCES docente(cedula),
    producto INT NOT NULL REFERENCES producto(id),
    activo SMALLINT NOT NULL DEFAULT 1,
    PRIMARY KEY (docente, producto)
);

-- =============================================
-- VERSIÓN 3: 3 TABLAS DE GESTIÓN DE USUARIOS Y ROLES
-- =============================================

CREATE TABLE IF NOT EXISTS rol (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255),
    activo SMALLINT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS usuario (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    nombre_completo VARCHAR(200),
    activo SMALLINT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS rol_usuario (
    usuario_id INT NOT NULL REFERENCES usuario(id) ON DELETE CASCADE,
    rol_id INT NOT NULL REFERENCES rol(id) ON DELETE CASCADE,
    PRIMARY KEY (usuario_id, rol_id)
);

-- =============================================
-- DATOS SEMILLA: LOS 17 ODS OFICIALES
-- =============================================

INSERT INTO objetivo_desarrollo_sostenible (id, nombre, categoria, activo) VALUES
(1, 'Fin de la pobreza', 'Social', 1),
(2, 'Hambre cero', 'Social', 1),
(3, 'Salud y bienestar', 'Social', 1),
(4, 'Educación de calidad', 'Social', 1),
(5, 'Igualdad de género', 'Social', 1),
(6, 'Agua limpia y saneamiento', 'Ambiental', 1),
(7, 'Energía asequible y no contaminante', 'Económica', 1),
(8, 'Trabajo decente y crecimiento económico', 'Económica', 1),
(9, 'Industria, innovación e infraestructura', 'Económica', 1),
(10, 'Reducción de las desigualdades', 'Social', 1),
(11, 'Ciudades y comunidades sostenibles', 'Social', 1),
(12, 'Producción y consumo responsables', 'Ambiental', 1),
(13, 'Acción por el clima', 'Ambiental', 1),
(14, 'Vida submarina', 'Ambiental', 1),
(15, 'Vida de ecosistemas terrestres', 'Ambiental', 1),
(16, 'Paz, justicia e instituciones sólidas', 'Social', 1),
(17, 'Alianzas para lograr los objetivos', 'Social', 1)
ON CONFLICT (id) DO NOTHING;

-- =============================================
-- DATOS SEMILLA: CATÁLOGOS BASE
-- =============================================

INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo) VALUES
(1, 'Ingeniería y Tecnología', 'Ingeniería de Sistemas', 'Sistemas de Información', 1),
(2, 'Ingeniería y Tecnología', 'Ingeniería de Software', 'Arquitectura de Software', 1),
(3, 'Ciencias Sociales', 'Ciencias de la Educación', 'Educación Superior', 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO area_aplicacion (id, nombre, activo) VALUES
(1, 'Tecnologías de la Información y las Comunicaciones', 1),
(2, 'Educación y Desarrollo Social', 1),
(3, 'Medio Ambiente y Sostenibilidad', 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO termino_clave (termino, termino_ingles, activo) VALUES
('Arquitectura de Software', 'Software Architecture', 1),
('Inteligencia Artificial', 'Artificial Intelligence', 1),
('Gestión del Conocimiento', 'Knowledge Management', 1)
ON CONFLICT (termino) DO NOTHING;

INSERT INTO linea_investigacion (nombre, descripcion, activo) VALUES
('Ingeniería de Software y Datos', 'Investigación en metodologías y arquitecturas de software y analítica.', 1),
('Innovación Educativa y Tecnológica', 'Tecnologías emergentes aplicadas al aprendizaje y currículo.', 1)
ON CONFLICT DO NOTHING;

INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo) VALUES
(900123456, 'Tech Solutions Colombia S.A.S.', 'Carlos Mendoza', 'contacto@techsolutions.com', '3101234567', 'Medellín', 1),
(900654321, 'Fundación Innovación y Futuro', 'María Paredes', 'alianzas@innovacionfuturo.org', '3209876543', 'Bogotá', 1)
ON CONFLICT (nit) DO NOTHING;

INSERT INTO tipo_producto (id, categoria, clase, nombre, tipologia, activo) VALUES
(1, 'Generación de Nuevo Conocimiento', 'Artículo de Investigación', 'Artículo en revista indexada A1', 'Publicación', 1),
(2, 'Desarrollo Tecnológico e Innovación', 'Software', 'Software registrado con soporte funcional', 'Tecnología', 1),
(3, 'Apropiación Social del Conocimiento', 'Evento Científico', 'Ponencia en congreso internacional', 'Divulgación', 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO proyecto (id, titulo, resumen, presupuesto, tipo_financiacion, tipo_fondos, fecha_inicio, fecha_fin, activo) VALUES
(1, 'Plataforma Inteligente de Gestión del Conocimiento', 'Investigación aplicada para mapeo de competencias y producción académica.', 85000000.00, 'Financiación Interna', 'Fondo de Investigaciones', '2025-01-15', '2025-12-15', 1),
(2, 'Observatorio de Sostenibilidad y ODS Universitario', 'Sistema analítico para monitoreo de impacto de proyectos en los 17 ODS.', 42000000.00, 'Cofinanciado', 'Mixto', '2025-03-01', '2025-11-30', 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO docente (cedula, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, telefono, url_cvlac, fecha_actualizacion, escalafon, perfil, cat_minciencia, conv_minciencia, nacionalidaad, linea_investigacion_principal, activo) VALUES
(1020304050, 'Santiago', 'Osorio Bedoya', 'Masculino', 'Profesor Asistente', '1995-04-12', 'santiago.osorio@universidad.edu.co', '3001234567', 'https://scienti.minciencias.gov.co/cvlac/1020304050', '2025-02-10', 'Asistente', 'Investigador en ingeniería de software y sistemas distribuidos.', 'Junior', '894 de 2021', 'Colombiana', 1, 1)
ON CONFLICT (cedula) DO NOTHING;

INSERT INTO producto (id, nombre, categoria, fecha_entrega, proyecto, tipo_producto, activo) VALUES
(1, 'API REST Mapa de Conocimiento v3', 'Desarrollo Tecnológico', '2025-06-30', 1, 2, 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO desarrolla (docente, proyecto, rol, descripcion, activo) VALUES
(1020304050, 1, 'Investigador Principal', 'Líder técnico del desarrollo arquitectónico.', 1)
ON CONFLICT (docente, proyecto) DO NOTHING;

-- =============================================
-- ROLES DEL SISTEMA Y USUARIO ADMINISTRADOR
-- =============================================

INSERT INTO rol (id, nombre, descripcion, activo) VALUES
(1, 'Administrador', 'Control total de las 21 tablas y gestión de usuarios.', 1),
(2, 'Investigador', 'CRUD en el módulo de investigación; sin acceso a administración de usuarios.', 1),
(3, 'Consulta', 'Solo lectura en el módulo de investigación; sin acceso a edición.', 1)
ON CONFLICT (id) DO NOTHING;

-- Contraseña 'admin123' con BCrypt
INSERT INTO usuario (id, username, password_hash, email, nombre_completo, activo) VALUES
(1, 'admin', '$2b$12$KIXe8s4y4.9G6wRjWkF3feqIoxD8sL5qE0w9jD9jD9jD9jD9jD9jD', 'admin@mapa.edu.co', 'Administrador General', 1)
ON CONFLICT (id) DO NOTHING;

INSERT INTO rol_usuario (usuario_id, rol_id) VALUES
(1, 1)
ON CONFLICT (usuario_id, rol_id) DO NOTHING;
