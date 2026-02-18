@echo off
chcp 65001 >nul
echo ============================================
echo   Converter - Generar instalador único
echo ============================================
echo.

set "ROOT=%~dp0"
cd /d "%ROOT%"

echo [1/3] Compilando Converter...
dotnet publish Converter.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
if %ERRORLEVEL% neq 0 (
    echo Error al compilar Converter.
    pause
    exit /b 1
)

if not exist "publish\Converter.exe" (
    echo No se generó publish\Converter.exe
    pause
    exit /b 1
)

echo.
echo [2/3] Compilando desinstalador...
dotnet publish ConverterUninstaller\ConverterUninstaller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish_uninstaller
if %ERRORLEVEL% neq 0 (
    echo Error al compilar el desinstalador.
    pause
    exit /b 1
)

if not exist "publish_uninstaller\UninstallConverter.exe" (
    echo No se generó UninstallConverter.exe
    pause
    exit /b 1
)

echo.
echo [3/3] Generando instalador (Program Files, Agregar o quitar programas, documentos, Inicio)...
dotnet publish ConverterInstaller\ConverterInstaller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
if %ERRORLEVEL% neq 0 (
    echo Error al compilar el instalador.
    pause
    exit /b 1
)

if not exist "dist\ConverterInstaller.exe" (
    echo No se generó el instalador.
    pause
    exit /b 1
)

copy /Y "dist\ConverterInstaller.exe" "Converter-Setup.exe" >nul
echo.
echo ============================================
echo   Listo.
echo ============================================
echo.
echo   Instalador generado: Converter-Setup.exe
echo   (en la carpeta del proyecto)
echo.
echo   Reparte o copia solo ese archivo a Windows y
echo   ejecútalo para instalar Converter. El instalador
echo   descargará FFmpeg automáticamente.
echo.
pause
