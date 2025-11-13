Especificación Técnica y Requerimientos de 
Hardware 
Table of Contents 
Proyecto Integrador C# WPF/.NET 8.0 
1. Arquitectura del Sistema 
2. Stack Tecnológico Detallado 
3. Requerimientos de Hardware 
4. Mapeo de Requerimientos Funcionales 
5. Consideraciones de Seguridad 
6. Plan de Implementación 
7. Monitoreo y Mantenimiento 
8. Costos Proyectados 
9. Conclusiones y Recomendaciones 
Sistema de Gestión Inmobiliaria – Proyecto Integrador C# WPF/.NET 8.0 
Resumen Ejecutivo 
Este documento describe la arquitectura, stack tecnológico, requerimientos de 
hardware y las 
mejores prácticas para el desarrollo y despliegue de una aplicación de escritorio 
inmobiliaria 
implementada en C# utilizando WPF sobre .NET 8.0, con base de datos SQL Server 
2022. Incluye 
aspectos funcionales, recomendaciones de seguridad, y lineamientos para trabajo 
colaborativo en 
LAN. 
1. Arquitectura del Sistema 
El sistema se basa en una arquitectura cliente-servidor. Cada estación ejecuta la 
aplicación de 
escritorio (C# + WPF), conectada a un servidor central (SQL Server 2022) a través de 
la LAN. La 
capa de presentación (UI) es manejada por WPF, la lógica de negocio en C#/.NET 8.0 y 
se adopta 
Entity Framework para el acceso seguro a datos. Se recomienda separar credenciales 
y usar 
autenticación por usuario para cumplir requisitos de seguridad y trazabilidad. 
2. Stack Tecnológico Detallado 
Lenguaje principal: C# 
Tecnología de UI: WPF (Windows Presentation Foundation) 
Runtime: .NET 8.0 
ORM: Entity Framework Core 8 
Base de datos: SQL Server 2022 (Express/Standard, según escala) 
Sistema operativo: Windows 10 (v1909+) o Windows 11 
IDE recomendado: Visual Studio 2022 
Control de versiones: Git 
3. Requerimientos de Hardware 
Estación de Trabajo (Desarrollo/Usuario): 
Procesador: Intel Core i3 (o AMD equivalente) mínimo; Core i5+ recomendado 
RAM: 4 GB mínimo, 8 GB recomendado 
Almacenamiento: 128 GB SSD mínimo; preferible 256 GB SSD 
GPU: DirectX 9c compatible (DirectX 11 recomendado para mejor performance en 
gráficos WPF) 
Resolución: 1366x768 mínimo; recomendado 1920x1080 
Red: Ethernet (Gigabit recomendado) o Wi-Fi estable 
Servidor SQL: 
Procesador: Intel Core i5+ o Xeon/Epyc, mínimo 1.4 GHz, recomendado 2.0 GHz+ 
RAM: 4 GB mínimo (Express); 8 GB+ recomendado 
Almacenamiento: 256 GB SSD mínimo (mayor según el tamaño esperado de la base 
de datos) 
Sistema operativo: Windows 10/11, Windows Server 2016/2019/2022 
Red: Ethernet preferente 
Monitor: 800x600 mínimo 
Consideraciones de Hardware Adicionales 
Se recomienda SSD por la mejora sustancial en tiempos de acceso y confiabilidad 
En entornos con más de 20 usuarios simultáneos, considerar 16 GB de RAM en el 
servidor 
[1] 
[2] 
[3] 
[4] 
Para estaciones de desarrollo con Visual Studio, lo ideal son 16 GB de RAM y 
procesador quad 
core 
4. Mapeo de Requerimientos Funcionales 
Gestión de propiedades, empleados y clientes 
Control de acceso por roles: usuario, bróker, asesor, secretaria, administrativos 
Seguimiento y auditoría de operaciones críticas (bitácora) 
Gestión y almacenamiento seguro de adjuntos (documentos, imágenes) 
Sistema de notificación interna 
5. Consideraciones de Seguridad 
[5] 
[6] 
Implementar autenticación y autorización basada en roles en la aplicación y en el 
acceso a SQL 
Server 
Uso de contraseñas robustas y políticas de expiración 
Limitar conexiones de clientes por IP o subred LAN 
Cifrado de datos sensibles en tránsito (LAN segmentada o IPSec interno si es 
necesario) 
Auditoría de accesos y cambios en base de datos (triggers o funciones de SQL) 
Uso de parámetros en consultas para evitar SQL Injection 
Políticas de backup locales y copia fuera del servidor en almacenamiento seguro 
6. Plan de Implementación 
Instalación de SQL Server en servidor central y configuración de accesos por usuario 
Configuración y despliegue del app en cada equipo mediante instalador 
(MSI/ClickOnce) 
Configuración del firewall en servidor para aceptar solo conexiones LAN 
Pruebas end-to-end y capacitación al usuario final 
7. Monitoreo y Mantenimiento 
Monitoreo del rendimiento del servidor (RAM, CPU, red, espacio en disco) 
Revisión programada de logs y bitácora 
Validación periódica de backups y restauración 
Actualizaciones de la app y de las dependencias .NET y SQL Server 
8. Costos Proyectados 
Hardware: actualización opcional de equipos antiguos, adquisición de SSDs, servidor 
Software: licencias de Windows y Visual Studio (edición gratuita para equipos 
pequeños) 
Soporte y mantenimiento 
El stack elegido permite costos reducidos si se opta por versiones 
Express/Community de 
herramientas principales 
9. Conclusiones y Recomendaciones 
El uso de C# y WPF sobre .NET 8.0 asegura máxima compatibilidad y soporte futuro 
La arquitectura basada en LAN y SQL Server centralizado es robusta, escalable y 
sencilla de 
administrar para equipos pequeños o medianos 
Mantener siempre buenas prácticas de seguridad, actualizaciones y auditoría


