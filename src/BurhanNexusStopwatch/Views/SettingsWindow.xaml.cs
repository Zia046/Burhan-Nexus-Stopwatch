using System.Windows;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Views;

public partial class SettingsWindow : Window
{
    public AppSettings Result { get; }

    public SettingsWindow(AppSettings settings)
    {
        Result = settings;
        InitializeComponent();

        // Never taller than the screen; the content scrolls if needed.
        MaxHeight = SystemParameters.WorkArea.Height - 40;

        PinCheck.IsChecked = settings.AlwaysOnTop;

        Clock24Radio.IsChecked = settings.Use24HourClock;
        Clock12Radio.IsChecked = !settings.Use24HourClock;
        ShowClockCheck.IsChecked = settings.ShowClock;
        ShowDateCheck.IsChecked = settings.ShowDate;

        PrecSecondsRadio.IsChecked = settings.Precision == DisplayPrecision.Seconds;
        PrecTenthsRadio.IsChecked = settings.Precision == DisplayPrecision.Tenths;
        PrecHundredthsRadio.IsChecked = settings.Precision == DisplayPrecision.Hundredths;
        PrecMillisRadio.IsChecked = settings.Precision == DisplayPrecision.Milliseconds;

        ThemeClassicRadio.IsChecked = settings.Theme == AppTheme.ClassicLcd;
        ThemeDarkRadio.IsChecked = settings.Theme == AppTheme.Dark;
        ThemeLightRadio.IsChecked = settings.Theme == AppTheme.Light;

        RememberPositionCheck.IsChecked = settings.RememberPosition;
        RememberSizeCheck.IsChecked = settings.RememberSize;
        StartWithWindowsCheck.IsChecked = settings.StartWithWindows;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Result.AlwaysOnTop = PinCheck.IsChecked == true;

        Result.Use24HourClock = Clock24Radio.IsChecked == true;
        Result.ShowClock = ShowClockCheck.IsChecked == true;
        Result.ShowDate = ShowDateCheck.IsChecked == true;

        Result.Precision =
            PrecSecondsRadio.IsChecked == true ? DisplayPrecision.Seconds :
            PrecTenthsRadio.IsChecked == true ? DisplayPrecision.Tenths :
            PrecMillisRadio.IsChecked == true ? DisplayPrecision.Milliseconds :
            DisplayPrecision.Hundredths;

        Result.Theme =
            ThemeDarkRadio.IsChecked == true ? AppTheme.Dark :
            ThemeLightRadio.IsChecked == true ? AppTheme.Light :
            AppTheme.ClassicLcd;

        Result.RememberPosition = RememberPositionCheck.IsChecked == true;
        Result.RememberSize = RememberSizeCheck.IsChecked == true;
        Result.StartWithWindows = StartWithWindowsCheck.IsChecked == true;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
