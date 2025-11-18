# Plan de Mejoras UI/UX - Sistema HERMES
## Enfoque Híbrido: Base UI + Funcionalidades Incrementales

---

## 📋 Índice
1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Mejoras Aprobadas](#mejoras-aprobadas)
3. [Estrategia de Implementación](#estrategia-de-implementación)
4. [Fase 1: Base de UI](#fase-1-base-de-ui)
5. [Fase 2: Funcionalidades Avanzadas](#fase-2-funcionalidades-avanzadas)
6. [Estimaciones de Tiempo](#estimaciones-de-tiempo)
7. [Beneficios Esperados](#beneficios-esperados)

---

## 🎯 Resumen Ejecutivo

Este documento detalla el plan de mejoras para el sistema HERMES, enfocado en mejorar significativamente la experiencia de usuario (UX) y la interfaz (UI) mediante un **enfoque híbrido**:

1. **Primero**: Sentar bases sólidas de UI con componentes reutilizables
2. **Después**: Agregar funcionalidades avanzadas sobre esa base

**Objetivo:** Evitar refactorización masiva futura y asegurar consistencia visual desde el inicio.

**Tiempo estimado total:** 6-8 días de desarrollo

---

## ✅ Mejoras Aprobadas

### 1. **Cards Interactivos en Dashboard** ⭐
**Descripción:** Hacer que las estadísticas del dashboard sean clickeables para navegar directamente a listas filtradas.

**Estado actual:**
```
Dashboard muestra:
- Total Tareas: 18
- Pendientes: 14
- Completadas: 3
- Vencidas: 1
(sin interacción)
```

**Mejora:**
```
Click en "14 Pendientes"
→ Abre BandejaTareasRecibidas
→ Con filtro automático: estado = "Pendiente"

Click en "1 Vencida"
→ Abre BandejaTareasRecibidas
→ Con filtro: tareas vencidas
```

**Componentes afectados:**
- `DashboardView.xaml`
- `DashboardViewModel.cs`
- `BandejaTareasRecibidasViewModel.cs` (agregar método `FiltrarPor()`)

**Prioridad:** ALTA
**Complejidad:** Media
**Tiempo estimado:** 4-6 horas

---

### 2. **Panel de Notificaciones Visuales** 🔔
**Descripción:** Agregar un panel prominente en la parte superior del dashboard que muestre alertas importantes.

**Diseño propuesto:**
```
┌─────────────────────────────────────────────────────────────┐
│ 🔴 Tienes 3 tareas que vencen HOY                          │
│ ⚠️ 2 tareas vencidas sin completar                         │
│ 📨 Nuevo comentario en: "Preparar documentos legales"      │
└─────────────────────────────────────────────────────────────┘
```

**Tipos de notificaciones:**
- 🔴 **Crítico**: Tareas que vencen HOY
- ⚠️ **Advertencia**: Tareas vencidas
- 📨 **Info**: Nuevos comentarios (futuro)
- ✅ **Éxito**: Tareas completadas recientemente

**Lógica:**
```csharp
// Calcular notificaciones
var vencenHoy = tareas.Where(t =>
    t.FechaLimiteTarea.HasValue &&
    t.FechaLimiteTarea.Value.Date == DateTime.Today &&
    t.EstadoTarea == "Pendiente"
).Count();

var vencidas = tareas.Where(t =>
    t.EstaVencida &&
    t.EstadoTarea != "Completado"
).Count();
```

**Componentes nuevos:**
- `Controls/NotificationPanel.xaml` (componente reutilizable)
- `NotificationPanelViewModel.cs`

**Prioridad:** ALTA
**Complejidad:** Media
**Tiempo estimado:** 6-8 horas

---

### 3. **Preview de Adjuntos Integrado** 📄
**Descripción:** Mostrar vista previa de PDFs e imágenes directamente en la ventana de detalle, sin necesidad de descargar.

**Funcionalidades:**
- **PDFs**: Visor integrado (usando componente WPF o librería)
- **Imágenes**: Thumbnail + vista ampliada al click
- **Otros archivos**: Ícono + opción de descargar

**Diseño propuesto:**
```
Tab "Adjuntos"
├── documento.pdf
│   └── [Vista previa integrada del PDF]
│   └── [Descargar] [Eliminar]
│
├── imagen.jpg
│   └── [Thumbnail 200x200]
│   └── [Ver completo] [Descargar] [Eliminar]
│
└── archivo.xlsx
    └── [Ícono Excel]
    └── [Descargar] [Eliminar]
```

**Librerías recomendadas:**
- **PDFs**: `PdfiumViewer` o `MoonPdfPanel`
- **Imágenes**: Control nativo `Image` de WPF

**Componentes afectados:**
- `DetalleTareaWindow.xaml` (tab Adjuntos)
- Crear `Controls/PdfViewer.xaml`
- Crear `Controls/ImagePreview.xaml`

**Prioridad:** ALTA
**Complejidad:** Alta
**Tiempo estimado:** 8-10 horas

---

### 4. **Drag & Drop para Adjuntos** 🎯
**Descripción:** Permitir arrastrar y soltar archivos directamente sobre la ventana de detalle para subirlos.

**Opciones simultáneas:**
1. ✅ **Click en botón** "Seleccionar Archivo" → File Dialog (mantener)
2. ✅ **Drag & Drop** archivo sobre zona de adjuntos → Subida automática

**Implementación:**
```xaml
<!-- Zona de drop -->
<Border AllowDrop="True"
        Drop="Border_Drop"
        DragOver="Border_DragOver">
    <TextBlock Text="Arrastra archivos aquí o haz click en 'Seleccionar Archivo'"
               FontStyle="Italic"
               Foreground="#95A5A6"/>
</Border>
```

```csharp
private async void Border_Drop(object sender, DragEventArgs e)
{
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var file in files)
        {
            await SubirArchivoAsync(file);
        }
    }
}
```

**Componentes afectados:**
- `DetalleTareaWindow.xaml`
- `DetalleTareaViewModel.cs` (refactorizar método SubirAdjunto)

**Prioridad:** MEDIA
**Complejidad:** Baja
**Tiempo estimado:** 3-4 horas

---

### 5. **Historial de Cambios (Audit Log)** 📝
**Descripción:** Registrar y mostrar quién hizo qué cambio en cada tarea y cuándo.

**Eventos a registrar:**
- ✏️ Cambio de estado
- 🎯 Cambio de prioridad
- 📎 Subida de archivo
- 💬 Nuevo comentario
- 👤 Reasignación de tarea
- 📅 Cambio de fechas

**Diseño de visualización:**
```
Tab "Historial"
├── 📝 15/01/2024 14:30 - Juan cambió prioridad: Media → Alta
├── 📎 15/01/2024 15:00 - María subió archivo: contrato.pdf
├── 💬 15/01/2024 15:30 - Juan agregó un comentario
├── ✅ 15/01/2024 16:45 - Pedro completó la tarea
└── 📦 16/01/2024 09:00 - Juan archivó la tarea
```

**Estructura de base de datos:**
```sql
CREATE TABLE TAREA_HISTORIAL (
    id_historial UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tarea_id UNIQUEIDENTIFIER NOT NULL,
    usuario_id UNIQUEIDENTIFIER NOT NULL,
    accion NVARCHAR(100) NOT NULL,
    detalle NVARCHAR(500),
    fecha_cambio DATETIME2 NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (tarea_id) REFERENCES TAREA(id_tarea),
    FOREIGN KEY (usuario_id) REFERENCES USUARIO(id_usuario)
);
```

**Permisos de visualización:**
- ✅ Todos pueden ver historial de sus tareas asignadas
- ✅ Broker puede ver historial de TODAS las tareas
- ✅ Emisor puede ver historial de las tareas que creó

**Componentes nuevos:**
- Modelo: `TareaHistorial.cs`
- Servicio: `TareaHistorialService.cs`
- Tab en: `DetalleTareaWindow.xaml`

**Prioridad:** MEDIA-ALTA
**Complejidad:** Media-Alta
**Tiempo estimado:** 10-12 horas

---

### 6. **Vista de Calendario** 📅
**Descripción:** Visualizar todas las tareas en un calendario mensual interactivo.

**Diseño propuesto:**
```
┌─────────────────────────────────────────────────────────────┐
│                      Enero 2024                              │
│  L    M    M    J    V    S    D                            │
│  1    2    3    4    5    6    7                            │
│  🔴  🟢  ⚪  🔴  🟢   -    -                                │
│  8    9   10   11   12   13   14                            │
│  ⚪  🔴  🟡  🟢  ⚪   -    -                                │
└─────────────────────────────────────────────────────────────┘

Leyenda:
🔴 Tareas de prioridad Alta
🟡 Tareas de prioridad Media
🟢 Tareas completadas
⚪ Tareas pendientes
```

**Interacciones:**
- Click en un día → Mostrar lista de tareas de ese día en panel lateral
- Doble click en día → Crear nueva tarea con esa fecha
- Hover sobre día → Tooltip con resumen rápido

**Librerías recomendadas:**
- Crear control personalizado
- O usar: `ModernWpf.Controls.CalendarView`

**Componentes nuevos:**
- `Views/CalendarioTareasView.xaml`
- `ViewModels/CalendarioTareasViewModel.cs`
- `Controls/CalendarDayCell.xaml` (control personalizado)

**Prioridad:** MEDIA
**Complejidad:** Alta
**Tiempo estimado:** 12-16 horas

---

### 7. **Atajos de Teclado (Quick Actions)** ⌨️
**Descripción:** Agregar comandos de teclado para acciones frecuentes.

**Atajos propuestos:**
```
Ctrl+N → Nueva tarea
Ctrl+F → Buscar (enfoca barra de búsqueda)
Ctrl+D → Ir a Dashboard
Ctrl+R → Refrescar vista actual
Ctrl+S → Guardar (en formularios)
Esc    → Cerrar ventana modal/Cancelar
Enter  → En lista de tareas: Abrir detalle
F5     → Actualizar datos
```

**Implementación en MainWindow:**
```xaml
<Window.InputBindings>
    <KeyBinding Key="N" Modifiers="Control" Command="{Binding NuevaTareaCommand}"/>
    <KeyBinding Key="F" Modifiers="Control" Command="{Binding EnfocarBusquedaCommand}"/>
    <KeyBinding Key="D" Modifiers="Control" Command="{Binding IrDashboardCommand}"/>
    <KeyBinding Key="R" Modifiers="Control" Command="{Binding RefrescarCommand}"/>
    <KeyBinding Key="F5" Command="{Binding ActualizarDatosCommand}"/>
</Window.InputBindings>
```

**Indicador visual:**
```
Agregar tooltips que mencionen el atajo:
"Nueva Tarea (Ctrl+N)"
"Buscar (Ctrl+F)"
```

**Componentes afectados:**
- `MainWindow.xaml`
- `MainViewModel.cs`
- Cada vista específica (para Enter en listas)

**Prioridad:** BAJA-MEDIA
**Complejidad:** Baja
**Tiempo estimado:** 2-3 horas

---

### 8. **Tabs Organizados en Detalle de Tarea** 📑
**Descripción:** Reorganizar la ventana de detalle de tarea usando tabs para mejor organización.

**Problema actual:**
- Scroll largo para ver comentarios y adjuntos
- Información desorganizada visualmente

**Solución con Tabs:**
```
┌─────────────────────────────────────────────────────────────┐
│ [General] [Adjuntos (5)] [Comentarios (12)] [Historial]    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  [Contenido del tab seleccionado]                           │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Tab "General":**
- Título, descripción, estado, prioridad
- Fechas (inicio, límite, completada)
- Usuario emisor y receptor
- Configuración de entrega con retraso
- Botones de acción (Completar, Devolver, etc.)

**Tab "Adjuntos":**
- Lista de archivos
- Vista previa integrada (mejora #3)
- Zona de drag & drop (mejora #4)
- Botones: Subir, Descargar, Eliminar

**Tab "Comentarios":**
- Lista de comentarios con timestamp
- Campo para nuevo comentario
- Botón "Agregar Comentario"

**Tab "Historial":**
- Timeline de cambios (mejora #5)
- Filtros por tipo de evento
- Solo visible si hay historial

**Componentes afectados:**
- `DetalleTareaWindow.xaml` (refactorización completa)
- `DetalleTareaViewModel.cs` (organización de propiedades)

**Prioridad:** ALTA
**Complejidad:** Media
**Tiempo estimado:** 6-8 horas

---

### 9. **Cards Enriquecidos en Bandeja de Tareas** 🎴
**Descripción:** Reemplazar lista simple de tareas por cards visualmente ricos con información de un vistazo.

**Diseño actual:**
```
Simple ListView con:
- Título
- Estado
```

**Diseño mejorado:**
```
┌───────────────────────────────────────────────────────────┐
│ 🔴 ALTA | Preparar informe mensual                       │
│ Vence: Hoy a las 17:00 (⏰ quedan 3h)                    │
│ Emisor: Juan Pérez | Receptor: Tú                        │
│ 📎 2 adjuntos | 💬 5 comentarios                         │
│ [Ver Detalle] [Completar]                                │
└───────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────┐
│ 🟡 MEDIA | Revisar contrato                              │
│ Vence: Mañana a las 10:00 (⏰ quedan 18h)                │
│ Emisor: María López | Receptor: Tú                       │
│ 📎 1 adjunto | 💬 2 comentarios                          │
│ [Ver Detalle] [Completar]                                │
└───────────────────────────────────────────────────────────┘
```

**Información visible en cada card:**
- 🎯 Prioridad con color e icono
- 📋 Título (truncado si es muy largo)
- ⏰ Fecha límite con countdown
- 👤 Emisor y receptor
- 📊 Contadores de adjuntos y comentarios
- 🎨 Color de borde según urgencia
- 🔘 Botones de acción rápida

**Colores por prioridad:**
```css
Alta:   Borde rojo #E74C3C, fondo #FFEBEE
Media:  Borde naranja #F39C12, fondo #FFF3E0
Baja:   Borde azul #3498DB, fondo #E3F2FD
```

**Colores por urgencia:**
```css
Vencida:           Fondo rojo claro, texto rojo oscuro
Vence hoy:         Fondo naranja claro, texto naranja oscuro
Vence en 3 días:   Fondo amarillo claro
Normal:            Fondo blanco
Completada:        Fondo verde claro
```

**Componentes nuevos:**
- `Controls/TaskCard.xaml` (componente reutilizable)
- `TaskCardViewModel.cs` (lógica del card)

**Componentes afectados:**
- `BandejaTareasRecibidasView.xaml`
- `BandejaTareasEnviadasView.xaml`
- `GestionTareasView.xaml`

**Prioridad:** ALTA
**Complejidad:** Media
**Tiempo estimado:** 8-10 horas

---

## 🏗️ Estrategia de Implementación

### Enfoque: **Híbrido**

**Principio:**
> Sentar bases sólidas de UI primero, luego agregar funcionalidades sobre esa base.

**Razones:**
1. ✅ Evita refactorización masiva futura
2. ✅ Asegura consistencia visual desde el inicio
3. ✅ Componentes reutilizables aceleran desarrollo posterior
4. ✅ Usuarios ven mejoras visuales inmediatas
5. ✅ Más fácil probar y debuggear

---

## 📅 Fase 1: Base de UI (Días 1-4)

### Objetivo: Crear arquitectura de UI sólida y reutilizable

### 1.1 Sistema de Estilos Globales (Día 1)
**Archivo:** `Resources/Styles.xaml`

**Contenido:**
```xaml
<!-- Paleta de Colores -->
<SolidColorBrush x:Key="PrimaryColor" Color="#2E86AB"/>
<SolidColorBrush x:Key="SuccessColor" Color="#62C370"/>
<SolidColorBrush x:Key="WarningColor" Color="#F39C12"/>
<SolidColorBrush x:Key="DangerColor" Color="#E74C3C"/>
<SolidColorBrush x:Key="InfoColor" Color="#3498DB"/>

<!-- Estilos de Botones -->
<Style x:Key="PrimaryButton" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource PrimaryColor}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="15,8"/>
    <!-- etc -->
</Style>

<!-- Estilos de Cards -->
<Style x:Key="CardStyle" TargetType="Border">
    <Setter Property="Background" Value="White"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="Padding" Value="15"/>
    <!-- etc -->
</Style>
```

**Tareas:**
- [x] Definir paleta de colores
- [x] Crear estilos de botones (Primary, Success, Warning, Danger)
- [x] Crear estilos de cards
- [x] Crear estilos de inputs (TextBox, ComboBox)
- [x] Crear estilos de iconos
- [x] Aplicar en App.xaml

**Tiempo estimado:** 4-6 horas

---

### 1.2 Componentes Reutilizables (Día 2)

#### TaskCard.xaml
**Ubicación:** `Controls/TaskCard.xaml`

**Propiedades:**
```csharp
public partial class TaskCard : UserControl
{
    public Tarea Tarea { get; set; }
    public ICommand VerDetalleCommand { get; set; }
    public ICommand CompletarCommand { get; set; }
}
```

**Tiempo estimado:** 4-5 horas

#### NotificationPanel.xaml
**Ubicación:** `Controls/NotificationPanel.xaml`

**Propiedades:**
```csharp
public List<Notificacion> Notificaciones { get; set; }
```

**Tiempo estimado:** 3-4 horas

---

### 1.3 Refactorizar Dashboard (Día 3)

**Mejoras:**
- ✅ Cards interactivos con estadísticas
- ✅ Panel de notificaciones en la parte superior
- ✅ Click en estadísticas navega a lista filtrada

**Archivos:**
- `DashboardView.xaml`
- `DashboardViewModel.cs`

**Tiempo estimado:** 6-8 horas

---

### 1.4 Mejorar Bandeja de Tareas (Día 4)

**Mejoras:**
- ✅ Reemplazar ListView simple por TaskCard
- ✅ Cards enriquecidos con toda la información
- ✅ Colores según prioridad y urgencia

**Archivos:**
- `BandejaTareasRecibidasView.xaml`
- `BandejaTareasEnviadasView.xaml`

**Tiempo estimado:** 6-8 horas

---

## 📅 Fase 2: Funcionalidades Avanzadas (Días 5-8)

### Objetivo: Agregar funcionalidades nuevas sobre la base sólida

### 2.1 Tabs en Detalle de Tarea (Día 5)

**Mejoras:**
- ✅ Reorganizar en tabs: General, Adjuntos, Comentarios, Historial
- ✅ Mejor organización visual

**Archivos:**
- `DetalleTareaWindow.xaml` (refactorización completa)

**Tiempo estimado:** 6-8 horas

---

### 2.2 Preview de PDFs + Drag & Drop (Día 6)

**Mejoras:**
- ✅ Visor de PDFs integrado
- ✅ Thumbnails de imágenes
- ✅ Drag & drop de archivos

**Archivos:**
- `DetalleTareaWindow.xaml` (tab Adjuntos)
- `Controls/PdfViewer.xaml`
- NuGet: `PdfiumViewer` o `MoonPdfPanel`

**Tiempo estimado:** 8-10 horas

---

### 2.3 Historial de Cambios (Día 7)

**Mejoras:**
- ✅ Crear tabla TAREA_HISTORIAL
- ✅ Registrar todos los cambios
- ✅ Mostrar timeline en tab Historial

**Archivos:**
- Modelo: `TareaHistorial.cs`
- Servicio: `TareaHistorialService.cs`
- Script SQL: `CrearTablaHistorial.sql`
- Interceptar cambios en todas las operaciones

**Tiempo estimado:** 10-12 horas

---

### 2.4 Vista de Calendario (Día 8)

**Mejoras:**
- ✅ Crear vista de calendario mensual
- ✅ Integrar TaskCard para mostrar tareas del día
- ✅ Navegación entre meses

**Archivos:**
- `Views/CalendarioTareasView.xaml`
- `ViewModels/CalendarioTareasViewModel.cs`

**Tiempo estimado:** 12-16 horas

---

### 2.5 Atajos de Teclado (Continuo)

**Mejoras:**
- ✅ Agregar input bindings en cada ventana
- ✅ Documentar atajos en tooltips

**Archivos:**
- `MainWindow.xaml`
- Cada vista que necesite atajos

**Tiempo estimado:** 2-3 horas

---

## ⏱️ Estimaciones de Tiempo

### Resumen por Fase

| Fase | Descripción | Tiempo Estimado |
|------|-------------|-----------------|
| **Fase 1** | Base de UI | 3-4 días |
| Día 1 | Sistema de estilos globales | 4-6 horas |
| Día 2 | Componentes reutilizables | 7-9 horas |
| Día 3 | Refactorizar Dashboard | 6-8 horas |
| Día 4 | Mejorar Bandeja de Tareas | 6-8 horas |
| **Fase 2** | Funcionalidades avanzadas | 3-4 días |
| Día 5 | Tabs en Detalle de Tarea | 6-8 horas |
| Día 6 | Preview PDFs + Drag & Drop | 8-10 horas |
| Día 7 | Historial de Cambios | 10-12 horas |
| Día 8 | Vista de Calendario | 12-16 horas |
| **Continuo** | Atajos de teclado | 2-3 horas |
| **TOTAL** | | **6-8 días** |

### Contingencia
- +20% buffer para pruebas y ajustes
- +10% para debugging y refinamiento

**Tiempo realista total:** 7-10 días

---

## 🎁 Beneficios Esperados

### 1. **Para el Usuario Final**
- ✅ Interfaz más moderna y atractiva
- ✅ Información importante de un vistazo
- ✅ Menos clicks para realizar acciones comunes
- ✅ Alertas proactivas de tareas importantes
- ✅ Vista previa de archivos sin descargar
- ✅ Organización visual clara (tabs, cards)
- ✅ Navegación más rápida (atajos de teclado)

### 2. **Para el Negocio**
- ✅ Mayor adopción del sistema
- ✅ Menos errores (alertas proactivas)
- ✅ Mayor productividad (acciones rápidas)
- ✅ Transparencia total (historial de cambios)
- ✅ Mejor organización del trabajo (calendario)

### 3. **Para el Desarrollo**
- ✅ Código más mantenible (componentes reutilizables)
- ✅ Consistencia visual automática
- ✅ Más fácil agregar funcionalidades futuras
- ✅ Menos bugs de UI
- ✅ Base sólida para crecer

---

## 📊 Métricas de Éxito

### Antes vs Después

| Métrica | Antes | Después (Esperado) |
|---------|-------|-------------------|
| Clicks para ver detalle de tarea | 3-4 | 1 (click en card) |
| Tiempo para encontrar tarea vencida | 30-60 seg | 5 seg (notificación visible) |
| Tiempo para ver adjunto PDF | 15-20 seg (descargar + abrir) | 2 seg (preview integrado) |
| Navegación a lista filtrada | 3-5 clicks | 1 click (card interactivo) |
| Satisfacción visual del usuario | 6/10 | 9/10 (estimado) |

---

## 🚦 Plan de Rollout

### Opción 1: Big Bang (Recomendado)
```
1. Desarrollo completo de Fase 1 (4 días)
2. Deploy de Fase 1 → Usuarios ven mejora visual inmediata
3. Desarrollo de Fase 2 (4 días)
4. Deploy de Fase 2 → Funcionalidades completas
```

### Opción 2: Incremental
```
1. Día 1-2: Estilos + Componentes
2. Día 3: Deploy Dashboard mejorado
3. Día 4: Deploy Bandeja mejorada
4. Día 5: Deploy Tabs en Detalle
5. Día 6-7: Deploy Preview + Historial
6. Día 8: Deploy Calendario
```

**Recomendación:** Opción 1 para evitar deployments frecuentes y mantener cohesión.

---

## 📝 Notas Finales

### Dependencias
- **PdfiumViewer** o **MoonPdfPanel** para preview de PDFs
- **ModernWpf** (opcional) para controles modernos

### Riesgos
- ⚠️ Preview de PDFs puede consumir memoria si archivos son grandes
- ⚠️ Refactorización de DetalleTarea requiere testing exhaustivo
- ⚠️ Historial de cambios aumentará tamaño de BD

### Mitigaciones
- ✅ Limitar tamaño de archivos a 10 MB
- ✅ Testing riguroso de cada componente
- ✅ Crear índices apropiados en tabla de historial

---

**Documento creado:** 2024-01-15
**Última actualización:** 2024-01-15
**Estado:** Aprobado - Listo para implementación
**Responsable:** Equipo de desarrollo HERMES
