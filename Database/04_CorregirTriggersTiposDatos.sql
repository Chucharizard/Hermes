-- =============================================
-- Script: CORREGIR Triggers de Auditoría para EMPLEADO y USUARIO
-- Base de datos: HERMES
-- Descripción: Triggers con tipos de datos correctos (INT para CI)
-- =============================================

USE HERMES;
GO

-- =============================================
-- TRIGGER: Auditoría INSERT en EMPLEADO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_EMPLEADO_INSERT', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_EMPLEADO_INSERT;
GO

CREATE TRIGGER trg_Auditoria_EMPLEADO_INSERT
ON EMPLEADO
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    -- Obtener del contexto de sesión
    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        ci_empleado_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'EMPLEADO',
        'INSERT',
        i.ci_empleado,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT * FROM inserted i2 WHERE i2.ci_empleado = i.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría UPDATE en EMPLEADO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_EMPLEADO_UPDATE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_EMPLEADO_UPDATE;
GO

CREATE TRIGGER trg_Auditoria_EMPLEADO_UPDATE
ON EMPLEADO
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        ci_empleado_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'EMPLEADO',
        'UPDATE',
        i.ci_empleado,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (
            SELECT
                (SELECT * FROM deleted d2 WHERE d2.ci_empleado = i.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS antes,
                (SELECT * FROM inserted i2 WHERE i2.ci_empleado = i.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS despues
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría DELETE en EMPLEADO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_EMPLEADO_DELETE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_EMPLEADO_DELETE;
GO

CREATE TRIGGER trg_Auditoria_EMPLEADO_DELETE
ON EMPLEADO
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        ci_empleado_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'EMPLEADO',
        'DELETE',
        d.ci_empleado,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT * FROM deleted d2 WHERE d2.ci_empleado = d.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END
GO

-- =============================================
-- TRIGGER: Auditoría INSERT en USUARIO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_USUARIO_INSERT', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_USUARIO_INSERT;
GO

CREATE TRIGGER trg_Auditoria_USUARIO_INSERT
ON USUARIO
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'USUARIO',
        'INSERT',
        i.id_usuario,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            i2.id_usuario,
            i2.empleado_ci,
            i2.rol_id,
            i2.nombre_usuario,
            i2.es_activo_usuario,
            i2.tema_preferido
        FROM inserted i2
        WHERE i2.id_usuario = i.id_usuario
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría UPDATE en USUARIO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_USUARIO_UPDATE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_USUARIO_UPDATE;
GO

CREATE TRIGGER trg_Auditoria_USUARIO_UPDATE
ON USUARIO
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'USUARIO',
        'UPDATE',
        i.id_usuario,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (
            SELECT
                (SELECT
                    d2.id_usuario,
                    d2.empleado_ci,
                    d2.rol_id,
                    d2.nombre_usuario,
                    d2.es_activo_usuario,
                    d2.tema_preferido
                FROM deleted d2
                WHERE d2.id_usuario = i.id_usuario
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS antes,
                (SELECT
                    i2.id_usuario,
                    i2.empleado_ci,
                    i2.rol_id,
                    i2.nombre_usuario,
                    i2.es_activo_usuario,
                    i2.tema_preferido
                FROM inserted i2
                WHERE i2.id_usuario = i.id_usuario
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS despues
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría DELETE en USUARIO
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_USUARIO_DELETE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_USUARIO_DELETE;
GO

CREATE TRIGGER trg_Auditoria_USUARIO_DELETE
ON USUARIO
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        'USUARIO',
        'DELETE',
        d.id_usuario,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            d2.id_usuario,
            d2.empleado_ci,
            d2.rol_id,
            d2.nombre_usuario,
            d2.es_activo_usuario,
            d2.tema_preferido
        FROM deleted d2
        WHERE d2.id_usuario = d.id_usuario
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END
GO

PRINT 'Triggers de EMPLEADO y USUARIO actualizados con tipos de datos correctos (INT para CI)';
