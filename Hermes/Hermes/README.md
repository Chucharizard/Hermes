# Sistema HERMES - Configuración

## ?? Descripción
Sistema de gestión inmobiliaria con autenticación de usuarios basada en roles y gestión completa de empleados.

## ??? Configuración de Base de Datos

### Script SQL para crear la base de datos y tablas:

```sql
-- Crear base de datos
CREATE DATABASE HERMES;
GO

USE HERMES;
GO

-- Tabla EMPLEADO
CREATE TABLE EMPLEADO (
    ci_empleado INT PRIMARY KEY,
    nombres_empleado NVARCHAR(100) NOT NULL,
    apellidos_empleado NVARCHAR(100) NOT NULL,
    telefono_empleado NVARCHAR(20),
    correo_empleado NVARCHAR(320),
    es_activo_empleado BIT NOT NULL DEFAULT 1
);
GO

-- Tabla ROL
CREATE TABLE ROL (
    id_rol UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    nombre_rol NVARCHAR(50) NOT NULL,
    descripcion_rol NVARCHAR(300),
    es_activo_rol BIT NOT NULL DEFAULT 1
);
GO

-- Tabla USUARIO
CREATE TABLE USUARIO (
    id_usuario UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    empleado_ci INT NOT NULL,
    rol_id UNIQUEIDENTIFIER NOT NULL,
    nombre_usuario NVARCHAR(50) NOT NULL UNIQUE,
    password_usuario NVARCHAR(255) NOT NULL,
    es_activo_usuario BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_USUARIO_EMPLEADO FOREIGN KEY (empleado_ci) REFERENCES EMPLEADO(ci_empleado),
    CONSTRAINT FK_USUARIO_ROL FOREIGN KEY (rol_id) REFERENCES ROL(id_rol)
);
GO

-- Insertar roles predeterminados
INSERT INTO ROL (nombre_rol, descripcion_rol, es_activo_rol)
VALUES 
    ('Broker', 'Corredor de bienes raíces con acceso completo', 1),
    ('Asesor', 'Asesor de ventas', 1),
    ('Secretaria', 'Personal administrativo', 1),
    ('Administrativo', 'Personal de administración', 1);
GO

-- Insertar empleado de prueba
INSERT INTO EMPLEADO (ci_empleado, nombres_empleado, apellidos_empleado, telefono_empleado, correo_empleado, es_activo_empleado)
VALUES (12345678, 'Juan', 'Pérez', '099123456', 'juan.perez@hermes.com', 1);
GO

-- Crear usuario de prueba (contraseña: "admin123" hasheada en SHA256)
-- Hash SHA256 de "admin123": 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
DECLARE @RolBroker UNIQUEIDENTIFIER;
SELECT @RolBroker = id_rol FROM ROL WHERE nombre_rol = 'Broker';

INSERT INTO USUARIO (empleado_ci, rol_id, nombre_usuario, password_usuario, es_activo_usuario)
VALUES (12345678, @RolBroker, 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);
GO

-- Insertar más empleados de ejemplo
INSERT INTO EMPLEADO (ci_empleado, nombres_empleado, apellidos_empleado, telefono_empleado, correo_empleado, es_activo_empleado)
VALUES 
    (23456789, 'María', 'González', '098765432', 'maria.gonzalez@hermes.com', 1),
    (34567890, 'Carlos', 'Rodríguez', '097654321', 'carlos.rodriguez@hermes.com', 1),
    (45678901, 'Ana', 'Martínez', '096543210', 'ana.martinez@hermes.com', 1),
    (56789012, 'Pedro', 'López', '095432109', 'pedro.lopez@hermes.com', 0);
GO
```

## ?? Usuario de Prueba

**CI:** 12345678  
**Contraseña:** admin123

## ?? Funcionalidades Implementadas

### ? Sistema de Autenticación
- Login con CI y contraseña
- Validación de roles (solo Broker accede al sistema)
- Hash SHA256 para contraseñas
- Gestión de sesión con `App.UsuarioActual`

### ? Ventana Principal (MainWindow)
- **Header elegante** con:
  - Nombre del usuario actual
  - Rol del usuario
  - Botón de cerrar sesión
- **Sidebar con menú** de navegación
- **Área de contenido dinámico**

### ? Gestión de Empleados
- **Vista completa con DataGrid**
- **Búsqueda en tiempo real** por CI, nombres o apellidos
- **Estadísticas** de empleados totales y activos
- **Botones de acción:**
  - ??? Ver - Muestra detalle del empleado
  - ?? Editar - Edita información del empleado
  - ?? Usuario - Gestiona el usuario asociado
- **Botón Nuevo Empleado** ? FUNCIONANDO
- **Botón Actualizar** - Recarga los datos

### ? Ventana de Nuevo Empleado
- Formulario completo para registrar empleados
- Campos: CI, Nombres, Apellidos, Teléfono, Correo, Estado
- Validaciones de campos obligatorios
- Verificación de CI duplicado
- Diseño moderno y responsive

### ? Ventana de Gestionar Usuario
- Crear o actualizar usuario para un empleado
- Asignar rol (Broker, Asesor, Secretaria, Administrativo)
- Cambiar contraseña con confirmación
- Hash SHA256 automático de contraseñas
- Validaciones completas
- Activar/desactivar usuario

