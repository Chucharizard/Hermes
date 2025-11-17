# ?? Guía de Verificación de Contraseñas - Sistema HERMES

## ?? Problema Común: Contraseña Incorrecta

Si tienes problemas para iniciar sesión, sigue estos pasos:

---

## ? PASO 1: Verificar Hash SHA256

### Contraseñas de Prueba y sus Hashes:

```
admin123 ? 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
broker123 ? 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
password123 ? ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f
```

---

## ? PASO 2: Script SQL para Actualizar/Verificar Usuario

Ejecuta este script en SQL Server Management Studio:

```sql
USE HERMES;
GO

-- Ver usuarios actuales
SELECT 
    u.id_usuario,
    u.empleado_ci,
    e.nombres_empleado,
    e.apellidos_empleado,
    u.nombre_usuario,
    u.password_usuario,
    r.nombre_rol,
    u.es_activo_usuario
FROM USUARIO u
INNER JOIN EMPLEADO e ON u.empleado_ci = e.ci_empleado
INNER JOIN ROL r ON u.rol_id = r.id_rol;
GO

-- OPCIÓN A: Actualizar contraseña a "admin123"
UPDATE USUARIO
SET password_usuario = '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9'
WHERE empleado_ci = 12345678;
GO

-- OPCIÓN B: Actualizar contraseña a "broker123"
UPDATE USUARIO
SET password_usuario = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'
WHERE empleado_ci = 12345678;
GO

-- Verificar actualización
SELECT 
    empleado_ci,
    nombre_usuario,
    password_usuario,
    LEN(password_usuario) as longitud_hash
FROM USUARIO
WHERE empleado_ci = 12345678;
GO
```

---

## ? PASO 3: Verificar en la Aplicación

### Método 1: Usar el Método de Verificación

En `LoginViewModel.cs`, descomenta temporalmente:

```csharp
private async Task LoginAsync()
{
    // TEMPORAL - Descomentar para probar
    var autenticacionService = new AutenticacionService();
    autenticacionService.VerificarHash("admin123", "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9");
    return;
    
    // ... resto del código
}
```

Esto mostrará una ventana con:
- La contraseña que probaste
- El hash generado por tu aplicación
- El hash esperado de la BD
- Si coinciden o no

---

## ? PASO 4: Crear Usuario desde Cero

Si nada funciona, crea un usuario nuevo con este script:

```sql
USE HERMES;
GO

-- 1. Verificar que existe el empleado
SELECT * FROM EMPLEADO WHERE ci_empleado = 12345678;
GO

-- 2. Obtener ID del rol Broker
DECLARE @RolBrokerId UNIQUEIDENTIFIER;
SELECT @RolBrokerId = id_rol FROM ROL WHERE nombre_rol = 'Broker';

-- 3. Eliminar usuario anterior si existe
DELETE FROM USUARIO WHERE empleado_ci = 12345678;
GO

-- 4. Crear nuevo usuario con contraseña "admin123"
DECLARE @RolBrokerId UNIQUEIDENTIFIER;
SELECT @RolBrokerId = id_rol FROM ROL WHERE nombre_rol = 'Broker';

INSERT INTO USUARIO (empleado_ci, rol_id, nombre_usuario, password_usuario, es_activo_usuario)
VALUES (
    12345678, 
    @RolBrokerId, 
    'admin', 
    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9',
    1
);
GO

-- 5. Verificar creación
SELECT 
    u.id_usuario,
    u.empleado_ci,
    e.nombres_empleado + ' ' + e.apellidos_empleado as empleado,
    u.nombre_usuario,
    u.password_usuario,
    r.nombre_rol,
    u.es_activo_usuario
FROM USUARIO u
INNER JOIN EMPLEADO e ON u.empleado_ci = e.ci_empleado
INNER JOIN ROL r ON u.rol_id = r.id_rol
WHERE u.empleado_ci = 12345678;
GO
```

---

## ? PASO 5: Verificar Conexión a Base de Datos

