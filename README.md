# Converter

Herramienta para Windows que añade al **menú contextual** (clic derecho) la opción **"Convertir a..."** en archivos de audio y video. Un solo instalador (.exe) instala en Program Files, aparece en Agregar o quitar programas, menú Inicio, y incluye términos de servicio, política de privacidad, acerca de y contacto.

## Para el usuario final

1. Ejecutar **Converter-Setup.exe** (el instalador pedirá permisos de administrador).
2. Pulsar **"Instalar"**. El instalador descarga FFmpeg, instala en *Program Files*, registra el menú contextual y crea accesos en Inicio.
3. Uso: **clic derecho** en cualquier audio o video → **"Convertir a..."** → elegir formato (MP3, MP4, etc.).
4. Desinstalar: **Inicio → Converter → Desinstalar Converter**, o **Configuración → Aplicaciones → Converter → Desinstalar**.

En la carpeta de instalación (*Program Files\Converter*) quedan los documentos: Términos de servicio, Política de privacidad, Acerca de, Contacto y LICENSE (GPL v3).

## Generar el instalador (desarrollador)

En **Windows** con **.NET 8 SDK** instalado:

1. Abrir la carpeta del proyecto `converter`.
2. Ejecutar:
   ```
   Build-Instalador.bat
   ```
3. Se generará **Converter-Setup.exe** en la raíz del proyecto. Ese es el único archivo a repartir.

El script compila en orden: Converter → UninstallConverter → ConverterInstaller (que incluye Converter, desinstalador y documentos).

## Estructura del proyecto

- **Converter** – Aplicación que realiza la conversión (menú contextual).
- **ConverterInstaller** – Instalador con UI: instala en Program Files, registra desinstalación, documentos, Inicio.
- **ConverterUninstaller** – Desinstalador (quita menú contextual y entrada en Agregar o quitar programas, borra la carpeta).
- **install/** – Scripts PowerShell alternativos (instalación manual sin el instalador completo).
- **Recursos/** – HTML/TXT de términos, privacidad, acerca de, contacto y LICENSE.

## Licencia

Converter se distribuye bajo **GNU GPL v3**. FFmpeg tiene su propia licencia (GPL/LGPL según la compilación). Véase LICENSE en la carpeta de instalación o en el repositorio.

## Icono

El icono del programa está en `icon.ico` (y `icon.png`). Para regenerar `icon.ico` desde `icon.png`:

```bash
python3 -c "from PIL import Image; Image.open('icon.png').save('icon.ico', format='ICO', sizes=[(256,256),(48,48),(32,32),(16,16)])"
```
