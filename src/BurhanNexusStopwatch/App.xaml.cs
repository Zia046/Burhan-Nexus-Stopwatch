using System.Windows;
using System.Windows.Threading;
using BurhanNexusStopwatch.Services;

namespace BurhanNexusStopwatch;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;

        var settings = SettingsService.Load();
        ThemeManager.Apply(settings.Theme);

        // Keep the startup entry pointing at the current exe location (in case it was moved).
        if (settings.StartWithWindows)
            StartupService.Apply(true);

        var window = new MainWindow(settings);
        MainWindow = window;
        window.Show();
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("An unexpected error occurred:\n\n" + e.Exception.Message,
            "Burhan Nexus Stopwatch", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }
}
