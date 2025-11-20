-- Script para aumentar el límite de descripcion_tarea de NVARCHAR(500) a NVARCHAR(MAX)
-- Ejecutar este script en SQL Server Management Studio o mediante query directa

USE HERMES;
GO

-- Verificar el tipo actual de la columna
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TAREA'
  AND COLUMN_NAME = 'descripcion_tarea';
GO

-- Alterar la columna para permitir textos largos
ALTER TABLE TAREA
ALTER COLUMN descripcion_tarea NVARCHAR(MAX);
GO

-- Verificar el cambio
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TAREA'
  AND COLUMN_NAME = 'descripcion_tarea';
GO

PRINT 'Columna descripcion_tarea actualizada exitosamente a NVARCHAR(MAX)';
