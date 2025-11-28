-- =============================================
-- Script: Corregir tipos de datos en AUDITORIA_SESION
-- Base de datos: HERMES
-- Descripción: Elimina y recrea AUDITORIA_SESION con tipos de datos correctos
-- =============================================

USE HERMES;
GO

-- Eliminar tabla AUDITORIA_SESION si existe
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AUDITORIA_SESION')
BEGIN
    DROP TABLE AUDITORIA_SESION;
    PRINT 'Tabla AUDITORIA_SESION eliminada';
END
GO

-- Recrear tabla AUDITORIA_SESION con tipo de datos correcto (INT en lugar de VARCHAR)
CREATE TABLE AUDITORIA_SESION (
    id_auditoria_sesion UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    usuario_id UNIQUEIDENTIFIER NOT NULL,
    ci_empleado INT NOT NULL,  -- Cambiado de VARCHAR(20) a INT
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

PRINT 'Tabla AUDITORIA_SESION recreada con tipos de datos correctos';
GO
