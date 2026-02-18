using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

namespace ConverterInstaller;

public partial class InstaladorForm : Form
{
    private static readonly string ConverterResourceName = "ConverterInstaller.Converter.exe";
    private static readonly string UninstallerResourceName = "ConverterInstaller.UninstallConverter.exe";
    private static readonly string[] DocResourceNames = {
        "ConverterInstaller.TerminosDeServicio.html",
        "ConverterInstaller.PoliticaPrivacidad.html",
        "ConverterInstaller.AcercaDe.html",
        "ConverterInstaller.Contacto.html",
        "ConverterInstaller.LICENSE.txt"
    };
    private const string FfmpegZipUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
    private const string AppFolderName = "Converter";
    public const string UninstallGuid = "{B8E8F5A0-1C2D-4E5F-9A0B-3C4D5E6F7890}";

    public InstaladorForm()
    {
        InitializeComponent();
        Text = "Converter - Instalador";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(420, 280);
        Size = new Size(420, 280);
    }

    private async void btnInstalar_Click(object sender, EventArgs e)
    {
        btnInstalar.Enabled = false;
        lblEstado.Text = "Preparando...";
        lblEstado.ForeColor = Color.DarkBlue;
        Application.DoEvents();

        string installDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            AppFolderName);

        try
        {
            Directory.CreateDirectory(installDir);

            // 1. Extraer Converter.exe embebido
            lblEstado.Text = "Instalando Converter...";
            Application.DoEvents();
            if (!ExtractConverter(installDir))
            {
                MostrarError("No se encontró Converter.exe dentro del instalador.\n\nEjecuta \"Build-Instalador.bat\" para crear el instalador completo.");
                return;
            }

            string exePath = Path.Combine(installDir, "Converter.exe");

            // 2. Extraer desinstalador y documentos
            lblEstado.Text = "Instalando documentos y desinstalador...";
            Application.DoEvents();
            ExtractUninstaller(installDir);
            ExtractDocs(installDir);

            // 3. Descargar y extraer FFmpeg
            lblEstado.Text = "Descargando FFmpeg (puede tardar un momento)...";
            Application.DoEvents();
            if (!await DescargarYExtraerFfmpeg(installDir))
            {
                MostrarError("No se pudo descargar FFmpeg. Comprueba tu conexión a internet y vuelve a intentarlo.");
                return;
            }

            // 4. Registrar menú contextual
            lblEstado.Text = "Registrando menú contextual...";
            Application.DoEvents();
            RegistrarMenuContextual(exePath);

            // 5. Registrar en Agregar o quitar programas
            string uninstallExe = Path.Combine(installDir, "UninstallConverter.exe");
            RegistrarDesinstalacion(installDir, uninstallExe);

            // 6. Acceso en menú Inicio
            CrearAccesoInicio(installDir, exePath, uninstallExe);

            lblEstado.Text = "¡Instalación completada!";
            lblEstado.ForeColor = Color.Green;
            btnInstalar.Text = "Cerrar";
            btnInstalar.Enabled = true;
            btnInstalar.Click -= btnInstalar_Click;
            btnInstalar.Click += (_, _) => Close();

            MessageBox.Show(
                "¡Listo! Converter ya está instalado.\n\n" +
                "Cómo usarlo:\n" +
                "• Haz clic derecho en cualquier video o canción\n" +
                "• Elige \"Convertir a...\" y el formato (MP3, MP4, etc.)\n\n" +
                "Para desinstalar: Menú Inicio → Converter → Desinstalar Converter\n" +
                "o Configuración → Aplicaciones → Converter.",
                "Converter - Instalación correcta",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MostrarError("Error durante la instalación:\n" + ex.Message);
        }
    }

    private void MostrarError(string mensaje)
    {
        lblEstado.Text = "Error";
        lblEstado.ForeColor = Color.Red;
        btnInstalar.Enabled = true;
        MessageBox.Show(mensaje, "Converter - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private bool ExtractConverter(string installDir)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ConverterResourceName);
        if (stream == null) return false;
        string exePath = Path.Combine(installDir, "Converter.exe");
        using (var fs = File.Create(exePath))
            stream.CopyTo(fs);
        return File.Exists(exePath);
    }

    private void ExtractUninstaller(string installDir)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(UninstallerResourceName);
        if (stream == null) return;
        string path = Path.Combine(installDir, "UninstallConverter.exe");
        using (var fs = File.Create(path))
            stream.CopyTo(fs);
    }

    private void ExtractDocs(string installDir)
    {
        var asm = Assembly.GetExecutingAssembly();
        string prefix = "ConverterInstaller.";
        foreach (string resName in DocResourceNames)
        {
            using var stream = asm.GetManifestResourceStream(resName);
            if (stream == null) continue;
            string fileName = resName.StartsWith(prefix) ? resName.Substring(prefix.Length) : resName;
            string path = Path.Combine(installDir, fileName);
            using (var fs = File.Create(path))
                stream.CopyTo(fs);
        }
    }

