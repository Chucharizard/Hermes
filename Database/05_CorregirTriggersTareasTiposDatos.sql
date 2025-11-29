-- =============================================
-- Script: CORREGIR Triggers de Auditoría para TAREA
-- Base de datos: HERMES
-- Descripción: Triggers con tipos de datos correctos (INT para CI)
-- =============================================

USE HERMES;
GO

-- =============================================
-- TRIGGER: Auditoría INSERT en TAREA
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_TAREA_INSERT', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_TAREA_INSERT;
GO

CREATE TRIGGER trg_Auditoria_TAREA_INSERT
ON TAREA
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    -- Obtener el usuario actual desde el contexto de sesión
    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_TAREA (
        tarea_id,
        accion,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        i.id_tarea,
        'INSERT',
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            id_tarea,
            usuario_emisor_id,
            usuario_receptor_id,
            titulo_tarea,
            descripcion_tarea,
            estado_tarea,
            prioridad_tarea,
            fecha_inicio_tarea,
            fecha_limite_tarea,
            permite_entrega_con_retraso
         FROM inserted i2 WHERE i2.id_tarea = i.id_tarea FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría UPDATE en TAREA
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_TAREA_UPDATE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_TAREA_UPDATE;
GO

CREATE TRIGGER trg_Auditoria_TAREA_UPDATE
ON TAREA
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_TAREA (
        tarea_id,
        accion,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        i.id_tarea,
        'UPDATE',
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (
            SELECT
                (SELECT
                    id_tarea,
                    usuario_emisor_id,
                    usuario_receptor_id,
                    titulo_tarea,
                    descripcion_tarea,
                    estado_tarea,
                    prioridad_tarea,
                    fecha_inicio_tarea,
                    fecha_limite_tarea,
                    fecha_completada_tarea,
                    permite_entrega_con_retraso
                 FROM deleted d2 WHERE d2.id_tarea = i.id_tarea FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS antes,
                (SELECT
                    id_tarea,
                    usuario_emisor_id,
                    usuario_receptor_id,
                    titulo_tarea,
                    descripcion_tarea,
                    estado_tarea,
                    prioridad_tarea,
                    fecha_inicio_tarea,
                    fecha_limite_tarea,
                    fecha_completada_tarea,
                    permite_entrega_con_retraso
                 FROM inserted i2 WHERE i2.id_tarea = i.id_tarea FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS despues
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i;
END
GO

-- =============================================
-- TRIGGER: Auditoría DELETE en TAREA
-- =============================================
IF OBJECT_ID('dbo.trg_Auditoria_TAREA_DELETE', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_TAREA_DELETE;
GO

CREATE TRIGGER trg_Auditoria_TAREA_DELETE
ON TAREA
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId UNIQUEIDENTIFIER;
    DECLARE @CiModificador INT;
    DECLARE @NombreMaquina NVARCHAR(100) = HOST_NAME();

    SELECT @UsuarioId = CAST(SESSION_CONTEXT(N'UsuarioId') AS UNIQUEIDENTIFIER);
    SELECT @CiModificador = CAST(SESSION_CONTEXT(N'CiEmpleado') AS INT);

    INSERT INTO AUDITORIA_TAREA (
        tarea_id,
        accion,
        usuario_id_modificador,
        ci_modificador,
        fecha_hora,
        nombre_maquina,
        detalles
    )
    SELECT
        d.id_tarea,
        'DELETE',
        @UsuarioId,
        @CiModificador,
        GETDATE(),
        @NombreMaquina,
        (SELECT
            id_tarea,
            usuario_emisor_id,
            usuario_receptor_id,
            titulo_tarea,
            descripcion_tarea,
            estado_tarea,
            prioridad_tarea,
            fecha_inicio_tarea,
            fecha_limite_tarea,
            fecha_completada_tarea,
            permite_entrega_con_retraso
         FROM deleted d2 WHERE d2.id_tarea = d.id_tarea FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END
GO

PRINT 'Triggers de TAREA actualizados con tipos de datos correctos (INT para CI)';
