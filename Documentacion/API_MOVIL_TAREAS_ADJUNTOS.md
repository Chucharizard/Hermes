# API/Documentación para Módulo Móvil - Sistema de Tareas HERMES

## 📋 Índice
1. [Estructura de Base de Datos](#estructura-de-base-de-datos)
2. [Modelos de Datos](#modelos-de-datos)
3. [Validaciones Críticas](#validaciones-críticas)
4. [Flujos de Trabajo](#flujos-de-trabajo)
5. [Operaciones CRUD](#operaciones-crud)
6. [Ejemplos de Código SQL](#ejemplos-de-código-sql)
7. [Consideraciones para App Móvil](#consideraciones-para-app-móvil)

---

## 🗄️ Estructura de Base de Datos

### Tabla: `TAREA`

Tabla principal que almacena las tareas del sistema.

| Columna | Tipo | Nullable | Descripción |
|---------|------|----------|-------------|
| `id_tarea` | UNIQUEIDENTIFIER | NO | PK - ID único de la tarea (GUID) |
| `usuario_emisor_id` | UNIQUEIDENTIFIER | NO | FK - Usuario que crea/envía la tarea |
| `usuario_receptor_id` | UNIQUEIDENTIFIER | NO | FK - Usuario que recibe/ejecuta la tarea |
| `titulo_tarea` | NVARCHAR(150) | NO | Título de la tarea |
| `descripcion_tarea` | NVARCHAR(500) | SÍ | Descripción detallada (opcional) |
| `estado_tarea` | NVARCHAR(50) | NO | Estados: "Pendiente", "Completado", "Vencido", "Observado", "Archivado" |
| `prioridad_tarea` | NVARCHAR(20) | NO | Prioridades: "Baja", "Media", "Alta" |
| `fecha_inicio_tarea` | DATETIME2 | NO | Fecha y hora de inicio |
| `fecha_limite_tarea` | DATETIME2 | SÍ | Fecha y hora límite (opcional) |
| `fecha_completada_tarea` | DATETIME2 | SÍ | Fecha y hora en que se completó |
| `permite_entrega_con_retraso` | BIT | NO | **IMPORTANTE**: true = permite subir después de límite, false = cierre estricto (DEFAULT: 1) |

**Índices importantes:**
- PK en `id_tarea`
- FK en `usuario_emisor_id` → `USUARIO.id_usuario`
- FK en `usuario_receptor_id` → `USUARIO.id_usuario`

---

### Tabla: `TAREA_ADJUNTO`

Almacena archivos adjuntos de las tareas (PDFs escaneados, documentos, imágenes).

| Columna | Tipo | Nullable | Descripción |
|---------|------|----------|-------------|
| `id_tarea_adjunto` | UNIQUEIDENTIFIER | NO | PK - ID único del adjunto (GUID) |
| `tarea_id` | UNIQUEIDENTIFIER | NO | FK - ID de la tarea a la que pertenece |
| `nombre_archivo_tarea_adjunto` | NVARCHAR(150) | NO | Nombre del archivo (ej: "documento.pdf") |
| `tipo_archivo_tarea_adjunto` | NVARCHAR(50) | NO | Extensión del archivo (ej: ".pdf", ".jpg") |
| `archivo_tarea_adjunto` | VARBINARY(MAX) | SÍ | **CONTENIDO BINARIO** del archivo |
| `fecha_subida_tarea_adjunto` | DATETIME2 | NO | Fecha y hora de subida |
| `id_usuario_subio` | UNIQUEIDENTIFIER | SÍ | FK - Usuario que subió el archivo (opcional) |

**Nota crítica:** Los archivos se guardan DENTRO de la base de datos como `VARBINARY(MAX)`, no en el sistema de archivos.

**Índices importantes:**
- PK en `id_tarea_adjunto`
- FK en `tarea_id` → `TAREA.id_tarea`
- FK en `id_usuario_subio` → `USUARIO.id_usuario`

---

### Tabla: `TAREA_COMENTARIO`

Almacena comentarios/observaciones sobre las tareas.

| Columna | Tipo | Nullable | Descripción |
|---------|------|----------|-------------|
| `id_tarea_comentario` | UNIQUEIDENTIFIER | NO | PK - ID único del comentario (GUID) |
| `tarea_id` | UNIQUEIDENTIFIER | NO | FK - ID de la tarea |
| `usuario_autor_id` | UNIQUEIDENTIFIER | NO | FK - Usuario que escribió el comentario |
| `comentario_tarea_comentario` | NVARCHAR(500) | NO | Texto del comentario |
| `fecha_tarea_comentario` | DATETIME2 | NO | Fecha y hora del comentario |

**Índices importantes:**
- PK en `id_tarea_comentario`
- FK en `tarea_id` → `TAREA.id_tarea`
- FK en `usuario_autor_id` → `USUARIO.id_usuario`

---

### Tabla: `USUARIO`

Usuarios del sistema (ya existe en tu BD).

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `id_usuario` | UNIQUEIDENTIFIER | PK - ID del usuario |
| `empleado_ci` | NVARCHAR(20) | FK - CI del empleado |
| `rol_id` | UNIQUEIDENTIFIER | FK - ID del rol |
| `nombre_usuario` | NVARCHAR(50) | Username |
| `password_usuario` | NVARCHAR(255) | Contraseña hasheada |
| `es_activo_usuario` | BIT | Estado (activo/inactivo) |

---

## 📦 Modelos de Datos

### Modelo C#: Tarea

```csharp
public class Tarea
{
    public Guid IdTarea { get; set; }
    public Guid UsuarioEmisorId { get; set; }
    public Guid UsuarioReceptorId { get; set; }
    public string TituloTarea { get; set; }
    public string? DescripcionTarea { get; set; }
    public string EstadoTarea { get; set; } // "Pendiente", "Completado", "Vencido", "Observado", "Archivado"
    public string PrioridadTarea { get; set; } // "Baja", "Media", "Alta"
    public DateTime FechaInicioTarea { get; set; }
    public DateTime? FechaLimiteTarea { get; set; }
    public DateTime? FechaCompletadaTarea { get; set; }
    public bool PermiteEntregaConRetraso { get; set; } = true; // DEFAULT true

    // Navigation properties
    public Usuario? UsuarioEmisor { get; set; }
    public Usuario? UsuarioReceptor { get; set; }
}
```

**Propiedades calculadas útiles:**

```csharp
// Indica si la tarea está vencida
public bool EstaVencida
{
    get
    {
        if (!FechaLimiteTarea.HasValue) return false;
        if (EstadoTarea == "Completado" || EstadoTarea == "Archivado") return false;
        return DateTime.Now > FechaLimiteTarea.Value;
    }
}

// Indica si fue completada con retraso
public bool CompletadaConRetraso
{
    get
    {
        if (EstadoTarea != "Completado" || !FechaCompletadaTarea.HasValue || !FechaLimiteTarea.HasValue)
            return false;
        return FechaCompletadaTarea.Value > FechaLimiteTarea.Value;
    }
}
```

---

### Modelo C#: TareaAdjunto

```csharp
public class TareaAdjunto
{
    public Guid IdTareaAdjunto { get; set; }
    public Guid TareaId { get; set; }
    public string NombreArchivoTareaAdjunto { get; set; }
    public string TipoArchivoTareaAdjunto { get; set; } // Extensión (.pdf, .jpg, etc.)
    public byte[]? ArchivoTareaAdjunto { get; set; } // CONTENIDO BINARIO
    public DateTime FechaSubidaTareaAdjunto { get; set; }
    public Guid? IdUsuarioSubioTareaAdjunto { get; set; }

    // Navigation properties
    public Tarea? Tarea { get; set; }
    public Usuario? UsuarioSubio { get; set; }
}
```

---

### Modelo C#: TareaComentario

```csharp
public class TareaComentario
{
    public Guid IdTareaComentario { get; set; }
    public Guid TareaIdComentario { get; set; }
    public Guid UsuarioAutorId { get; set; }
    public string ComentarioTareaComentario { get; set; }
    public DateTime FechaTareaComentario { get; set; }

    // Navigation properties
    public Tarea? Tarea { get; set; }
    public Usuario? Usuario { get; set; }
}
```

---

## ⚠️ Validaciones Críticas

### 1. Validación de Subida de Archivos

**Regla de Negocio CRÍTICA:** Antes de permitir subir un archivo, validar:

```csharp
public bool PuedeSubirArchivos(Tarea tarea)
{
    // 1. Si la tarea está completada o archivada, NO se pueden subir archivos
    if (tarea.EstadoTarea == "Completado" || tarea.EstadoTarea == "Archivado")
    {
        return false;
    }

    // 2. Si la tarea tiene fecha límite y ya pasó
    if (tarea.FechaLimiteTarea.HasValue && DateTime.Now > tarea.FechaLimiteTarea.Value)
    {
        // Si NO permite entrega con retraso, se cierra estrictamente
        if (!tarea.PermiteEntregaConRetraso)
        {
            return false; // BLOQUEO TOTAL
        }
        // Si permite retraso, se puede subir (pero será marcado como con retraso)
    }

    return true;
}
```

**Mensajes de error recomendados:**

- Si `PermiteEntregaConRetraso = false` y ya pasó la fecha límite:
  ```
  "Esta tarea tiene cierre estricto y no acepta entregas después de la fecha y hora límite."
  ```

- Si `PermiteEntregaConRetraso = true` pero la tarea está completada/archivada:
  ```
  "No se pueden subir archivos en tareas completadas o archivadas."
  ```

---

### 2. Validación de Comentarios

Los comentarios se pueden agregar en cualquier momento, EXCEPTO:
- Cuando el usuario o empleado está inactivo (`es_activo_usuario = 0` o `es_activo_empleado = 0`)

```csharp
public bool PuedeComentarTarea(Usuario usuario)
{
    if (usuario == null || !usuario.EsActivoUsuario)
        return false;

    if (usuario.Empleado == null || !usuario.Empleado.EsActivoEmpleado)
        return false;

    return true;
}
```

---

### 3. Estados de Tarea

**Flujo de estados:**

```
Pendiente → Completado → Archivado (flujo normal)
Pendiente → Observado → Pendiente (si hay observaciones)
Pendiente → Vencido (automático si pasa fecha límite)
```

**Cambios de estado permitidos:**

| Estado Actual | Puede cambiar a | Quién puede |
|---------------|-----------------|-------------|
| Pendiente | Completado | Receptor |
| Pendiente | Observado | Receptor (devolver) o Emisor (observar completada) |
| Completado | Archivado | Emisor |
| Completado | Observado | Emisor |
| Observado | Pendiente | Emisor (reasignar) |

---

## 🔄 Flujos de Trabajo

### Flujo 1: Subir Archivo Adjunto (App Móvil como Scanner)

**Escenario:** Usuario escanea un documento con la app móvil y lo sube como PDF a una tarea.

```sql
-- PASO 1: Obtener la tarea y validar
DECLARE @IdTarea UNIQUEIDENTIFIER = 'GUID-DE-LA-TAREA';
DECLARE @IdUsuarioActual UNIQUEIDENTIFIER = 'GUID-DEL-USUARIO';

SELECT
    id_tarea,
    estado_tarea,
    fecha_limite_tarea,
    permite_entrega_con_retraso,
    CASE
        WHEN estado_tarea IN ('Completado', 'Archivado') THEN 0
        WHEN fecha_limite_tarea IS NOT NULL
             AND GETDATE() > fecha_limite_tarea
             AND permite_entrega_con_retraso = 0 THEN 0
        ELSE 1
    END AS puede_subir
FROM TAREA
WHERE id_tarea = @IdTarea;

-- PASO 2: Si puede_subir = 1, insertar el adjunto
-- En tu app móvil, convierte el PDF escaneado a byte[]
DECLARE @ContenidoPDF VARBINARY(MAX) = <BYTES_DEL_PDF>;
DECLARE @NombreArchivo NVARCHAR(150) = 'documento_escaneado.pdf';

INSERT INTO TAREA_ADJUNTO (
    id_tarea_adjunto,
    tarea_id,
    nombre_archivo_tarea_adjunto,
    tipo_archivo_tarea_adjunto,
    archivo_tarea_adjunto,
    fecha_subida_tarea_adjunto,
    id_usuario_subio
) VALUES (
    NEWID(),
    @IdTarea,
    @NombreArchivo,
    '.pdf',
    @ContenidoPDF,
    GETDATE(),
    @IdUsuarioActual
);
```

---

### Flujo 2: Agregar Comentario

```sql
DECLARE @IdTarea UNIQUEIDENTIFIER = 'GUID-DE-LA-TAREA';
DECLARE @IdUsuarioActual UNIQUEIDENTIFIER = 'GUID-DEL-USUARIO';
DECLARE @Comentario NVARCHAR(500) = 'Archivo escaneado y subido desde app móvil';

-- Validar que el usuario esté activo
DECLARE @EsActivo BIT;
SELECT @EsActivo = u.es_activo_usuario
FROM USUARIO u
WHERE u.id_usuario = @IdUsuarioActual;

IF @EsActivo = 1
BEGIN
    INSERT INTO TAREA_COMENTARIO (
        id_tarea_comentario,
        tarea_id,
        usuario_autor_id,
        comentario_tarea_comentario,
        fecha_tarea_comentario
    ) VALUES (
        NEWID(),
        @IdTarea,
        @IdUsuarioActual,
        @Comentario,
        GETDATE()
    );
END
```

---

### Flujo 3: Completar Tarea

```sql
DECLARE @IdTarea UNIQUEIDENTIFIER = 'GUID-DE-LA-TAREA';
DECLARE @IdUsuarioReceptor UNIQUEIDENTIFIER = 'GUID-DEL-RECEPTOR';

-- Validar que el usuario actual sea el receptor
UPDATE TAREA
SET
    estado_tarea = 'Completado',
    fecha_completada_tarea = GETDATE()
WHERE
    id_tarea = @IdTarea
    AND usuario_receptor_id = @IdUsuarioReceptor
    AND estado_tarea = 'Pendiente';

-- Retorna el número de filas afectadas
-- Si es 0, el usuario no tiene permiso o la tarea no está en estado correcto
```

---

### Flujo 4: Obtener Tareas Asignadas al Usuario (Para App Móvil)

```sql
DECLARE @IdUsuarioActual UNIQUEIDENTIFIER = 'GUID-DEL-USUARIO';

SELECT
    t.id_tarea,
    t.titulo_tarea,
    t.descripcion_tarea,
    t.estado_tarea,
    t.prioridad_tarea,
    t.fecha_inicio_tarea,
    t.fecha_limite_tarea,
    t.fecha_completada_tarea,
    t.permite_entrega_con_retraso,

    -- Usuario Emisor
    emisor.nombre_usuario AS emisor_username,
    emp_emisor.nombres_empleado + ' ' + emp_emisor.apellidos_empleado AS emisor_nombre_completo,

    -- Usuario Receptor
    receptor.nombre_usuario AS receptor_username,
    emp_receptor.nombres_empleado + ' ' + emp_receptor.apellidos_empleado AS receptor_nombre_completo,

    -- Contadores
    (SELECT COUNT(*) FROM TAREA_ADJUNTO WHERE tarea_id = t.id_tarea) AS total_adjuntos,
    (SELECT COUNT(*) FROM TAREA_COMENTARIO WHERE tarea_id = t.id_tarea) AS total_comentarios,

    -- Flags calculados
    CASE
        WHEN t.fecha_limite_tarea IS NOT NULL
             AND GETDATE() > t.fecha_limite_tarea
             AND t.estado_tarea NOT IN ('Completado', 'Archivado') THEN 1
        ELSE 0
    END AS esta_vencida,

    CASE
        WHEN t.estado_tarea IN ('Completado', 'Archivado') THEN 0
        WHEN t.fecha_limite_tarea IS NOT NULL
             AND GETDATE() > t.fecha_limite_tarea
             AND t.permite_entrega_con_retraso = 0 THEN 0
        ELSE 1
    END AS puede_subir_archivos

FROM TAREA t
LEFT JOIN USUARIO emisor ON t.usuario_emisor_id = emisor.id_usuario
LEFT JOIN EMPLEADO emp_emisor ON emisor.empleado_ci = emp_emisor.ci_empleado
LEFT JOIN USUARIO receptor ON t.usuario_receptor_id = receptor.id_usuario
LEFT JOIN EMPLEADO emp_receptor ON receptor.empleado_ci = emp_receptor.ci_empleado

WHERE t.usuario_receptor_id = @IdUsuarioActual
ORDER BY
    CASE t.estado_tarea
        WHEN 'Pendiente' THEN 1
        WHEN 'Observado' THEN 2
        WHEN 'Vencido' THEN 3
        WHEN 'Completado' THEN 4
        WHEN 'Archivado' THEN 5
    END,
    t.fecha_limite_tarea ASC;
```

---

### Flujo 5: Obtener Adjuntos de una Tarea

```sql
DECLARE @IdTarea UNIQUEIDENTIFIER = 'GUID-DE-LA-TAREA';

SELECT
    ta.id_tarea_adjunto,
    ta.nombre_archivo_tarea_adjunto,
    ta.tipo_archivo_tarea_adjunto,
    ta.fecha_subida_tarea_adjunto,
    DATALENGTH(ta.archivo_tarea_adjunto) AS tamaño_bytes,

    -- Usuario que subió
    u.nombre_usuario,
    e.nombres_empleado + ' ' + e.apellidos_empleado AS nombre_completo,

    -- Para descargar, necesitarás el contenido binario
    ta.archivo_tarea_adjunto AS contenido_binario

FROM TAREA_ADJUNTO ta
LEFT JOIN USUARIO u ON ta.id_usuario_subio = u.id_usuario
LEFT JOIN EMPLEADO e ON u.empleado_ci = e.ci_empleado

WHERE ta.tarea_id = @IdTarea
ORDER BY ta.fecha_subida_tarea_adjunto DESC;
```

---

### Flujo 6: Descargar/Abrir un Adjunto

```sql
-- Para descargar un archivo específico
DECLARE @IdAdjunto UNIQUEIDENTIFIER = 'GUID-DEL-ADJUNTO';

SELECT
    nombre_archivo_tarea_adjunto AS nombre_archivo,
    tipo_archivo_tarea_adjunto AS extension,
    archivo_tarea_adjunto AS contenido_binario
FROM TAREA_ADJUNTO
WHERE id_tarea_adjunto = @IdAdjunto;

-- En tu app móvil:
-- 1. Recupera el byte[] de 'contenido_binario'
-- 2. Escribe el byte[] a un archivo temporal
-- 3. Abre el archivo con un visor de PDF o guardarlo en el dispositivo
```

---

## 🛠️ Operaciones CRUD

### Crear Tarea

```sql
INSERT INTO TAREA (
    id_tarea,
    usuario_emisor_id,
    usuario_receptor_id,
    titulo_tarea,
    descripcion_tarea,
    estado_tarea,
    prioridad_tarea,
    fecha_inicio_tarea,
    fecha_limite_tarea,
    permite_entrega_con_retraso
) VALUES (
    NEWID(),
    @EmisorId,
    @ReceptorId,
    @Titulo,
    @Descripcion,
    'Pendiente',
    @Prioridad, -- 'Baja', 'Media', 'Alta'
    GETDATE(),
    @FechaLimite, -- Puede ser NULL
    @PermiteRetraso -- 1 o 0
);
```

---

### Leer Tareas (Filtros Comunes)

```sql
-- Tareas pendientes del usuario
SELECT * FROM TAREA
WHERE usuario_receptor_id = @IdUsuario
  AND estado_tarea = 'Pendiente'
ORDER BY fecha_limite_tarea ASC;

-- Tareas vencidas
SELECT * FROM TAREA
WHERE usuario_receptor_id = @IdUsuario
  AND estado_tarea NOT IN ('Completado', 'Archivado')
  AND fecha_limite_tarea < GETDATE()
ORDER BY fecha_limite_tarea ASC;

-- Tareas por prioridad
SELECT * FROM TAREA
WHERE usuario_receptor_id = @IdUsuario
  AND prioridad_tarea = 'Alta'
  AND estado_tarea = 'Pendiente'
ORDER BY fecha_limite_tarea ASC;
```

---

### Actualizar Estado de Tarea

```sql
-- Completar tarea
UPDATE TAREA
SET
    estado_tarea = 'Completado',
    fecha_completada_tarea = GETDATE()
WHERE id_tarea = @IdTarea;

-- Devolver tarea con observaciones (cambia a Observado)
UPDATE TAREA
SET estado_tarea = 'Observado'
WHERE id_tarea = @IdTarea;

-- Archivar tarea
UPDATE TAREA
SET estado_tarea = 'Archivado'
WHERE id_tarea = @IdTarea;
```

---

### Eliminar Adjunto

```sql
-- Solo puede eliminar quien lo subió
DELETE FROM TAREA_ADJUNTO
WHERE id_tarea_adjunto = @IdAdjunto
  AND id_usuario_subio = @IdUsuarioActual;

-- Retorna el número de filas afectadas
-- Si es 0, el usuario no tiene permiso
```

---

## 📱 Consideraciones para App Móvil

### 1. Scanner de Documentos a PDF

**Librerías recomendadas:**
- **Android**: PDFBox-Android, iText, CamScanner SDK
- **iOS**: VisionKit (escaneo nativo), PDFKit
- **Flutter/React Native**: flutter_document_scanner, react-native-document-scanner

**Flujo recomendado:**
1. Usuario abre cámara en modo scanner
2. Captura imagen(s)
3. Procesa y mejora imagen (auto-crop, ajuste de brillo/contraste)
4. Convierte a PDF
5. Comprime PDF si es muy grande (idealmente < 5MB)
6. Convierte PDF a `byte[]`
7. Envía a base de datos con query INSERT

---

### 2. Conversión de PDF a byte[]

**Pseudocódigo:**

```java
// Android/Java
File pdfFile = new File("/path/to/scanned.pdf");
byte[] pdfBytes = Files.readAllBytes(pdfFile.toPath());

// Ahora pdfBytes se puede insertar en VARBINARY(MAX)
```

```csharp
// C# (por si usas Xamarin)
byte[] pdfBytes = File.ReadAllBytes("/path/to/scanned.pdf");
```

```dart
// Flutter/Dart
import 'dart:io';
File pdfFile = File('/path/to/scanned.pdf');
List<int> pdfBytes = await pdfFile.readAsBytes();
```

---

### 3. Conexión Directa a SQL Server desde Móvil

**IMPORTANTE:** Conectar directamente desde la app móvil a SQL Server NO es recomendado por seguridad. Considera:

**Opción A: API REST (Recomendado)**
- Crear una API intermedia en .NET/Node.js/PHP
- La app móvil llama endpoints REST
- La API se conecta a SQL Server
- Más seguro (no expone credenciales de BD)

**Opción B: Conexión Directa (Si es red local/VPN)**
- Usa librerías como:
  - **Android**: jTDS JDBC Driver
  - **iOS**: No nativo, usar API REST
  - **Flutter**: sqljocky5 (no oficial, limitado)

**Ejemplo conexión directa Android (NO recomendado para producción):**

```java
import net.sourceforge.jtds.jdbc.*;

String connectionString = "jdbc:jtds:sqlserver://IP_SERVER:1433;databaseName=HERMES;user=usuario;password=pass";
Connection conn = DriverManager.getConnection(connectionString);

// Subir archivo
String sql = "INSERT INTO TAREA_ADJUNTO (id_tarea_adjunto, tarea_id, nombre_archivo_tarea_adjunto, tipo_archivo_tarea_adjunto, archivo_tarea_adjunto, fecha_subida_tarea_adjunto, id_usuario_subio) VALUES (?, ?, ?, ?, ?, GETDATE(), ?)";

PreparedStatement ps = conn.prepareStatement(sql);
ps.setString(1, UUID.randomUUID().toString());
ps.setString(2, tareaId);
ps.setString(3, "documento.pdf");
ps.setString(4, ".pdf");
ps.setBytes(5, pdfBytes); // byte[] del PDF
ps.setString(6, usuarioId);

int rows = ps.executeUpdate();
```

---

### 4. Validación de Entrega con Retraso (CRÍTICO)

**Antes de mostrar botón "Subir Archivo" en la UI:**

```java
// Pseudocódigo
boolean puedeSubirArchivos(Tarea tarea) {
    // Si completada o archivada
    if (tarea.estadoTarea.equals("Completado") || tarea.estadoTarea.equals("Archivado")) {
        return false;
    }

    // Si tiene fecha límite
    if (tarea.fechaLimiteTarea != null) {
        Date ahora = new Date();

        // Si ya pasó la fecha límite
        if (ahora.after(tarea.fechaLimiteTarea)) {
            // Si NO permite retraso, bloquear
            if (!tarea.permiteEntregaConRetraso) {
                return false;
            }
        }
    }

    return true;
}

// En la UI
if (puedeSubirArchivos(tarea)) {
    btnSubirArchivo.setEnabled(true);
} else {
    btnSubirArchivo.setEnabled(false);
    showMessage("Esta tarea no acepta más entregas");
}
```

---

### 5. Manejo de Archivos Grandes

**Límites recomendados:**
- PDF escaneado: < 5 MB
- Si es mayor, comprimir antes de subir

**Compresión de PDF:**

```java
// Usar iText o PDFBox para comprimir
// Reducir calidad de imágenes dentro del PDF
// Esto es importante para no saturar la BD
```

---

### 6. Sincronización Offline (Opcional)

Si la app móvil puede trabajar sin internet:

1. Guardar PDFs localmente (SQLite local)
2. Marcar como "pendiente de subir"
3. Cuando haya internet, subir a SQL Server
4. Actualizar estado local

---

## 🎯 Resumen de Implementación para App Móvil

### Funcionalidades Mínimas Requeridas:

1. **Login y Autenticación**
   - Conectar a tabla `USUARIO`
   - Validar credenciales
   - Almacenar `id_usuario` en sesión

2. **Listar Tareas Asignadas**
   - Query: `SELECT * FROM TAREA WHERE usuario_receptor_id = @IdUsuario`
   - Mostrar: título, prioridad, fecha límite, estado
   - Indicador visual si está vencida

3. **Ver Detalle de Tarea**
   - Mostrar toda la información
   - Listar adjuntos existentes
   - Listar comentarios
   - Botones: Completar, Agregar Comentario, Subir Archivo

4. **Scanner de Documentos**
   - Abrir cámara
   - Capturar documento
   - Convertir a PDF
   - Subir a `TAREA_ADJUNTO`

5. **Subir Archivo (VALIDADO)**
   - **ANTES de permitir:** Validar `permite_entrega_con_retraso` y fechas
   - Convertir PDF a `byte[]`
   - INSERT en `TAREA_ADJUNTO`
   - Refrescar lista de adjuntos

6. **Agregar Comentario**
   - Campo de texto
   - INSERT en `TAREA_COMENTARIO`
   - Refrescar lista de comentarios

7. **Completar Tarea**
   - Botón visible solo si `estado_tarea = 'Pendiente'` y usuario es receptor
   - UPDATE `TAREA` SET `estado_tarea = 'Completado'`, `fecha_completada_tarea = GETDATE()`

---

## 🚨 Validaciones Obligatorias

| Operación | Validación |
|-----------|------------|
| **Subir Archivo** | 1. `estado_tarea` != "Completado" ni "Archivado"<br>2. Si pasó `fecha_limite_tarea`, verificar `permite_entrega_con_retraso = 1` |
| **Completar Tarea** | 1. Usuario actual = `usuario_receptor_id`<br>2. `estado_tarea = 'Pendiente'` |
| **Agregar Comentario** | 1. Usuario activo (`es_activo_usuario = 1`)<br>2. Empleado activo (`es_activo_empleado = 1`) |
| **Ver Tareas** | 1. Filtrar por `usuario_receptor_id` o `usuario_emisor_id` según rol |

---

## 📊 Queries Útiles para Estadísticas

```sql
-- Total de tareas pendientes del usuario
SELECT COUNT(*)
FROM TAREA
WHERE usuario_receptor_id = @IdUsuario
  AND estado_tarea = 'Pendiente';

-- Total de tareas vencidas
SELECT COUNT(*)
FROM TAREA
WHERE usuario_receptor_id = @IdUsuario
  AND estado_tarea NOT IN ('Completado', 'Archivado')
  AND fecha_limite_tarea < GETDATE();

-- Total de adjuntos de una tarea
SELECT COUNT(*)
FROM TAREA_ADJUNTO
WHERE tarea_id = @IdTarea;

-- Total de comentarios de una tarea
SELECT COUNT(*)
FROM TAREA_COMENTARIO
WHERE tarea_id = @IdTarea;
```

---

## 📝 Ejemplo Completo: Flujo de Scanner en App Móvil

```
1. Usuario abre app móvil
2. Login → Guardar id_usuario en sesión
3. Pantalla principal: Lista de tareas asignadas
4. Usuario selecciona una tarea
5. Pantalla detalle de tarea:
   - Muestra: título, descripción, estado, prioridad, fechas
   - Lista de adjuntos existentes
   - Lista de comentarios

6. Usuario presiona "Escanear Documento"
7. Validación CRÍTICA:
   IF (tarea.estado == "Completado" || tarea.estado == "Archivado") {
       Mostrar error: "No se pueden subir archivos en tareas completadas"
       RETURN
   }

   IF (tarea.fechaLimite != null && ahora > tarea.fechaLimite) {
       IF (!tarea.permiteEntregaConRetraso) {
           Mostrar error: "Esta tarea tiene cierre estricto. No acepta entregas tardías"
           RETURN
       }
   }

8. Abrir cámara en modo scanner
9. Capturar imagen → Procesar → Convertir a PDF
10. Mostrar preview del PDF al usuario
11. Usuario confirma "Subir"
12. Convertir PDF a byte[]
13. Ejecutar INSERT en TAREA_ADJUNTO:
    - id_tarea_adjunto = NEWID()
    - tarea_id = tarea.id
    - nombre_archivo = "scan_2024_01_15.pdf"
    - tipo_archivo = ".pdf"
    - archivo = byte[]
    - fecha_subida = GETDATE()
    - id_usuario_subio = usuarioActual.id

14. Si INSERT exitoso:
    - Mostrar mensaje: "Documento subido exitosamente"
    - Refrescar lista de adjuntos
    - Opcional: Agregar comentario automático "Documento escaneado desde app móvil"

15. Usuario puede:
    - Seguir escaneando más documentos
    - Agregar comentarios
    - Completar la tarea
    - Salir
```

---

## 🔐 Seguridad

1. **NO exponer credenciales de BD en la app**
   - Usar API REST intermedia
   - Tokens JWT para autenticación

2. **Validar permisos en CADA operación**
   - Usuario activo
   - Usuario es receptor/emisor según acción

3. **Limitar tamaño de archivos**
   - Max 10MB por archivo
   - Validar en app antes de subir

4. **Sanitizar inputs**
   - Evitar SQL Injection
   - Usar queries parametrizadas

---

**Fin de la Documentación**

Para más dudas, consulta el código fuente en:
- `Hermes/Models/Tarea.cs`
- `Hermes/Models/TareaAdjunto.cs`
- `Hermes/Services/TareaAdjuntoService.cs`
- `Hermes/ViewModels/DetalleTareaViewModel.cs`