    private static void RegistrarDesinstalacion(string installDir, string uninstallExePath)
    {
        if (!File.Exists(uninstallExePath)) return;
        string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallGuid}";
        using var key = Registry.LocalMachine.CreateSubKey(keyPath, true);
        if (key == null) return;
        key.SetValue("DisplayName", "Converter");
        key.SetValue("DisplayVersion", "1.0");
        key.SetValue("Publisher", "Converter");
        key.SetValue("InstallLocation", installDir);
        key.SetValue("UninstallString", "\"" + uninstallExePath + "\"");
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    private static void CrearAccesoInicio(string installDir, string exePath, string uninstallExePath)
    {
        string startMenuDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            "Programs", AppFolderName);
        Directory.CreateDirectory(startMenuDir);
        CrearAccesoDirecto(Path.Combine(startMenuDir, "Converter.lnk"), exePath, "Converter - Convertir audio y video", installDir, null);
        CrearAccesoDirecto(Path.Combine(startMenuDir, "Desinstalar Converter.lnk"), uninstallExePath, "Desinstalar Converter", installDir, null);
        CrearAccesoDirecto(Path.Combine(startMenuDir, "Carpeta de Converter.lnk"), installDir, "Abrir carpeta de instalación", installDir, null);
    }

    private static void CrearAccesoDirecto(string shortcutPath, string targetPath, string description, string workingDir, string? _ = null)
    {
        try
        {
            string ps = $@"
$s = (New-Object -ComObject WScript.Shell).CreateShortcut('{shortcutPath.Replace("'", "''")}')
$s.TargetPath = '{targetPath.Replace("'", "''")}'
$s.WorkingDirectory = '{workingDir.Replace("'", "''")}'
$s.Description = '{description.Replace("'", "''")}'
$s.Save()
";
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + ps.Replace("\"", "\\\"") + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(startInfo)?.WaitForExit(5000);
        }
        catch { }
    }

    private async Task<bool> DescargarYExtraerFfmpeg(string installDir)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), "ffmpeg-converter-install.zip");
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Add("User-Agent", "ConverterInstaller/1.0");
            byte[] bytes = await client.GetByteArrayAsync(FfmpegZipUrl);
            await File.WriteAllBytesAsync(tempZip, bytes);

            string tempExtract = Path.Combine(Path.GetTempPath(), "ffmpeg-converter-extract");
            if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
            ZipFile.ExtractToDirectory(tempZip, tempExtract);

            // Buscar ffmpeg.exe dentro de la estructura (puede ser bin/ffmpeg.exe o raíz)
            string? ffmpegExe = BuscarArchivo(tempExtract, "ffmpeg.exe");
            if (ffmpegExe == null) return false;
            File.Copy(ffmpegExe, Path.Combine(installDir, "ffmpeg.exe"), true);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(tempZip)) File.Delete(tempZip);
                string tempExtract = Path.Combine(Path.GetTempPath(), "ffmpeg-converter-extract");
                if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
            }
            catch { }
        }
    }

    private static string? BuscarArchivo(string dir, string fileName)
    {
        string path = Path.Combine(dir, fileName);
        if (File.Exists(path)) return path;
        foreach (string sub in Directory.GetDirectories(dir))
        {
            var found = BuscarArchivo(sub, fileName);
            if (found != null) return found;
        }
        return null;
    }

    private static void RegistrarMenuContextual(string exePath)
    {
        string[] extensions = {
            ".mp4", ".avi", ".mkv", ".webm", ".mov", ".flv", ".wmv", ".m4v", ".mpeg", ".mpg",
            ".mp3", ".flac", ".wav", ".m4a", ".aac", ".ogg", ".wma", ".opus"
        };
        var formats = new (string id, string name)[] {
            ("mp3", "MP3"), ("aac", "AAC"), ("m4a", "M4A"), ("wav", "WAV"), ("flac", "FLAC"),
            ("ogg", "OGG"), ("opus", "Opus"), ("mp4", "MP4"), ("mkv", "MKV"), ("avi", "AVI"),
            ("webm", "WebM"), ("mov", "MOV"), ("wmv", "WMV"), ("flv", "FLV")
        };
        const string menuName = "Convertir a...";
        string baseKey = @"Software\Classes";

        foreach (string ext in extensions)
        {
            string keyPath = $"{baseKey}\\{ext}\\Shell\\ConverterMenu";
            using var key = Registry.LocalMachine.CreateSubKey(keyPath, true);
            if (key == null) continue;
            key.SetValue("MUIVerb", menuName);
            key.SetValue("SubCommands", "");

            foreach (var (id, name) in formats)
            {
                string cmdPath = $"{keyPath}\\shell\\{id}\\command";
                using var cmdKey = Registry.LocalMachine.CreateSubKey(cmdPath, true);
                if (cmdKey != null)
                    cmdKey.SetValue("", $"\"{exePath}\" \"%1\" --format {id}");
                string verbPath = $"{keyPath}\\shell\\{id}";
                using var verbKey = Registry.LocalMachine.CreateSubKey(verbPath, true);
                if (verbKey != null)
                    verbKey.SetValue("MUIVerb", name);
            }
        }
    }
}
