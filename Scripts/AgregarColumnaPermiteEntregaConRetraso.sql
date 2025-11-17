-- Script para agregar la columna permite_entrega_con_retraso a la tabla TAREA
-- Fecha: 2025-01-17
-- Descripción: Agrega configuración de cierre de tarea (estilo Microsoft Teams)

USE HERMES;
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TAREA'
    AND COLUMN_NAME = 'permite_entrega_con_retraso'
)
BEGIN
    -- Agregar la columna con valor por defecto
    ALTER TABLE TAREA
    ADD permite_entrega_con_retraso BIT NOT NULL DEFAULT 1;

    PRINT 'Columna permite_entrega_con_retraso agregada exitosamente a la tabla TAREA';
END
ELSE
BEGIN
    PRINT 'La columna permite_entrega_con_retraso ya existe en la tabla TAREA';
END
GO

-- Verificar que la columna se agregó correctamente
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TAREA'
AND COLUMN_NAME = 'permite_entrega_con_retraso';
GO
