# Plantilla de Excel para Reportes

## Ubicación del archivo

Coloca tu archivo `Tareas.xlsx` en esta carpeta (`Resources/Plantillas/`).

## Configuración de la plantilla

El sistema actualizará automáticamente las siguientes celdas con datos de la base de datos:

- **Celda B2**: Cantidad de tareas con estado **PENDIENTE**
- **Celda B3**: Cantidad de tareas con estado **COMPLETADO**
- **Celda B4**: Cantidad de tareas con estado **VENCIDO**

## Cómo crear la plantilla

1. Crea un archivo Excel llamado `Tareas.xlsx`
2. En la celda A2 escribe "Pendiente", en A3 "Completado", en A4 "Vencido"
3. Las celdas B2, B3, B4 pueden contener valores de ejemplo (serán sobrescritos)
4. Crea los gráficos que desees usando los datos de las celdas B2:B4
   - Por ejemplo, un gráfico de torta con el rango A2:B4
   - O un gráfico de barras con el mismo rango
5. Guarda el archivo como `Tareas.xlsx` en esta carpeta

## Funcionamiento

Cuando generes un reporte desde el Dashboard:

1. El sistema copiará esta plantilla al Escritorio
2. Actualizará las celdas B2, B3, B4 con los datos reales de la base de datos
3. Los gráficos se actualizarán automáticamente porque están vinculados a esas celdas
4. El archivo se abrirá automáticamente en Excel

## Ejemplo de estructura

| A         | B     |
|-----------|-------|
| Pendiente | 10    |
| Completado| 25    |
| Vencido   | 3     |

Con esta estructura, puedes crear cualquier tipo de gráfico (torta, barras, columnas, etc.) y el sistema mantendrá el formato y los gráficos, solo actualizando los números.
