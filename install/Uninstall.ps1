# Desinstalar entradas del menú contextual de Converter
$baseKey = "HKCU:\Software\Classes"
$extensions = @(
    ".mp4", ".avi", ".mkv", ".webm", ".mov", ".flv", ".wmv", ".m4v", ".mpeg", ".mpg",
    ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".wma", ".opus"
)

foreach ($ext in $extensions) {
    $keyPath = "$baseKey\$ext\Shell\ConverterMenu"
    if (Test-Path $keyPath) {
        Remove-Item -Path $keyPath -Recurse -Force
        Write-Host "Eliminado: $ext" -ForegroundColor Gray
    }
}

Write-Host "Converter desinstalado del menú contextual." -ForegroundColor Green
exit 0
