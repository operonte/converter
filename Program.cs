using System.Diagnostics;

// Ejemplo: Converter.exe "C:\ruta\video.mp4" --format mp3
// FFmpeg debe estar en la misma carpeta que el exe o en PATH

string? inputPath = null;
string? format = null;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--format" && i + 1 < args.Length)
    {
        format = args[i + 1].ToLowerInvariant().Trim();
        i++;
        continue;
    }
    if (!args[i].StartsWith("-"))
    {
        string path = args[i].Trim().Trim('"');
        if (File.Exists(path))
        {
            inputPath = Path.GetFullPath(path);
            break;
        }
    }
}

if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(format))
{
    ShowError("Uso: Converter.exe \"<archivo>\" --format <formato>");
    return 1;
}

var formats = GetSupportedFormats();
if (!formats.TryGetValue(format, out var formatInfo))
{
    ShowError($"Formato no soportado: {format}. Formatos: {string.Join(", ", formats.Keys)}");
    return 1;
}

string? ffmpegPath = FindFfmpeg();
if (string.IsNullOrEmpty(ffmpegPath))
{
    ShowError("No se encontró ffmpeg.exe. Colócalo en la carpeta del programa o instálalo en el sistema.");
    return 1;
}

string inputDir = Path.GetDirectoryName(inputPath) ?? "";
string inputName = Path.GetFileNameWithoutExtension(inputPath);
string outputExt = formatInfo.Extension;
string outputPath = Path.Combine(inputDir, $"{inputName}.{outputExt}");

// Evitar sobrescribir el mismo archivo
if (Path.GetFullPath(outputPath).Equals(Path.GetFullPath(inputPath), StringComparison.OrdinalIgnoreCase))
    outputPath = Path.Combine(inputDir, $"{inputName}_converted.{outputExt}");

int counter = 1;
while (File.Exists(outputPath))
{
    outputPath = Path.Combine(inputDir, $"{inputName}_converted{counter}.{outputExt}");
    counter++;
}

string ffmpegArgs = formatInfo.BuildArgs(inputPath, outputPath);
var startInfo = new ProcessStartInfo
{
    FileName = ffmpegPath,
    Arguments = ffmpegArgs,
    UseShellExecute = false,
    CreateNoWindow = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
};
using var process = Process.Start(startInfo);
if (process == null)
{
    ShowError("No se pudo iniciar ffmpeg.");
    return 1;
}

string stderr = await process.StandardError.ReadToEndAsync();
await process.WaitForExitAsync();

if (process.ExitCode != 0)
{
    ShowError($"Error en la conversión:\n{stderr}");
    return 1;
}

ShowSuccess($"Convertido: {Path.GetFileName(outputPath)}");
return 0;

// --- Helpers ---

string? FindFfmpeg()
{
    string? appDir = AppContext.BaseDirectory;
    if (!string.IsNullOrEmpty(appDir))
    {
        string local = Path.Combine(appDir, "ffmpeg.exe");
        if (File.Exists(local)) return local;
    }
    return FindInPath("ffmpeg.exe");
}

string? FindInPath(string exeName)
{
    string? pathEnv = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathEnv)) return null;
    foreach (string dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        string full = Path.Combine(dir.Trim(), exeName);
        if (File.Exists(full)) return full;
    }
    return null;
}

Dictionary<string, FormatInfo> GetSupportedFormats()
{
    return new Dictionary<string, FormatInfo>(StringComparer.OrdinalIgnoreCase)
    {
        // Audio (solo extracción/conversión de audio)
        ["mp3"] = new FormatInfo("mp3", true, "libmp3lame", "-q:a 2"),
        ["aac"] = new FormatInfo("aac", true, "aac", "-b:a 192k"),
        ["m4a"] = new FormatInfo("m4a", true, "aac", "-b:a 192k"),
        ["wav"] = new FormatInfo("wav", true, "pcm_s16le", ""),
        ["flac"] = new FormatInfo("flac", true, "flac", ""),
        ["ogg"] = new FormatInfo("ogg", true, "libvorbis", "-q:a 5"),
        ["wma"] = new FormatInfo("wma", true, "wmav2", ""),
        ["opus"] = new FormatInfo("opus", true, "libopus", "-b:a 128k"),
        // Video
        ["mp4"] = new FormatInfo("mp4", false, "-c:v libx264 -crf 23 -c:a aac -b:a 128k"),
        ["mkv"] = new FormatInfo("mkv", false, "-c:v libx264 -crf 23 -c:a aac -b:a 128k"),
        ["avi"] = new FormatInfo("avi", false, "-c:v mpeg4 -q:v 5 -c:a mp3 -b:a 192k"),
        ["webm"] = new FormatInfo("webm", false, "-c:v libvpx-vp9 -crf 30 -b:a 128k"),
        ["mov"] = new FormatInfo("mov", false, "-c:v libx264 -crf 23 -c:a aac -b:a 128k"),
        ["wmv"] = new FormatInfo("wmv", false, "-c:v wmv2 -c:a wmav2"),
        ["flv"] = new FormatInfo("flv", false, "-c:v flv -c:a aac -b:a 128k"),
    };
}

void ShowError(string message) => MessageBox(0, message, "Converter - Error", 0x10); // MB_ICONERROR
void ShowSuccess(string message) => MessageBox(0, message, "Converter", 0x40);       // MB_ICONINFO

[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
static extern int MessageBox(int hWnd, string text, string caption, int type);

record FormatInfo(string Extension, bool AudioOnly, string CodecOrArgs, string? AudioExtra = null)
{
    public string BuildArgs(string input, string output)
    {
        if (AudioOnly)
            return $"-y -i \"{input}\" -vn -c:a {CodecOrArgs} {AudioExtra} \"{output}\"".TrimEnd();
        return $"-y -i \"{input}\" {CodecOrArgs} \"{output}\"";
    }
}
