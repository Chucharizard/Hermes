-- =============================================
-- Script: Triggers de Auditoría para EMPLEADO y USUARIO
-- Base de datos: HERMES
-- Descripción: Triggers automáticos para capturar INSERT, UPDATE, DELETE
-- =============================================

USE HERMES;
GO

-- =============================================
-- FUNCIÓN AUXILIAR: Obtener nombre de máquina actual
-- =============================================
IF OBJECT_ID('dbo.fn_ObtenerNombreMaquina', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_ObtenerNombreMaquina;
GO

CREATE FUNCTION dbo.fn_ObtenerNombreMaquina()
RETURNS NVARCHAR(100)
AS
BEGIN
    RETURN CAST(CONNECTIONPROPERTY('client_net_address') AS NVARCHAR(100));
END
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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    -- Intentar obtener el usuario actual desde el contexto de sesión
    -- (Esto se establecerá desde la aplicación C#)
    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

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
        (SELECT
            (SELECT * FROM deleted d WHERE d.ci_empleado = i.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Anterior,
            (SELECT * FROM inserted i2 WHERE i2.ci_empleado = i.ci_empleado FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Nuevo
         FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        ci_empleado_afectado,
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
        i.empleado_ci,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            id_usuario, empleado_ci, rol_id, nombre_usuario, es_activo_usuario, tema_preferido
         FROM inserted i2 WHERE i2.id_usuario = i.id_usuario FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        ci_empleado_afectado,
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
        i.empleado_ci,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            (SELECT id_usuario, empleado_ci, rol_id, nombre_usuario, es_activo_usuario, tema_preferido
             FROM deleted d WHERE d.id_usuario = i.id_usuario FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Anterior,
            (SELECT id_usuario, empleado_ci, rol_id, nombre_usuario, es_activo_usuario, tema_preferido
             FROM inserted i2 WHERE i2.id_usuario = i.id_usuario FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS Nuevo
         FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
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
    DECLARE @CiModificador VARCHAR(20);
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS VARCHAR(20));

    INSERT INTO AUDITORIA_EMPLEADO_USUARIO (
        tabla_afectada,
        accion,
        usuario_id_afectado,
        ci_empleado_afectado,
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
        d.empleado_ci,
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT id_usuario, empleado_ci, rol_id, nombre_usuario, es_activo_usuario, tema_preferido
         FROM deleted d2 WHERE d2.id_usuario = d.id_usuario FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END
GO

PRINT 'Triggers de auditoría para EMPLEADO y USUARIO creados exitosamente';
