-- =============================================================================
-- Base de datos: mapa_conocimiento (MariaDB / MySQL - InnoDB)
-- Módulo: Mapa de Conocimiento (18 tablas del módulo + 3 de usuarios = 21 tablas)
-- Nota de metodología (§7.0): Se agrega la columna 'activo TINYINT(1) NOT NULL DEFAULT 1'
-- a las 18 tablas del módulo para soportar el borrado lógico obligatorio en todo el sistema.
-- =============================================================================

CREATE DATABASE IF NOT EXISTS `mapa_conocimiento`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE `mapa_conocimiento`;

-- =============================================
-- VERSIÓN 1: 8 TABLAS SIN CLAVES FORÁNEAS
-- =============================================

CREATE TABLE IF NOT EXISTS `area_conocimiento` (
    `id` INT NOT NULL,
    `gran_area` VARCHAR(60) NOT NULL,
    `area` VARCHAR(60) NOT NULL,
    `disciplina` VARCHAR(60) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `objetivo_desarrollo_sostenible` (
    `id` INT NOT NULL,
    `nombre` VARCHAR(60) NOT NULL,
    `categoria` VARCHAR(45) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `area_aplicacion` (
    `id` INT NOT NULL,
    `nombre` VARCHAR(60) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `termino_clave` (
    `termino` VARCHAR(30) NOT NULL,
    `termino_ingles` VARCHAR(30),
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`termino`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `linea_investigacion` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `nombre` VARCHAR(45) NOT NULL,
    `descripcion` VARCHAR(256) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `aliado` (
    `nit` INT NOT NULL,
    `razon_social` VARCHAR(60) NOT NULL,
    `nombre_contacto` VARCHAR(60) NOT NULL,
    `correo` VARCHAR(70) NOT NULL,
    `telefono` VARCHAR(45) NOT NULL,
    `ciudad` VARCHAR(45) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`nit`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `tipo_producto` (
    `id` INT NOT NULL,
    `categoria` VARCHAR(45) NOT NULL,
    `clase` VARCHAR(45) NOT NULL,
    `nombre` VARCHAR(45) NOT NULL,
    `tipologia` VARCHAR(45) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `proyecto` (
    `id` INT NOT NULL,
    `titulo` VARCHAR(70) NOT NULL,
    `resumen` VARCHAR(256) NOT NULL,
    `presupuesto` DOUBLE NOT NULL,
    `tipo_financiacion` VARCHAR(45) NOT NULL,
    `tipo_fondos` VARCHAR(45) NOT NULL,
    `fecha_inicio` DATE NOT NULL,
    `fecha_fin` DATE,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB;

-- =============================================
-- VERSIÓN 2: 10 TABLAS CON CLAVES FORÁNEAS
-- =============================================

CREATE TABLE IF NOT EXISTS `docente` (
    `cedula` INT NOT NULL,
    `nombres` VARCHAR(60) NOT NULL,
    `apellidos` VARCHAR(60) NOT NULL,
    `genero` VARCHAR(12) NOT NULL,
    `cargo` VARCHAR(30) NOT NULL,
    `fecha_nacimiento` DATE NOT NULL,
    `correo` VARCHAR(70) NOT NULL,
    `telefono` VARCHAR(20) NOT NULL,
    `url_cvlac` VARCHAR(128) NOT NULL,
    `fecha_actualizacion` DATE NOT NULL,
    `escalafon` VARCHAR(45) NOT NULL,
    `perfil` LONGTEXT NOT NULL,
    `cat_minciencia` VARCHAR(45),
    `conv_minciencia` VARCHAR(45) NOT NULL,
    `nacionalidaad` VARCHAR(45) NOT NULL,
    `linea_investigacion_principal` INT,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`cedula`),
    FOREIGN KEY (`linea_investigacion_principal`) REFERENCES `linea_investigacion`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `producto` (
    `id` INT NOT NULL,
    `nombre` VARCHAR(45) NOT NULL,
    `categoria` VARCHAR(45) NOT NULL,
    `fecha_entrega` DATE NOT NULL,
    `proyecto` INT,
    `tipo_producto` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`tipo_producto`) REFERENCES `tipo_producto`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `aliado_proyecto` (
    `aliado` INT NOT NULL,
    `proyecto` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`aliado`, `proyecto`),
    FOREIGN KEY (`aliado`) REFERENCES `aliado`(`nit`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `desarrolla` (
    `docente` INT NOT NULL,
    `proyecto` INT NOT NULL,
    `rol` VARCHAR(45) NOT NULL,
    `descripcion` VARCHAR(256) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`docente`, `proyecto`),
    FOREIGN KEY (`docente`) REFERENCES `docente`(`cedula`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `palabras_clave` (
    `proyecto` INT NOT NULL,
    `termino_clave` VARCHAR(30) NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`proyecto`, `termino_clave`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`termino_clave`) REFERENCES `termino_clave`(`termino`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `ac_proyecto` (
    `proyecto` INT NOT NULL,
    `area_conocimiento` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`proyecto`, `area_conocimiento`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`area_conocimiento`) REFERENCES `area_conocimiento`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `proyecto_linea` (
    `proyecto` INT NOT NULL,
    `linea_investigacion` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`proyecto`, `linea_investigacion`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`linea_investigacion`) REFERENCES `linea_investigacion`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `ods_proyecto` (
    `proyecto` INT NOT NULL,
    `ods` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`proyecto`, `ods`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`ods`) REFERENCES `objetivo_desarrollo_sostenible`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `aa_proyecto` (
    `proyecto` INT NOT NULL,
    `area_aplicacion` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`proyecto`, `area_aplicacion`),
    FOREIGN KEY (`proyecto`) REFERENCES `proyecto`(`id`),
    FOREIGN KEY (`area_aplicacion`) REFERENCES `area_aplicacion`(`id`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `docente_producto` (
    `docente` INT NOT NULL,
    `producto` INT NOT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`docente`, `producto`),
    FOREIGN KEY (`docente`) REFERENCES `docente`(`cedula`),
    FOREIGN KEY (`producto`) REFERENCES `producto`(`id`)
) ENGINE=InnoDB;

-- =============================================
-- VERSIÓN 3: 3 TABLAS DE GESTIÓN DE USUARIOS
-- =============================================

CREATE TABLE IF NOT EXISTS `rol` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nombre` VARCHAR(100) NOT NULL UNIQUE,
    `descripcion` TEXT,
    `activo` TINYINT(1) DEFAULT 1,
    `fecha_creacion` DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `usuario` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `username` VARCHAR(100) NOT NULL UNIQUE,
    `password` VARCHAR(255) NOT NULL,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `nombre_completo` VARCHAR(200),
    `activo` TINYINT(1) DEFAULT 1,
    `fecha_creacion` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `fecha_actualizacion` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `rol_usuario` (
    `usuario_id` INT NOT NULL,
    `rol_id` INT NOT NULL,
    PRIMARY KEY (`usuario_id`, `rol_id`),
    FOREIGN KEY (`usuario_id`) REFERENCES `usuario`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`rol_id`) REFERENCES `rol`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- =============================================================================
-- DATOS SEMILLA INICIALES (CATÁLOGOS BASE + 17 ODS + DATOS DE PRUEBA + ROLES)
-- =============================================================================

INSERT IGNORE INTO `objetivo_desarrollo_sostenible` (`id`, `nombre`, `categoria`) VALUES
(1, 'Fin de la pobreza', 'Social'),
(2, 'Hambre cero', 'Social'),
(3, 'Salud y bienestar', 'Social'),
(4, 'Educacion de calidad', 'Social'),
(5, 'Igualdad de genero', 'Social'),
(6, 'Agua limpia y saneamiento', 'Ambiental'),
(7, 'Energia asequible y no contaminante', 'Ambiental'),
(8, 'Trabajo decente y crecimiento economico', 'Economica'),
(9, 'Industria, innovacion e infraestructura', 'Economica'),
(10, 'Reduccion de las desigualdades', 'Social'),
(11, 'Ciudades y comunidades sostenibles', 'Ambiental'),
(12, 'Produccion y consumo responsables', 'Economica'),
(13, 'Accion por el clima', 'Ambiental'),
(14, 'Vida submarina', 'Ambiental'),
(15, 'Vida de ecosistemas terrestres', 'Ambiental'),
(16, 'Paz, justicia e instituciones solidas', 'Institucional'),
(17, 'Alianzas para lograr los objetivos', 'Institucional');

INSERT IGNORE INTO `area_conocimiento` (`id`, `gran_area`, `area`, `disciplina`) VALUES
(1, 'Ingenieria y Tecnologia', 'Ingenieria Electrica, Electronica e Informatica', 'Ciencias de la Computacion'),
(2, 'Ingenieria y Tecnologia', 'Ingenieria Civil', 'Ingenieria Ambiental'),
(3, 'Ciencias Naturales', 'Matematicas', 'Estadistica y Probabilidad'),
(4, 'Ciencias Medicas y de la Salud', 'Ciencias de la Salud', 'Salud Publica'),
(5, 'Ciencias Sociales', 'Economia y Negocios', 'Gestion del Conocimiento'),
(6, 'Ciencias Agricolas', 'Agricultura, Silvicultura y Pesca', 'Agroecologia');

INSERT IGNORE INTO `area_aplicacion` (`id`, `nombre`) VALUES
(1, 'Educacion Superior y Ciencia'),
(2, 'Salud y Bienestar Social'),
(3, 'Tecnologias de la Informacion'),
(4, 'Industria y Manufactura'),
(5, 'Medio Ambiente y Sostenibilidad'),
(6, 'Sector Agropecuario');

INSERT IGNORE INTO `termino_clave` (`termino`, `termino_ingles`) VALUES
('Inteligencia Artificial', 'Artificial Intelligence'),
('Analitica de Datos', 'Data Analytics'),
('Sostenibilidad', 'Sustainability'),
('Ciencia Abierta', 'Open Science'),
('IoT', 'Internet of Things');

INSERT IGNORE INTO `linea_investigacion` (`id`, `nombre`, `descripcion`) VALUES
(1, 'Sistemas Inteligentes y Datos', 'Modelos predictivos, analitica de datos e IA aplicada.'),
(2, 'Tecnologias Sostenibles', 'Innovacion tecnologica orientada a los ODS y energia limpia.'),
(3, 'Gestion del Conocimiento Universitario', 'Articulacion de capacidades investigativas entre sedes.');

INSERT IGNORE INTO `aliado` (`nit`, `razon_social`, `nombre_contacto`, `correo`, `telefono`, `ciudad`) VALUES
(900123456, 'Centro de Innovacion Regional S.A.S.', 'Laura Restrepo', 'contacto@cir.org.co', '6043102030', 'Medellin'),
(890900111, 'Secretaria de Ciencia y Tecnologia', 'Carlos Mendoza', 'proyectos@sct.gov.co', '6014405566', 'Bogota');

INSERT IGNORE INTO `tipo_producto` (`id`, `categoria`, `clase`, `nombre`, `tipologia`) VALUES
(1, 'Nuevo Conocimiento', 'Articulo de investigacion', 'Articulo A1', 'Publicacion cientifica'),
(2, 'Desarrollo Tecnologico', 'Software', 'Plataforma Web / API', 'Software certificado'),
(3, 'Apropiacion Social', 'Evento cientifico', 'Ponencia internacional', 'Divulgacion');

INSERT IGNORE INTO `proyecto` (`id`, `titulo`, `resumen`, `presupuesto`, `tipo_financiacion`, `tipo_fondos`, `fecha_inicio`, `fecha_fin`) VALUES
(101, 'Observatorio Universitario de Conocimiento Regional', 'Plataforma para mapear proyectos, docentes y productos cientificos.', 85000000, 'Cofinanciado', 'Internos y Externos', '2026-01-15', '2026-12-15'),
(102, 'Analitica Ambiental para Cuencas Urbanas', 'Monitoreo inteligente de calidad hidrica en seccionales.', 62000000, 'Externo', 'Minciencias', '2026-02-01', NULL),
(103, 'Red de Ciencia Abierta y ODS', 'Articulacion curricular de proyectos con impacto sostenible.', 40000000, 'Interno', 'Propios', '2026-03-01', '2027-02-28');

INSERT IGNORE INTO `docente` (`cedula`, `nombres`, `apellidos`, `genero`, `cargo`, `fecha_nacimiento`, `correo`, `telefono`, `url_cvlac`, `fecha_actualizacion`, `escalafon`, `perfil`, `cat_minciencia`, `conv_minciencia`, `nacionalidaad`, `linea_investigacion_principal`) VALUES
(71000101, 'Ana Maria', 'Gomez Perez', 'Femenino', 'Docente Titular', '1982-05-12', 'ana.gomez@uni.edu.co', '3001112233', 'https://scienti.minciencias.gov.co/cvlac/001', '2026-08-01', 'Titular', 'Doctora en Ingenieria de Sistemas enfocada en gestion del conocimiento.', 'Investigador Senior', 'Convocatoria 894', 'Colombiana', 1),
(98000202, 'Julian', 'Ramirez Velez', 'Masculino', 'Docente Asociado', '1987-11-20', 'julian.ramirez@uni.edu.co', '3104445566', 'https://scienti.minciencias.gov.co/cvlac/002', '2026-09-10', 'Asociado', 'Magister en Analitica de Datos y redes academicas.', 'Investigador Asociado', 'Convocatoria 894', 'Colombiana', 2);

INSERT IGNORE INTO `producto` (`id`, `nombre`, `categoria`, `fecha_entrega`, `proyecto`, `tipo_producto`) VALUES
(501, 'API y Portal Mapa Conocimiento', 'Desarrollo Tecnologico', '2026-09-30', 101, 2),
(502, 'Articulo Medicion Impacto ODS', 'Nuevo Conocimiento', '2026-10-15', 103, 1);

INSERT IGNORE INTO `aliado_proyecto` (`aliado`, `proyecto`) VALUES (900123456, 101), (890900111, 102);
INSERT IGNORE INTO `desarrolla` (`docente`, `proyecto`, `rol`, `descripcion`) VALUES
(71000101, 101, 'Investigador Principal', 'Direccion cientifica y modelado del observatorio.'),
(98000202, 101, 'Coinvestigador', 'Diseno de integracion analitica y validacion.'),
(98000202, 102, 'Investigador Principal', 'Coordinacion de sensores y tablero ambiental.');
INSERT IGNORE INTO `palabras_clave` (`proyecto`, `termino_clave`) VALUES (101, 'Analitica de Datos'), (101, 'Ciencia Abierta'), (102, 'IoT');
INSERT IGNORE INTO `ac_proyecto` (`proyecto`, `area_conocimiento`) VALUES (101, 1), (102, 2);
INSERT IGNORE INTO `proyecto_linea` (`proyecto`, `linea_investigacion`) VALUES (101, 1), (101, 3), (102, 2);
INSERT IGNORE INTO `ods_proyecto` (`proyecto`, `ods`) VALUES (101, 9), (102, 6), (103, 4);
INSERT IGNORE INTO `aa_proyecto` (`proyecto`, `area_aplicacion`) VALUES (101, 1), (101, 3), (102, 5);
INSERT IGNORE INTO `docente_producto` (`docente`, `producto`) VALUES (71000101, 501), (98000202, 501), (71000101, 502);

INSERT IGNORE INTO `rol` (`id`, `nombre`, `descripcion`) VALUES
(1, 'Administrador', 'Control total del sistema incluyendo usuarios, roles y catalogos.'),
(2, 'Investigador', 'Puede consultar y gestionar proyectos, docentes, productos y asociaciones.'),
(3, 'Consulta', 'Solo lectura sobre los recursos del modulo de Mapa de Conocimiento.');

-- Nota: Al iniciar api-mapa, semilla.py garantiza los hashes BCrypt reales de:
--   admin / admin123 (Administrador)
--   investigador / inv123 (Investigador)
--   consulta / consulta123 (Consulta)
INSERT IGNORE INTO `usuario` (`id`, `username`, `password`, `email`, `nombre_completo`) VALUES
(1, 'admin', '$2a$12$PJf6LIuW8uL9q9hK0LsG0ebvUll.eLcJgg6lmTIPVk84p0fwD0T5u', 'admin@mapa.edu.co', 'Administrador General');

INSERT IGNORE INTO `rol_usuario` (`usuario_id`, `rol_id`) VALUES (1, 1);