## ?? Estructura Implementada

### Models
- ? `Empleado.cs` - Entidad de empleados
- ? `Rol.cs` - Entidad de roles
- ? `Usuario.cs` - Entidad de usuarios
- ? `RolUsuario.cs` (Enum) - Tipos de roles
- ? `Cliente.cs`, `Propiedad.cs`, `Transaccion.cs`

### Data
- ? `HermesDbContext.cs` - Contexto de Entity Framework
- ? `DatabaseHelper.cs` - Helper de conexión

### Services
- ? `AutenticacionService.cs` - Servicio de autenticación
- ? `UsuarioService.cs` - Servicio CRUD de usuarios
- ? `EmpleadoService.cs` - Servicio CRUD de empleados
- ? `PropiedadService.cs`, `ClienteService.cs`

### ViewModels
- ? `BaseViewModel.cs` - Clase base con INotifyPropertyChanged
- ? `LoginViewModel.cs` - ViewModel de login
- ? `MainViewModel.cs` - ViewModel de ventana principal
- ? `GestionEmpleadosViewModel.cs` - ViewModel de gestión de empleados
- ? `UsuariosViewModel.cs`, `PropiedadesViewModel.cs`, `ClientesViewModel.cs`

### Views
- ? `LoginWindow.xaml` - Ventana de inicio de sesión
- ? `MainWindow.xaml` - Ventana principal con sidebar
- ? `GestionEmpleadosView.xaml` - Vista de gestión de empleados
- ? `PropiedadesView.xaml`, `ClientesView.xaml`, `UsuariosView.xaml`

### Helpers
- ? `InverseBooleanConverter.cs` - Convertidor booleano inverso
- ? `StringToVisibilityConverter.cs` - Convertidor de string a visibilidad
- ? `ValidationHelper.cs`, `FileHelper.cs`

### Commands
- ? `RelayCommand.cs` - Implementación de ICommand

### Resources
- ? `Styles.xaml` - Estilos incluyendo MenuButtonStyle

## ?? Configuración de Conexión

Actualiza la cadena de conexión en:
- `Data/HermesDbContext.cs` (línea 14)
- `Data/DatabaseHelper.cs` (línea 7)
- `App.config`

```
Server=PANADERO\PANCITO;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;
```

## ?? Paquetes NuGet Instalados

- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- System.Configuration.ConfigurationManager (10.0.0)

## ?? Flujo de Autenticación

1. La aplicación inicia con `LoginWindow.xaml`
2. El usuario ingresa su CI de empleado y contraseña
3. `AutenticacionService` valida las credenciales con hash SHA256
4. Si el usuario tiene rol "Broker", accede a `MainWindow`
5. El usuario actual se guarda en `App.UsuarioActual`
6. Se muestra la vista de Gestión de Empleados por defecto

## ?? Características de la Gestión de Empleados

### Vista Principal
- **DataGrid con columnas:**
  - CI
  - Nombres
  - Apellidos
  - Teléfono
  - Correo
  - Estado (Activo/Inactivo)
  - Acciones (Ver, Editar, Usuario)

### Funcionalidades
- ? **Búsqueda en tiempo real** - Filtra mientras escribes
- ? **Visualización de detalles** - Muestra información completa
- ? **Estadísticas** - Total de empleados y activos
- ? **Carga asíncrona** - No congela la interfaz
- ? **Diseño responsive** - Se adapta al tamaño de ventana

## ??? Próximos Pasos para Implementar

1. **Ventana de Nuevo Empleado**
   - Formulario para crear empleados
   - Validación de campos
   - Guardado en base de datos

2. **Ventana de Editar Empleado**
   - Cargar datos existentes
   - Actualizar información
   - Gestionar estado activo/inactivo

3. **Ventana de Gestionar Usuario**
   - Crear usuario para empleado
   - Asignar rol (Broker, Asesor, Secretaria, Administrativo)
   - Cambiar contraseña

4. **Otras secciones**
   - Gestión de Propiedades
   - Gestión de Clientes
   - Gestión de Transacciones
   - Reportes

## ?? Paleta de Colores

- **Header:** #2C3E50 (Azul oscuro)
- **Sidebar:** #34495E (Gris azulado)
- **Botón Verde (Nuevo):** #27AE60
- **Botón Azul (Ver/Actualizar):** #3498DB
- **Botón Naranja (Editar):** #F39C12
- **Botón Morado (Usuario):** #9B59B6
- **Botón Rojo (Cerrar):** #E74C3C

## ?? Notas de Desarrollo

- El proyecto compila correctamente ?
- Entity Framework Core está configurado ?
- Los servicios están implementados con async/await ?
- El patrón MVVM está correctamente aplicado ?
- Los comandos están funcionando ?

## ?? Cómo Ejecutar

1. Ejecuta el script SQL en tu servidor `PANADERO\PANCITO`
2. Verifica la cadena de conexión en los archivos mencionados
3. Compila el proyecto (Ctrl + Shift + B)
4. Ejecuta la aplicación (F5)
5. Inicia sesión con CI `12345678` y contraseña `admin123`
6. ¡Ya puedes ver y gestionar empleados!
