using System.Reflection;

namespace ConverterInstaller;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new InstaladorForm());
    }
}
