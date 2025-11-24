-- ========================================
-- Script para agregar columna tema_preferido
-- a la tabla USUARIO
-- ========================================

USE HERMES;
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'USUARIO'
    AND COLUMN_NAME = 'tema_preferido'
)
BEGIN
    -- Agregar columna tema_preferido con valor por defecto 'Emerald'
    ALTER TABLE USUARIO
    ADD tema_preferido NVARCHAR(50) NOT NULL DEFAULT 'Emerald';

    PRINT 'Columna tema_preferido agregada exitosamente a la tabla USUARIO';
END
ELSE
BEGIN
    PRINT 'La columna tema_preferido ya existe en la tabla USUARIO';
END
GO
