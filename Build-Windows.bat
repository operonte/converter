@echo off
REM Genera Converter.exe para Windows (ejecutar en Windows con .NET SDK instalado)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
echo Listo. Ejecutable en: publish\Converter.exe
echo Copia ffmpeg.exe en esa carpeta y ejecuta install\Ejecutar instalador.bat
pause
