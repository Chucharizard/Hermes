# Plantillas de Excel para Reportes

## Ubicación de los archivos

Coloca tus archivos de plantillas en esta carpeta (`Resources/Plantillas/`):

- `Tareas.xlsx` - Plantilla para reporte por estado de tareas
- `Prioridades.xlsx` - Plantilla para reporte por prioridad

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

## Funcionamiento

Cuando generes un reporte desde el Dashboard:

1. El sistema copiará la plantilla correspondiente al Escritorio
2. Actualizará las celdas B2, B3, B4 con los datos reales de la base de datos
3. Los gráficos se actualizarán automáticamente porque están vinculados a esas celdas
4. El archivo se abrirá automáticamente en Excel

Con estas plantillas, puedes crear cualquier tipo de gráfico (torta, barras, columnas, etc.) y el sistema mantendrá el formato y los gráficos, solo actualizando los números.
