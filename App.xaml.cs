using System.IO;
using System.Windows;
using PhotoMap.Services;

namespace PhotoMap;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fotomappa");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "errori.log"), $"{DateTime.Now:s}\n{e.Exception}\n\n");
            }
            catch { }
            MessageBox.Show("Si è verificato un errore:\n\n" + e.Exception.Message, "Fotomappa", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Themes.Apply(Settings.Load().Theme);
        base.OnStartup(e);
    }
}
