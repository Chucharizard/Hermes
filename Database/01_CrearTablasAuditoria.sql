-- =============================================
-- Script: Crear Tablas de Auditoría
-- Base de datos: HERMES
-- Descripción: Tablas para auditoría de sesiones, empleados/usuarios y tareas
-- =============================================

USE HERMES;
GO

-- =============================================
-- 1. TABLA DE AUDITORÍA DE SESIONES (LOGIN/LOGOUT)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AUDITORIA_SESION')
BEGIN
    CREATE TABLE AUDITORIA_SESION (
        id_auditoria_sesion UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        usuario_id UNIQUEIDENTIFIER NOT NULL,
        ci_empleado INT NOT NULL,
        fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
        nombre_maquina NVARCHAR(100) NOT NULL,
        tipo_evento NVARCHAR(20) NOT NULL CHECK (tipo_evento IN ('LOGIN', 'LOGOUT')),

        -- Foreign Keys
        CONSTRAINT FK_AUDITORIA_SESION_USUARIO FOREIGN KEY (usuario_id)
            REFERENCES USUARIO(id_usuario) ON DELETE NO ACTION,
        CONSTRAINT FK_AUDITORIA_SESION_EMPLEADO FOREIGN KEY (ci_empleado)
            REFERENCES EMPLEADO(ci_empleado) ON DELETE NO ACTION
    );

    -- Índices para mejorar performance en consultas
    CREATE INDEX IX_AUDITORIA_SESION_FECHA ON AUDITORIA_SESION(fecha_hora DESC);
    CREATE INDEX IX_AUDITORIA_SESION_USUARIO ON AUDITORIA_SESION(usuario_id);
    CREATE INDEX IX_AUDITORIA_SESION_CI ON AUDITORIA_SESION(ci_empleado);

    PRINT 'Tabla AUDITORIA_SESION creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla AUDITORIA_SESION ya existe';
END
GO

-- =============================================
-- 2. TABLA DE AUDITORÍA DE EMPLEADOS Y USUARIOS (CRUD)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AUDITORIA_EMPLEADO_USUARIO')
BEGIN
    CREATE TABLE AUDITORIA_EMPLEADO_USUARIO (
        id_auditoria UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tabla_afectada NVARCHAR(50) NOT NULL CHECK (tabla_afectada IN ('EMPLEADO', 'USUARIO')),
        accion NVARCHAR(10) NOT NULL CHECK (accion IN ('INSERT', 'UPDATE', 'DELETE')),
        ci_empleado_afectado INT,
        usuario_id_afectado UNIQUEIDENTIFIER,
        usuario_id_modificador UNIQUEIDENTIFIER,
        ci_modificador INT,
        fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
        nombre_maquina NVARCHAR(100) NOT NULL,
        detalles NVARCHAR(MAX), -- JSON con los cambios

        -- Foreign Keys opcionales (pueden ser NULL en caso de DELETE)
        CONSTRAINT FK_AUDITORIA_EMP_USU_MODIFICADOR FOREIGN KEY (usuario_id_modificador)
            REFERENCES USUARIO(id_usuario) ON DELETE NO ACTION
    );

    -- Índices
    CREATE INDEX IX_AUDITORIA_EMP_USU_FECHA ON AUDITORIA_EMPLEADO_USUARIO(fecha_hora DESC);
    CREATE INDEX IX_AUDITORIA_EMP_USU_TABLA ON AUDITORIA_EMPLEADO_USUARIO(tabla_afectada);
    CREATE INDEX IX_AUDITORIA_EMP_USU_ACCION ON AUDITORIA_EMPLEADO_USUARIO(accion);
    CREATE INDEX IX_AUDITORIA_EMP_USU_MODIFICADOR ON AUDITORIA_EMPLEADO_USUARIO(usuario_id_modificador);

    PRINT 'Tabla AUDITORIA_EMPLEADO_USUARIO creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla AUDITORIA_EMPLEADO_USUARIO ya existe';
END
GO

-- =============================================
-- 3. TABLA DE AUDITORÍA DE TAREAS (CRUD)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AUDITORIA_TAREA')
BEGIN
    CREATE TABLE AUDITORIA_TAREA (
        id_auditoria UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tarea_id UNIQUEIDENTIFIER,
        accion NVARCHAR(10) NOT NULL CHECK (accion IN ('INSERT', 'UPDATE', 'DELETE')),
        usuario_id_modificador UNIQUEIDENTIFIER,
        ci_modificador INT,
        fecha_hora DATETIME NOT NULL DEFAULT GETDATE(),
        nombre_maquina NVARCHAR(100) NOT NULL,
        detalles NVARCHAR(MAX), -- JSON con los cambios

        -- Foreign Keys opcionales
        CONSTRAINT FK_AUDITORIA_TAREA_MODIFICADOR FOREIGN KEY (usuario_id_modificador)
            REFERENCES USUARIO(id_usuario) ON DELETE NO ACTION
    );

    -- Índices
    CREATE INDEX IX_AUDITORIA_TAREA_FECHA ON AUDITORIA_TAREA(fecha_hora DESC);
    CREATE INDEX IX_AUDITORIA_TAREA_ACCION ON AUDITORIA_TAREA(accion);
    CREATE INDEX IX_AUDITORIA_TAREA_MODIFICADOR ON AUDITORIA_TAREA(usuario_id_modificador);
    CREATE INDEX IX_AUDITORIA_TAREA_TAREA_ID ON AUDITORIA_TAREA(tarea_id);

    PRINT 'Tabla AUDITORIA_TAREA creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla AUDITORIA_TAREA ya existe';
END
GO

PRINT 'Script de creación de tablas de auditoría completado';
