-- Script para agregar el rol "Abogada" al sistema HERMES
-- Ejecutar este script en la base de datos

USE HermesDB;
GO

-- Verificar si el rol ya existe
IF NOT EXISTS (SELECT 1 FROM ROL WHERE nombre_rol = 'Abogada')
BEGIN
    -- Insertar el rol Abogada
    INSERT INTO ROL (nombre_rol, descripcion_rol, es_activo_rol)
    VALUES ('Abogada', 'Asesor legal de transacciones inmobiliarias', 1);

    PRINT 'Rol "Abogada" insertado exitosamente';
END
ELSE
BEGIN
    PRINT 'El rol "Abogada" ya existe en la base de datos';
END
GO

-- Verificar la inserción
SELECT * FROM ROL WHERE nombre_rol = 'Abogada';
GO
