Documentación del Sistema HERMES
Table of Contents
Módulo de Autenticación y Seguridad
Tabla de Contenidos
1. Introducción
2. Arquitectura del Sistema
3. Tablas de Base de Datos
4. Sistema de Hasheado de Contraseñas
5. Flujo de Autenticación
6. Implementación en C# WPF
7. Seguridad y Mejores Prácticas
8. Guía de Uso
9. Troubleshooting
10. Conclusiones
Módulo de Autenticación y Seguridad
Tabla de Contenidos
1. Introducción
2. Arquitectura del Sistema
3. Tablas de Base de Datos
4. Sistema de Hasheado de Contraseñas
5. Flujo de Autenticación
6. Implementación en C# WPF
7. Seguridad y Mejores Prácticas
8. Guía de Uso
1. Introducción
El Sistema HERMES es una aplicación de escritorio desarrollada en C# con WPF sobre el framework .NET 8.0, con base de datos
SQL Server 2022. Este documento detalla el módulo de autenticación y gestión de usuarios, enfocándose en la seguridad de las
credenciales y el flujo de login.
Versión: 1.0
Fecha: Noviembre 2025
Stack Tecnológico:
Lenguaje: C# 12
Framework UI: WPF (Windows Presentation Foundation)
Framework Backend: .NET 8.0
ORM: Entity Framework Core 8.0
Base de Datos: SQL Server 2022
Algoritmo de Hash: SHA256
2. Arquitectura del Sistema
2.1 Capas de la Aplicación
┌─────────────────────────────────────┐
│   Capa de Presentación (UI/WPF)     
│
│   • LoginWindow.xaml                
│   • MainWindow.xaml                 
│
│
└────────────────┬────────────────────┘
│
┌─────────────────┴────────────────────┐
│  Capa de Lógica de Presentación      
│
│  • LoginViewModel                    
│  • MainViewModel                     
│  • GestionEmpleadosViewModel         
│
│
│
└────────────────┬────────────────────┘
│
┌─────────────────┴────────────────────┐
│  Capa de Lógica de Negocio (Services)│
│  • AutenticacionService              
│
│  • EmpleadoService                   
│  • UsuarioService                    
│  • RolService                        
│
│
│
└────────────────┬────────────────────┘
│
┌─────────────────┴────────────────────┐
│  Capa de Acceso a Datos (EF Core)    
│  • HermesDbContext                   
│  • DatabaseHelper                    
│  • USUARIO                           
│  • EMPLEADO                          
│  • ROL                               
│
│
│
└────────────────┬────────────────────┘
│
┌─────────────────┴────────────────────┐
│  Base de Datos (SQL Server 2022)     
│
│
│
│
└─────────────────────────────────────┘
2.2 Patrón MVVM
La aplicación sigue el patrón Model-View-ViewModel (MVVM), que proporciona:
Separación clara de responsabilidades
Facilidad de testing
Data binding automático
Mantenimiento simplificado
3. Tablas de Base de Datos
3.1 Tabla EMPLEADO
Propósito: Almacena la información personal de los empleados del sistema.
Campo
ci_empleado
nombres_empleado
INT
Tipo
VARCHAR(100)
Restricción
PRIMARY KEY
NOT NULL
Descripción
Cédula de Identidad (ID único)
Nombres del empleado
apellidos_empleado
VARCHAR(100)
VARCHAR(20)
NOT NULL
NULL
Apellidos del empleado
Número de teléfono
telefono_empleado
Campo
correo_empleado
es_activo_empleado
Tipo
VARCHAR(320)
BIT
Restricción
NULL
DEFAULT 1
Descripción
Correo electrónico
Estado del empleado (1=activo, 0=inactivo)
Script SQL:
CREATE TABLE EMPLEADO (
ci_empleado INT PRIMARY KEY,
nombres_empleado VARCHAR(100) NOT NULL,
apellidos_empleado VARCHAR(100) NOT NULL,
telefono_empleado VARCHAR(20),
correo_empleado VARCHAR(320),
es_activo_empleado BIT NOT NULL DEFAULT 1
);
3.2 Tabla ROL
Propósito: Define los roles disponibles en el sistema y sus permisos.
Campo
id_rol
nombre_rol
descripcion_rol
es_activo_rol
Script SQL:
Tipo
UNIQUEIDENTIFIER
VARCHAR(50)
VARCHAR(300)
BIT
Restricción
PRIMARY KEY
NOT NULL
NULL
DEFAULT 1
CREATE TABLE ROL (
id_rol UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
nombre_rol VARCHAR(50) NOT NULL,
descripcion_rol VARCHAR(300),
es_activo_rol BIT NOT NULL DEFAULT 1
);
Roles Predefinidos:
Broker: Administrador del sistema con acceso completo
Asesor: Asesor de ventas inmobiliarias
Secretaria: Personal administrativo y soporte
Abogado: Asesor legal de transacciones inmobiliarias
Administrador: Administrador del sistema
3.3 Tabla USUARIO
Propósito: Vincula empleados con sus credenciales de acceso y roles asignados.
Campo
id_usuario
empleado_ci
Tipo
UNIQUEIDENTIFIER
INT
UNIQUEIDENTIFIER
Restricción
PRIMARY KEY
NOT NULL, FK
NOT NULL, FK
Descripción
Identificador único del rol
Nombre del rol
Descripción de funciones del rol
Estado del rol (1=activo, 0=inactivo)
Descripción
Identificador único del usuario
Referencia al empleado (CI)
rol_id
Referencia al rol asignado
Campo
nombre_usuario
Tipo
VARCHAR(50)
Restricción
Descripción
NOT NULL
password_usuario
es_activo_usuario
VARCHAR(255)
BIT
NOT NULL
Nombre de usuario para login
Contraseña hasheada (SHA256)
DEFAULT 1
Script SQL:
CREATE TABLE USUARIO (
id_usuario UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
empleado_ci INT NOT NULL,
rol_id UNIQUEIDENTIFIER NOT NULL,
nombre_usuario VARCHAR(50) NOT NULL,
password_usuario VARCHAR(255) NOT NULL,
es_activo_usuario BIT NOT NULL DEFAULT 1,
CONSTRAINT FK_USUARIO_EMPLEADO FOREIGN KEY (empleado_ci) 
REFERENCES EMPLEADO(ci_empleado),
CONSTRAINT FK_USUARIO_ROL FOREIGN KEY (rol_id) 
REFERENCES ROL(id_rol)
);
3.4 Relaciones entre Tablas
EMPLEADO (1) ──────→ (N) USUARIO
|                     
|
|                     
|
└─────────────────────┴──→ ROL
EMPLEADO:USUARIO = 1:N
(Un empleado puede tener múltiples usuarios en el tiempo)
ROL:USUARIO = 1:N
(Un rol puede ser asignado a múltiples usuarios)
4. Sistema de Hasheado de Contraseñas
4.1 Algoritmo SHA256
¿Por qué SHA256?
Algoritmo criptográfico de 256 bits
Función hash unidireccional (irreversible)
Genera hash de 64 caracteres hexadecimales
Estándar NIST (National Institute of Standards and Technology)
Ampliamente utilizado en la industria
Resistente a ataques de colisión
4.2 Proceso de Hasheado
Estado del usuario (1=activo, 0=inactivo)
Contraseña Original: "broker123"
↓
[SHA256 HASH]
↓
Hash Resultado: "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"
NUNCA se almacena la contraseña en texto plano. Solo se almacena el hash SHA256:
Campo Valor
nombre_usuario broker_admin
password_usuario 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
private string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        // Convertir contraseña a bytes UTF-8
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        // Convertir bytes a string hexadecimal
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            // "x2" = formato hexadecimal de 2 dígitos en minúsculas
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
✓ Irreversible: Es imposible obtener la contraseña original del hash
✓ Determinístico: La misma contraseña siempre genera el mismo hash
✓ Colisión mínima: Probabilidad extremadamente baja de dos contraseñas diferentes generando el mismo hash
✓ Velocidad: Procesamiento rápido para validación
✓ Estandarizado: Reconocido internacionalmente
┌─────────────────────────────────────┐
│   Ventana de Login (LoginWindow)    │
│  [CI: ________]  [Password: ______] │
└──────────────┬──────────────────────┘
               │
               ↓
     ┌─────────────────────┐
     │  ¿Datos válidos?    │ → NO → Mostrar error
     └──────────┬──────────┘
               │ SÍ
               ↓
   ┌─────────────────────────────────┐
   │ AutenticacionService.Validar()  │
   │                                 │
   │ 1. Buscar usuario por CI        │
   │ 2. Verificar que esté activo    │
   │ 3. Hashear contraseña ingresada │
   │ 4. Comparar hashes              │
   └──────────┬──────────────────────┘
              │
              ↓
    ┌──────────────────────┐
