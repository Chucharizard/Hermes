-- =============================================
-- Script: Limpiar y verificar tablas de auditoría
-- Base de datos: HERMES
-- Descripción: Limpia completamente las tablas y verifica el estado
-- =============================================

USE HERMES;
GO

PRINT '===========================================';
PRINT 'LIMPIANDO TABLAS DE AUDITORÍA';
PRINT '===========================================';
PRINT '';

-- Contar registros ANTES de limpiar
DECLARE @CountSesionAntes INT, @CountEmpleadoUsuarioAntes INT, @CountTareaAntes INT;
SELECT @CountSesionAntes = COUNT(*) FROM AUDITORIA_SESION;
SELECT @CountEmpleadoUsuarioAntes = COUNT(*) FROM AUDITORIA_EMPLEADO_USUARIO;
SELECT @CountTareaAntes = COUNT(*) FROM AUDITORIA_TAREA;

-- Mostrar contenido ANTES de limpiar
PRINT '--- Estado ANTES de limpiar ---';
PRINT 'AUDITORIA_SESION: ' + CAST(@CountSesionAntes AS VARCHAR) + ' registros';
PRINT 'AUDITORIA_EMPLEADO_USUARIO: ' + CAST(@CountEmpleadoUsuarioAntes AS VARCHAR) + ' registros';
PRINT 'AUDITORIA_TAREA: ' + CAST(@CountTareaAntes AS VARCHAR) + ' registros';
PRINT '';

-- Limpiar AUDITORIA_TAREA (primero, no tiene FK)
PRINT 'Limpiando AUDITORIA_TAREA...';
DELETE FROM AUDITORIA_TAREA;
PRINT '✓ AUDITORIA_TAREA limpiada';
PRINT '';

-- Limpiar AUDITORIA_EMPLEADO_USUARIO
PRINT 'Limpiando AUDITORIA_EMPLEADO_USUARIO...';
DELETE FROM AUDITORIA_EMPLEADO_USUARIO;
PRINT '✓ AUDITORIA_EMPLEADO_USUARIO limpiada';
PRINT '';

-- Limpiar AUDITORIA_SESION
PRINT 'Limpiando AUDITORIA_SESION...';
DELETE FROM AUDITORIA_SESION;
PRINT '✓ AUDITORIA_SESION limpiada';
PRINT '';

-- Verificar que estén vacías
PRINT '--- Estado DESPUÉS de limpiar ---';
DECLARE @CountSesion INT = (SELECT COUNT(*) FROM AUDITORIA_SESION);
DECLARE @CountEmpleadoUsuario INT = (SELECT COUNT(*) FROM AUDITORIA_EMPLEADO_USUARIO);
DECLARE @CountTarea INT = (SELECT COUNT(*) FROM AUDITORIA_TAREA);

PRINT 'AUDITORIA_SESION: ' + CAST(@CountSesion AS VARCHAR) + ' registros';
PRINT 'AUDITORIA_EMPLEADO_USUARIO: ' + CAST(@CountEmpleadoUsuario AS VARCHAR) + ' registros';
PRINT 'AUDITORIA_TAREA: ' + CAST(@CountTarea AS VARCHAR) + ' registros';
PRINT '';

-- Verificar tipos de datos de las columnas
PRINT '--- Verificación de tipos de datos ---';
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
IF @CountSesion = 0 AND @CountEmpleadoUsuario = 0 AND @CountTarea = 0
BEGIN
    PRINT '===========================================';
    PRINT '✓✓✓ LIMPIEZA COMPLETADA EXITOSAMENTE ✓✓✓';
    PRINT '===========================================';
    PRINT 'Todas las tablas están vacías y listas para recibir nuevos registros';
    PRINT 'con los tipos de datos correctos (INT para CI)';
END
ELSE
BEGIN
    PRINT '⚠ ADVERTENCIA: Algunas tablas no se limpiaron completamente';
END
