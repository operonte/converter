using Microsoft.Win32;

// Uso: UninstallConverter.exe [carpeta_instalacion]
// Si no se pasa carpeta, se intenta leer del registro de desinstalación.

const string UninstallGuid = "{B8E8F5A0-1C2D-4E5F-9A0B-3C4D5E6F7890}";
string installDir = args.Length > 0 ? args[0].Trim().Trim('"') : "";

if (string.IsNullOrEmpty(installDir))
{
    try
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallGuid}");
        installDir = key?.GetValue("InstallLocation") as string ?? "";
    }
    catch { }
}

if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
{
    // Sin interfaz: salir en silencio si no hay qué desinstalar
    return 0;
}

// Quitar menú contextual (HKCU)
string[] extensions = {
    ".mp4", ".avi", ".mkv", ".webm", ".mov", ".flv", ".wmv", ".m4v", ".mpeg", ".mpg",
    ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".wma", ".opus"
};
foreach (string ext in extensions)
{
    try
    {
        string keyPath = $@"Software\Classes\{ext}\Shell\ConverterMenu";
        Registry.CurrentUser.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
        Registry.LocalMachine.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
    }
    catch { }
}

// Quitar entrada de Agregar o quitar programas (HKLM)
try
{
    Registry.LocalMachine.DeleteSubKeyTree($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallGuid}", throwOnMissingSubKey: false);
}
catch { }

// Eliminar carpeta tras cerrar este proceso: crear batch en temp y ejecutarlo
string batchPath = Path.Combine(Path.GetTempPath(), "ConverterUninstall_" + Guid.NewGuid().ToString("N")[..8] + ".bat");
string batchContent = @"@echo off
timeout /t 2 /nobreak >nul
rmdir /s /q """ + installDir.Replace("\"", "\"\"") + @"""
del /f /q ""%~f0""";
File.WriteAllText(batchPath, batchContent);
var startInfo = new System.Diagnostics.ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c \"\"{batchPath}\"\"",
    UseShellExecute = false,
    CreateNoWindow = true
};
System.Diagnostics.Process.Start(startInfo);
return 0;
