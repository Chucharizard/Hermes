-- =============================================
-- Script: Limpiar datos antiguos de tablas de auditoría
-- Base de datos: HERMES
-- Descripción: Elimina registros con tipos de datos incorrectos
-- =============================================

USE HERMES;
GO

-- Eliminar todos los registros de las tablas de auditoría
-- (tienen datos con tipos incorrectos de antes de la corrección)

PRINT 'Limpiando tablas de auditoría...';

-- Limpiar AUDITORIA_SESION
DELETE FROM AUDITORIA_SESION;
PRINT 'Tabla AUDITORIA_SESION limpiada';

-- Limpiar AUDITORIA_EMPLEADO_USUARIO
DELETE FROM AUDITORIA_EMPLEADO_USUARIO;
PRINT 'Tabla AUDITORIA_EMPLEADO_USUARIO limpiada';

-- Limpiar AUDITORIA_TAREA
DELETE FROM AUDITORIA_TAREA;
PRINT 'Tabla AUDITORIA_TAREA limpiada';

PRINT '';
PRINT '===========================================';
PRINT 'Limpieza completada exitosamente';
PRINT 'Las tablas están listas para recibir nuevos registros con tipos correctos';
PRINT '===========================================';
