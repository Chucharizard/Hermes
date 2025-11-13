-- ============================================================ -- CREACIÓN DE BASE DE DATOS -- ============================================================ 
CREATE DATABASE HERMES; 
GO 
USE HERMES; 
GO -- ============================================================ -- TABLAS SIN DEPENDENCIAS (SIN FK) -- ============================================================ -- 1. TABLA: EMPLEADO 
CREATE TABLE EMPLEADO ( 
ci_empleado INT PRIMARY KEY, 
nombres_empleado VARCHAR(100) NOT NULL, 
apellidos_empleado VARCHAR(100) NOT NULL, 
telefono_empleado VARCHAR(20), 
correo_empleado VARCHAR(320), 
es_activo_empleado BIT NOT NULL DEFAULT 1 
); 
GO -- 2. TABLA: ROL 
CREATE TABLE ROL ( 
id_rol UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
nombre_rol VARCHAR(50) NOT NULL, 
descripcion_rol VARCHAR(300), 
es_activo_rol BIT NOT NULL DEFAULT 1 
); 
GO -- 3. TABLA: TIPO_TRAMITE 
CREATE TABLE TIPO_TRAMITE ( 
id_tipo_tramite UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
nombre_tipo_tramite VARCHAR(100) NOT NULL, 
descripcion_tipo_tramite VARCHAR(300), 
fecha_creacion_tipo_tramite DATETIME2 NOT NULL DEFAULT SYSDATETIME() 
); 
GO -- ============================================================ -- TABLAS CON DEPENDENCIAS (CON FK) -- ============================================================ -- 4. TABLA: USUARIO 
CREATE TABLE USUARIO ( 
id_usuario UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
empleado_ci INT NOT NULL, 
rol_id UNIQUEIDENTIFIER NOT NULL, 
nombre_usuario VARCHAR(50) NOT NULL, 
password_usuario VARCHAR(255) NOT NULL, 
es_activo_usuario BIT NOT NULL DEFAULT 1, 
CONSTRAINT FK_USUARIO_EMPLEADO FOREIGN KEY (empleado_ci) REFERENCES 
EMPLEADO(ci_empleado), 
CONSTRAINT FK_USUARIO_ROL FOREIGN KEY (rol_id) REFERENCES ROL(id_rol) 
); 
GO -- 5. TABLA: HOJA_RUTA 
CREATE TABLE HOJA_RUTA ( 
id_hoja_ruta UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
tipo_tramite_id UNIQUEIDENTIFIER NOT NULL, 
usuario_id UNIQUEIDENTIFIER NOT NULL, 
titulo_hoja_ruta VARCHAR(150) NOT NULL, 
estado_hoja_ruta VARCHAR(50) NOT NULL, 
fecha_inicio_hoja_ruta DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
fecha_fin_hoja_ruta DATETIME2 NULL, 
CONSTRAINT FK_HOJA_RUTA_TIPO FOREIGN KEY (tipo_tramite_id) REFERENCES 
TIPO_TRAMITE(id_tipo_tramite), 
CONSTRAINT FK_HOJA_RUTA_USUARIO FOREIGN KEY (usuario_id) REFERENCES 
USUARIO(id_usuario) 
); 
GO -- 6. TABLA: HOJA_RUTA_PASO 
CREATE TABLE HOJA_RUTA_PASO ( 
id_hoja_ruta_paso UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
hoja_ruta_id UNIQUEIDENTIFIER NOT NULL, 
usuario_emisor_id UNIQUEIDENTIFIER NOT NULL, 
usuario_receptor_id UNIQUEIDENTIFIER NOT NULL, 
numero_paso_hoja_ruta_paso INT NOT NULL, 
estado_hoja_ruta_paso VARCHAR(50) NOT NULL, 
fecha_envio_hoja_ruta_paso DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
fecha_completado_hoja_ruta_paso DATETIME2 NULL, 
CONSTRAINT FK_PASO_HOJA_RUTA FOREIGN KEY (hoja_ruta_id) REFERENCES 
HOJA_RUTA(id_hoja_ruta), 
CONSTRAINT FK_PASO_EMISOR FOREIGN KEY (usuario_emisor_id) REFERENCES 
USUARIO(id_usuario), 
CONSTRAINT FK_PASO_RECEPTOR FOREIGN KEY (usuario_receptor_id) 
REFERENCES USUARIO(id_usuario) 
); 
GO -- 7. TABLA: HOJA_RUTA_ADJUNTO 
CREATE TABLE HOJA_RUTA_ADJUNTO ( 
id_hoja_ruta_adjunto UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
hoja_ruta_id UNIQUEIDENTIFIER NOT NULL, 
nombre_archivo_hoja_ruta_adjunto VARCHAR(150) NOT NULL, 
tipo_archivo_hoja_ruta_adjunto VARCHAR(50) NOT NULL, 
archivo_hoja_ruta_adjunto VARBINARY(MAX) NULL, 
fecha_subida_hoja_ruta_adjunto DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
CONSTRAINT FK_HOJA_RUTA_ADJUNTO FOREIGN KEY (hoja_ruta_id) REFERENCES 
HOJA_RUTA(id_hoja_ruta) 
); 
GO 
-- 8. TABLA: PASO_OBSERVACION 
CREATE TABLE PASO_OBSERVACION ( 
id_paso_observacion UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
hoja_ruta_paso_id UNIQUEIDENTIFIER NOT NULL, 
usuario_autor_id UNIQUEIDENTIFIER NOT NULL, 
observacion_paso_observacion VARCHAR(500) NOT NULL, 
fecha_paso_observacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
CONSTRAINT FK_OBSERVACION_PASO FOREIGN KEY (hoja_ruta_paso_id) 
REFERENCES HOJA_RUTA_PASO(id_hoja_ruta_paso), 
CONSTRAINT FK_OBSERVACION_AUTOR FOREIGN KEY (usuario_autor_id) 
REFERENCES USUARIO(id_usuario) 
); 
GO -- 9. TABLA: TAREA 
CREATE TABLE TAREA ( 
id_tarea UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
usuario_emisor_id UNIQUEIDENTIFIER NOT NULL, 
usuario_receptor_id UNIQUEIDENTIFIER NOT NULL, 
titulo_tarea VARCHAR(150) NOT NULL, 
descripcion_tarea VARCHAR(500) NULL, 
estado_tarea VARCHAR(50) NOT NULL, 
prioridad_tarea VARCHAR(20) NOT NULL, 
fecha_inicio_tarea DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
fecha_limite_tarea DATETIME2 NULL, 
fecha_completada_tarea DATETIME2 NULL, 
CONSTRAINT FK_TAREA_EMISOR FOREIGN KEY (usuario_emisor_id) REFERENCES 
USUARIO(id_usuario), 
CONSTRAINT FK_TAREA_RECEPTOR FOREIGN KEY (usuario_receptor_id) 
REFERENCES USUARIO(id_usuario) 
); 
GO -- 10. TABLA: TAREA_COMENTARIO 
CREATE TABLE TAREA_COMENTARIO ( 
id_tarea_comentario UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
tarea_id UNIQUEIDENTIFIER NOT NULL, 
usuario_autor_id UNIQUEIDENTIFIER NOT NULL, 
comentario_tarea_comentario VARCHAR(500) NOT NULL, 
fecha_tarea_comentario DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
CONSTRAINT FK_COMENTARIO_TAREA FOREIGN KEY (tarea_id) REFERENCES 
TAREA(id_tarea), 
CONSTRAINT FK_COMENTARIO_AUTOR FOREIGN KEY (usuario_autor_id) 
REFERENCES USUARIO(id_usuario) 
); 
GO -- 11. TABLA: TAREA_ADJUNTO 
CREATE TABLE TAREA_ADJUNTO ( 
id_tarea_adjunto UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
tarea_id UNIQUEIDENTIFIER NOT NULL, 
nombre_archivo_tarea_adjunto VARCHAR(150) NOT NULL, 
tipo_archivo_tarea_adjunto VARCHAR(50) NOT NULL, 
archivo_tarea_adjunto VARBINARY(MAX) NULL, 
fecha_subida_tarea_adjunto DATETIME2 NOT NULL DEFAULT SYSDATETIME(), 
CONSTRAINT FK_TAREA_ADJUNTO FOREIGN KEY (tarea_id) REFERENCES 
TAREA(id_tarea) 
); 
GO -- 12. TABLA: TIPO_TRAMITE_PASO 
CREATE TABLE TIPO_TRAMITE_PASO ( 
id_tipo_tramite_paso UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
tipo_tramite_id UNIQUEIDENTIFIER NOT NULL, 
rol_emisor_id UNIQUEIDENTIFIER NOT NULL, 
rol_receptor_id UNIQUEIDENTIFIER NOT NULL, 
orden_tipo_tramite_paso INT NOT NULL, 
descripcion_tipo_tramite_paso VARCHAR(300) NULL, 
CONSTRAINT FK_TIPO_PASO_TIPO FOREIGN KEY (tipo_tramite_id) REFERENCES 
TIPO_TRAMITE(id_tipo_tramite), 
CONSTRAINT FK_TIPO_PASO_EMISOR FOREIGN KEY (rol_emisor_id) REFERENCES 
ROL(id_rol), 
CONSTRAINT FK_TIPO_PASO_RECEPTOR FOREIGN KEY (rol_receptor_id) 
REFERENCES ROL(id_rol) 
); 
GO