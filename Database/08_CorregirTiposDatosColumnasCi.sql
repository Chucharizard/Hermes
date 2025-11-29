-- =============================================
-- Script: Corregir tipos de datos en tablas AUDITORIA_EMPLEADO_USUARIO y AUDITORIA_TAREA
-- Base de datos: HERMES
-- Descripción: Cambia VARCHAR(20) a INT para columnas CI
-- =============================================

USE HERMES;
GO

PRINT '===========================================';
PRINT 'CORRIGIENDO TIPOS DE DATOS DE COLUMNAS CI';
PRINT '===========================================';
PRINT '';

-- =============================================
-- 1. CORREGIR AUDITORIA_EMPLEADO_USUARIO
-- =============================================
PRINT 'Corrigiendo AUDITORIA_EMPLEADO_USUARIO...';

-- Cambiar ci_empleado_afectado de VARCHAR(20) a INT
ALTER TABLE AUDITORIA_EMPLEADO_USUARIO
ALTER COLUMN ci_empleado_afectado INT;
PRINT '  ✓ ci_empleado_afectado: VARCHAR(20) → INT';

-- Cambiar ci_modificador de VARCHAR(20) a INT
ALTER TABLE AUDITORIA_EMPLEADO_USUARIO
ALTER COLUMN ci_modificador INT;
PRINT '  ✓ ci_modificador: VARCHAR(20) → INT';

PRINT '';

-- =============================================
-- 2. CORREGIR AUDITORIA_TAREA
-- =============================================
PRINT 'Corrigiendo AUDITORIA_TAREA...';

-- Cambiar ci_modificador de VARCHAR(20) a INT
ALTER TABLE AUDITORIA_TAREA
ALTER COLUMN ci_modificador INT;
PRINT '  ✓ ci_modificador: VARCHAR(20) → INT';

PRINT '';

-- =============================================
-- 3. VERIFICAR CAMBIOS
-- =============================================
PRINT '===========================================';
PRINT 'VERIFICANDO TIPOS DE DATOS CORREGIDOS';
PRINT '===========================================';
PRINT '';

SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('AUDITORIA_SESION', 'AUDITORIA_EMPLEADO_USUARIO', 'AUDITORIA_TAREA')
    AND COLUMN_NAME LIKE '%ci%'
ORDER BY TABLE_NAME, ORDINAL_POSITION;

PRINT '';
PRINT '===========================================';
PRINT '✓✓✓ CORRECCIÓN COMPLETADA ✓✓✓';
PRINT '===========================================';
PRINT 'Todas las columnas CI ahora son tipo INT';
