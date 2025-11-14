# Plantillas de Excel para Reportes

## Ubicación de los archivos

Coloca tus archivos de plantillas en esta carpeta (`Resources/Plantillas/`):

- `Tareas.xlsx` - Plantilla para reporte por estado de tareas
- `Prioridades.xlsx` - Plantilla para reporte por prioridad
- `Top3.xlsx` - Plantilla para Top 3 usuarios con más tareas finalizadas

## 1. Plantilla de Tareas por Estado (`Tareas.xlsx`)

### Configuración de celdas

El sistema actualizará automáticamente las siguientes celdas:

- **Celda B2**: Cantidad de tareas con estado **PENDIENTE**
- **Celda B3**: Cantidad de tareas con estado **COMPLETADO**
- **Celda B4**: Cantidad de tareas con estado **VENCIDO**

### Cómo crear la plantilla

1. Crea un archivo Excel llamado `Tareas.xlsx`
2. En la celda A2 escribe "Pendiente", en A3 "Completado", en A4 "Vencido"
3. Las celdas B2, B3, B4 pueden contener valores de ejemplo (serán sobrescritos)
4. Crea los gráficos que desees usando los datos de las celdas B2:B4
   - Por ejemplo, un gráfico de torta con el rango A2:B4
   - O un gráfico de barras con el mismo rango
5. Guarda el archivo como `Tareas.xlsx` en esta carpeta

### Ejemplo de estructura

| A         | B     |
|-----------|-------|
| Pendiente | 10    |
| Completado| 25    |
| Vencido   | 3     |

## 2. Plantilla de Prioridades (`Prioridades.xlsx`)

### Configuración de celdas

El sistema actualizará automáticamente las siguientes celdas:

- **Celda B2**: Cantidad de tareas con prioridad **ALTA**
- **Celda B3**: Cantidad de tareas con prioridad **MEDIA**
- **Celda B4**: Cantidad de tareas con prioridad **BAJA**

### Cómo crear la plantilla

1. Crea un archivo Excel llamado `Prioridades.xlsx`
2. En la celda A2 escribe "Alta", en A3 "Media", en A4 "Baja"
3. Las celdas B2, B3, B4 pueden contener valores de ejemplo (serán sobrescritos)
4. Crea los gráficos que desees usando los datos de las celdas B2:B4
   - Por ejemplo, un gráfico de torta con el rango A2:B4
   - O un gráfico de barras con el mismo rango
5. Guarda el archivo como `Prioridades.xlsx` en esta carpeta

### Ejemplo de estructura

| A     | B     |
|-------|-------|
| Alta  | 15    |
| Media | 20    |
| Baja  | 8     |

## 3. Plantilla de Top 3 Usuarios (`Top3.xlsx`)

### Configuración de celdas

El sistema actualizará automáticamente las siguientes celdas con datos del Top 3 de usuarios que tienen más tareas finalizadas:

**Usuario #1 (el que más tareas finalizó):**
- **Celda A2**: Nombre completo del usuario
- **Celda B2**: Total de tareas asignadas a este usuario
- **Celda C2**: Cantidad de tareas finalizadas por este usuario

**Usuario #2 (segundo lugar):**
- **Celda A3**: Nombre completo del usuario
- **Celda B3**: Total de tareas asignadas a este usuario
- **Celda C3**: Cantidad de tareas finalizadas por este usuario

**Usuario #3 (tercer lugar):**
- **Celda A4**: Nombre completo del usuario
- **Celda B4**: Total de tareas asignadas a este usuario
- **Celda C4**: Cantidad de tareas finalizadas por este usuario

### Cómo crear la plantilla

1. Crea un archivo Excel llamado `Top3.xlsx`
2. En la celda A1 escribe "Usuario", B1 "Total Tareas", C1 "Finalizadas"
3. Las celdas A2-A4, B2-B4, C2-C4 pueden contener valores de ejemplo (serán sobrescritos)
4. Crea los gráficos que desees usando los datos de las celdas A2:C4
   - Por ejemplo, un gráfico de barras comparativo
   - O un gráfico de columnas agrupadas
5. Guarda el archivo como `Top3.xlsx` en esta carpeta

### Ejemplo de estructura

| A              | B     | C     |
|----------------|-------|-------|
| Usuario        | Total | Finalizadas |
| Juan Pérez     | 50    | 45    |
| María García   | 42    | 38    |
| Carlos López   | 35    | 30    |

## Funcionamiento

Cuando generes un reporte desde el Dashboard:

1. El sistema copiará la plantilla correspondiente al Escritorio
2. Actualizará las celdas específicas con los datos reales de la base de datos
3. Los gráficos se actualizarán automáticamente porque están vinculados a esas celdas
4. El archivo se abrirá automáticamente en Excel

Con estas plantillas, puedes crear cualquier tipo de gráfico (torta, barras, columnas, etc.) y el sistema mantendrá el formato y los gráficos, solo actualizando los números.