Asegúrate de que la cadena de conexión es correcta:

**Archivos a verificar:**
1. `Data/HermesDbContext.cs` (línea 14)
2. `Data/DatabaseHelper.cs` (línea 7)
3. `App.config`

**Cadena de conexión actual:**
```
Server=PANADERO\PANCITO;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;
```

---

## ?? PASO 6: Debugging en Código

### Agregar logging temporal en AutenticacionService.cs:

```csharp
public async Task<(bool Success, Usuario? Usuario, string Mensaje)> ValidarCredencialesAsync(int ciEmpleado, string password)
{
    try
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Empleado)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.EmpleadoCi == ciEmpleado && u.EsActivoUsuario);

        if (usuario == null)
        {
            MessageBox.Show($"No se encontró usuario con CI: {ciEmpleado}", "Debug");
            return (false, null, "Usuario no encontrado o inactivo");
        }

        // Hashear la contraseña ingresada
        string passwordHash = HashPassword(password);

        // IMPORTANTE: Limpiar espacios y convertir a minúsculas para comparación
        string hashBD = usuario.PasswordUsuario?.Trim().ToLower() ?? string.Empty;
        string hashGenerado = passwordHash?.Trim().ToLower() ?? string.Empty;

        // DEBUG - Mostrar hashes
        MessageBox.Show(
            $"Hash en BD: {hashBD}\n\n" +
            $"Hash generado: {hashGenerado}\n\n" +
            $"¿Coinciden?: {hashBD == hashGenerado}",
            "Debug Hashes");

        // Comparar hashes
        if (hashBD != hashGenerado)
        {
            return (false, null, "Contraseña incorrecta");
        }

        return (true, usuario, "Autenticación exitosa");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "Error");
        return (false, null, $"Error de autenticación: {ex.Message}");
    }
}
```

---

## ?? Checklist de Verificación

- [ ] La base de datos HERMES existe
- [ ] La tabla USUARIO tiene registros
- [ ] El empleado con CI 12345678 existe en EMPLEADO
- [ ] El rol "Broker" existe en ROL
- [ ] El hash de la contraseña tiene exactamente 64 caracteres
- [ ] El hash está en minúsculas
- [ ] El usuario está activo (es_activo_usuario = 1)
- [ ] La cadena de conexión es correcta
- [ ] El servidor SQL está ejecutándose
- [ ] No hay espacios extra en el hash de la BD

---

## ?? Credenciales de Prueba Confirmadas

Después de ejecutar los scripts:

**CI:** `12345678`  
**Usuario:** `admin`  
**Contraseña:** `admin123`  
**Hash SHA256:** `240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9`

---

## ?? Si Aún No Funciona

1. **Verifica en SQL Server:**
   ```sql
   SELECT @@VERSION; -- Ver versión de SQL Server
   SELECT DB_NAME(); -- Ver base de datos actual
   ```

2. **Verifica permisos:**
   - El usuario de Windows debe tener permisos en SQL Server
   - Trusted_Connection=True requiere autenticación de Windows

3. **Prueba con usuario SQL:**
   Si Trusted_Connection no funciona, usa:
   ```
   Server=PANADERO\PANCITO;Database=HERMES;User Id=sa;Password=tu_contraseña;TrustServerCertificate=True;
   ```

---

## ? Solución Rápida (Copy-Paste)

```sql
USE HERMES;
DELETE FROM USUARIO WHERE empleado_ci = 12345678;

DECLARE @RolBrokerId UNIQUEIDENTIFIER;
SELECT @RolBrokerId = id_rol FROM ROL WHERE nombre_rol = 'Broker';

INSERT INTO USUARIO (empleado_ci, rol_id, nombre_usuario, password_usuario, es_activo_usuario)
VALUES (12345678, @RolBrokerId, 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);

SELECT * FROM USUARIO WHERE empleado_ci = 12345678;
```

Luego en la app:
- CI: `12345678`
- Contraseña: `admin123`