4.3 Almacenamiento en Base de Datos
4.4 Implementación en C#
4.5 Ventajas del Hasheado SHA256
5. Flujo de Autenticación
5.1 Diagrama de Flujo Login
│ ¿Hash coincide?      
│ → NO → "Contraseña incorrecta"
└──────────┬───────────┘
│ SÍ
↓
┌────────────────────────────────┐
│ Guardar Usuario en sesión      
│ App.UsuarioActual = usuario    
│
│
└──────────┬─────────────────────┘
│
↓
┌────────────────────────────────┐
│ ¿Es rol Broker?                
│ → NO → "Acceso denegado"
└──────────┬─────────────────────┘
│ SÍ
↓
┌────────────────────────────────┐
│ Abre MainWindow (Panel Broker)  │
│ Cierra LoginWindow              
│
└────────────────────────────────┘
5.2 Pasos Detallados
Paso 1: Validación de Entrada
if (CiEmpleado == 0 || string.IsNullOrWhiteSpace(Password))
{
MensajeError = "Por favor, ingrese CI y contraseña";
return;
}
Paso 2: Búsqueda en Base de Datos
var usuario = await _context.Usuarios
.Include(u =&gt; u.Empleado)
.Include(u =&gt; u.Rol)
.FirstOrDefaultAsync(u =&gt; u.EmpleadoCi == ciEmpleado &amp;&amp; u.EsActivoUsuario);
if (usuario == null)
return (false, null, "Usuario no encontrado o inactivo");
Paso 3: Hasheado de Contraseña Ingresada
string passwordHash = HashPassword(password);
// password: "broker123"
// passwordHash: "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"
Paso 4: Comparación de Hashes
if (usuario.PasswordUsuario != passwordHash)
{
return (false, null, "Contraseña incorrecta");
}
return (true, usuario, "Autenticación exitosa");
6. Implementación en C# WPF
6.1 Archivo: LoginWindow.xaml
Descripción: Interfaz gráfica del login
Campos: CI de Empleado, Contraseña, Botón Login
Estilos: Bordes redondeados, sombras, animaciones suaves
Validaciones: En tiempo real, mensajes de error en rojo
6.2 Archivo: LoginViewModel.cs
Comando: 
LoginCommand → Ejecuta validación y autenticación
Propiedades: 
CiEmpleado, 
Password, 
Método: 
MensajeError, 
IsLoading
LoginAsync() → Coordina la autenticación
6.3 Archivo: AutenticacionService.cs
public async Task&lt;(bool Success, Usuario Usuario, string Mensaje)&gt; 
ValidarCredencialesAsync(int ciEmpleado, string password)
{
// 1. Buscar usuario
// 2. Validar que esté activo
// 3. Hashear contraseña
// 4. Comparar
// 5. Retornar resultado
}
6.4 Archivo: HermesDbContext.cs
public DbSet&lt;Usuario&gt; Usuarios { get; set; }
public DbSet&lt;Empleado&gt; Empleados { get; set; }
public DbSet&lt;Rol&gt; Roles { get; set; }
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
optionsBuilder.UseSqlServer(
@"Server=PANADERO\PANCITO;Database=HERMES;
Trusted_Connection=True;TrustServerCertificate=True;"
);
}
6.5 Archivo: App.xaml.cs
public static Usuario UsuarioActual { get; set; }
// Almacena globalmente el usuario logueado
// Se utiliza en toda la aplicación
7. Seguridad y Mejores Prácticas
7.1 Medidas de Seguridad Implementadas
✓ Hasheado irreversible: SHA256 unidireccional
✓ Base de datos segura: Nunca se almacenan contraseñas en texto plano
✓ Conexión segura: TrustServerCertificate en conexión SQL
✓ Validación de entrada: Se validan CI y contraseñas antes de procesar
✓ Validación activa: Solo usuarios activos pueden autenticarse
✓ Control de roles: Acceso restringido por tipo de usuario
✓ Manejo de excepciones: Try-catch en operaciones críticas
7.2 Recomendaciones Adicionales (Futuro)
Para mayor seguridad en producción, considerar:
1. Salting: Agregar un valor aleatorio al hash
string salt = GenerarSaltAleatorio(); // 16+ caracteres
string hashConSalt = HashPassword(password + salt);
2. PBKDF2: Usar algoritmo más resistente a ataques
var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
byte[] hash = pbkdf2.GetBytes(32);
3. Bcrypt: Biblioteca especializada
string hash = BCrypt.Net.BCrypt.HashPassword(password);
bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
4. MFA (Multi-Factor Authentication): Verificación de dos factores
5. Logging: Registrar intentos de login fallidos
7.3 Contraseñas de Prueba
Rol
Broker
CI
Usuario
12345678
8. Guía de Uso
8.1 Crear Nuevo Usuario
1. Login como Broker
CI: 
12345678
Contraseña: 
broker123
2. Ir a Gestión de Empleados
Click en "Gestión de Empleados"
3. Crear Nuevo Empleado
Click en "➕ Nuevo Empleado"
Llenar formulario:
CI (único)
Nombres
Apellidos
Teléfono
Correo
Click "Guardar Empleado"
4. Asignar Usuario
En la tabla, click " Usuario"
Llenar:
Nombre de Usuario
broker_admin
Contraseña
broker123
Contraseña (mín. 6 caracteres)
Confirmar Contraseña
Seleccionar Rol
Click "Crear Usuario"
8.2 Cambiar Contraseña
1. En la ventana "Gestionar Usuario"
2. Ingresar nueva contraseña
3. Confirmar contraseña
4. Click "Actualizar Usuario"
8.3 Desactivar Usuario
1. En la ventana "Gestionar Usuario"
2. Desmarcar "Usuario Activo"
3. Click "Actualizar Usuario"
9. Troubleshooting
Problema: "Usuario no encontrado"
Solución: Verificar que:
El CI del empleado existe en tabla EMPLEADO
El usuario existe en tabla USUARIO
El usuario está marcado como activo (es_activo_usuario = 1)
Problema: "Contraseña incorrecta"
Solución:
Verificar que la contraseña sea exactamente "broker123"
Asegurarse de que el hash en BD sea: 
Limpiar caché de Entity Framework
Problema: "Acceso denegado"
Solución:
8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
Verificar que el rol asignado sea "Broker"
Solo Brokers pueden acceder al panel principal
Problema: Error de conexión a BD
Solución:
Verificar que SQL Server esté corriendo
Verificar la cadena de conexión en App.config
Asegurarse de que la BD "HERMES" exista
10. Conclusiones
El sistema HERMES implementa un módulo de autenticación robusto y seguro utilizando:
Hasheado SHA256 para proteger contraseñas
Arquitectura MVVM para separación de responsabilidades
Entity Framework Core para acceso a datos
Validaciones en múltiples niveles
Control de roles y acceso
Este diseño garantiza que las credenciales se almacenen de forma segura y que el acceso al sistema esté protegido
adecuadamente.
Documento Generado: Noviembre 2025
Versión: 1.0
Autor: Equipo de Desarrollo HERME