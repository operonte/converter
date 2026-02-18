# Instalador de Converter - Menú contextual en Windows
# Ejecutar como usuario (no requiere admin). Registra "Convertir a..." en clic derecho sobre audio/video.
# Requiere: PowerShell 5.1 o superior. Ejecutar desde la carpeta donde está Converter.exe y ffmpeg.exe

$ErrorActionPreference = "Stop"
$appName = "Converter"
$exeName = "Converter.exe"

# Ruta del ejecutable: busca en carpeta padre o en padre\publish
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$installRoot = Split-Path -Parent $scriptDir
$exePath = Join-Path $installRoot $exeName
if (-not (Test-Path $exePath)) {
    $exePath = Join-Path (Join-Path $installRoot "publish") $exeName
}

if (-not (Test-Path $exePath)) {
    Write-Host "No se encontró $exeName en: $installRoot" -ForegroundColor Red
    Write-Host "Coloca este script en la subcarpeta 'install' dentro de la carpeta del programa." -ForegroundColor Yellow
    exit 1
}

# Escapar la ruta para uso en registro (barras invertidas dobles en reg)
$exePathEscaped = $exePath -replace '\\', '\\'

$extensions = @(
    ".mp4", ".avi", ".mkv", ".webm", ".mov", ".flv", ".wmv", ".m4v", ".mpeg", ".mpg",
    ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".wma", ".opus"
)

$formats = @(
    @{ id = "mp3";  name = "MP3" },
    @{ id = "aac";  name = "AAC" },
    @{ id = "m4a";  name = "M4A" },
    @{ id = "wav";  name = "WAV" },
    @{ id = "flac"; name = "FLAC" },
    @{ id = "ogg";  name = "OGG" },
    @{ id = "opus"; name = "Opus" },
    @{ id = "mp4";  name = "MP4" },
    @{ id = "mkv";  name = "MKV" },
    @{ id = "avi";  name = "AVI" },
    @{ id = "webm"; name = "WebM" },
    @{ id = "mov";  name = "MOV" },
    @{ id = "wmv"; name = "WMV" },
    @{ id = "flv";  name = "FLV" }
)

$baseKey = "HKCU:\Software\Classes"
$menuName = "Convertir a..."

foreach ($ext in $extensions) {
    $keyPath = "$baseKey\$ext\Shell\ConverterMenu"
    if (-not (Test-Path $keyPath)) { New-Item -Path $keyPath -Force | Out-Null }
    Set-ItemProperty -Path $keyPath -Name "MUIVerb" -Value $menuName -Type String -Force
    Set-ItemProperty -Path $keyPath -Name "SubCommands" -Value "" -Type String -Force

    foreach ($f in $formats) {
        $cmdPath = "$keyPath\shell\$($f.id)\command"
        if (-not (Test-Path $cmdPath)) { New-Item -Path $cmdPath -Force | Out-Null }
        $cmd = "`"$exePath`" `"%1`" --format $($f.id)"
        Set-ItemProperty -Path $cmdPath -Name "(Default)" -Value $cmd -Type String -Force
        $verbPath = "$keyPath\shell\$($f.id)"
        Set-ItemProperty -Path $verbPath -Name "MUIVerb" -Value $f.name -Type String -Force
    }
}

Write-Host "Converter instalado correctamente." -ForegroundColor Green
Write-Host "Ruta: $exePath" -ForegroundColor Gray
Write-Host "Haz clic derecho en un archivo de audio o video y elige '$menuName'." -ForegroundColor Cyan
exit 0
