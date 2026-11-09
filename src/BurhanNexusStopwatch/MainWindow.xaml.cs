using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BurhanNexusStopwatch.Models;
using BurhanNexusStopwatch.Services;
using BurhanNexusStopwatch.Views;

namespace BurhanNexusStopwatch;

public partial class MainWindow : Window
{
    private AppSettings _settings;
    private readonly StopwatchEngine _engine = new();
    private readonly DispatcherTimer _displayTimer;
    private readonly DispatcherTimer _clockTimer;

    public MainWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();

        // Display refresh only runs while the stopwatch runs. It just *reads* the
        // high-resolution stopwatch, so a late tick never causes drift.
        _displayTimer = new DispatcherTimer(DispatcherPriority.Render);
        _displayTimer.Tick += (_, _) => UpdateStopwatchDisplay();

        // Clock ticks once per second, aligned to the start of each second.
        _clockTimer = new DispatcherTimer(DispatcherPriority.Background);
        _clockTimer.Tick += (_, _) => UpdateClock();

        RestoreWindowBounds();

        SourceInitialized += (_, _) => NativeMethods.SetTopMost(this, _settings.AlwaysOnTop);
        StateChanged += OnWindowStateChanged;
        Deactivated += (_, _) =>
        {
            // Keep the window in the top-most band while another app is active.
            if (_settings.AlwaysOnTop) NativeMethods.SetTopMost(this, true);
        };
        Closing += OnClosing;

        ApplySettings();
        UpdateStopwatchDisplay();
        UpdateControls();
        InitializeCountdown();
    }

    // ---------------------------------------------------------------- Stopwatch

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_engine.IsRunning) return;
        _engine.Start();
        _displayTimer.Start();
        UpdateControls();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_engine.IsRunning) return;
        _engine.Pause();
        _displayTimer.Stop();
        UpdateStopwatchDisplay(); // show the exact paused time
        UpdateControls();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _engine.Reset();
        _displayTimer.Stop();
        UpdateStopwatchDisplay();
        UpdateControls();
    }

    private void UpdateStopwatchDisplay()
    {
        var text = StopwatchEngine.Format(_engine.Elapsed, _settings.Precision);
        if (MainDisplay.Text != text) MainDisplay.Text = text;
    }

    private void UpdateControls()
    {
        bool running = _engine.IsRunning;
        bool zero = _engine.IsZero;

        StartButton.IsEnabled = !running;
        PauseButton.IsEnabled = running;
        ResetButton.IsEnabled = !zero;

        if (CurrentMode == AppMode.Stopwatch)
            StatusText.Text = running ? "RUN" : zero ? "READY" : "PAUSE";
    }

    // ---------------------------------------------------------------- Clock

    private void UpdateClock()
    {
        var now = DateTime.Now;

        if (_settings.ShowClock)
        {
            if (_settings.Use24HourClock)
            {
                ClockDisplay.Text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                int h = now.Hour % 12;
                if (h == 0) h = 12;
                ClockDisplay.Text = $"{h,2}:{now:mm}:{now:ss}";
                AmPmText.Text = now.Hour < 12 ? "AM" : "PM";
            }
        }

        if (_settings.ShowDate)
        {
            var culture = CultureInfo.CurrentCulture;
            DateText.Text = now.ToString("ddd  dd MMM yyyy", culture).ToUpper(culture);
        }

        // Schedule the next tick just after the next whole second.
        _clockTimer.Interval = TimeSpan.FromMilliseconds(1000 - now.Millisecond + 15);
        if (!_clockTimer.IsEnabled && (_settings.ShowClock || _settings.ShowDate)) _clockTimer.Start();
    }

    // ---------------------------------------------------------------- Settings

    private void ApplySettings()
    {
        ThemeManager.Apply(_settings.Theme);

        if (IsLoaded || new System.Windows.Interop.WindowInteropHelper(this).Handle != IntPtr.Zero)
            NativeMethods.SetTopMost(this, _settings.AlwaysOnTop);
        else
            Topmost = _settings.AlwaysOnTop;
        PinIndicator.Visibility = _settings.AlwaysOnTop ? Visibility.Visible : Visibility.Collapsed;

        ClockPanel.Visibility = _settings.ShowClock ? Visibility.Visible : Visibility.Collapsed;
        AmPmText.Visibility = _settings.Use24HourClock ? Visibility.Collapsed : Visibility.Visible;
        DateText.Visibility = _settings.ShowDate ? Visibility.Visible : Visibility.Collapsed;
        InfoRow.Visibility = _settings.ShowClock || _settings.ShowDate ? Visibility.Visible : Visibility.Collapsed;

        _displayTimer.Interval = StopwatchEngine.RefreshInterval(_settings.Precision);

        if (_settings.ShowClock || _settings.ShowDate) UpdateClock();
        else _clockTimer.Stop();

        UpdateStopwatchDisplay();
        if (_uiReady) ApplyMode(); // keeps the TIME'S UP layout if the alarm is ringing
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(_settings.Clone()) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        var updated = dialog.Result;
        if (updated.StartWithWindows != _settings.StartWithWindows &&
            !StartupService.Apply(updated.StartWithWindows))
        {
            MessageBox.Show(this, "Could not change the Windows startup setting.",
                AppInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
            updated.StartWithWindows = _settings.StartWithWindows;
        }

        _settings = updated;
        ApplySettings();
        SaveSettings();
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        new AboutWindow { Owner = this }.ShowDialog();
    }

    // ---------------------------------------------------------------- Window

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed && WindowState == WindowState.Normal)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            double inset = NativeMethods.MaximizedOverhang(VisualTreeHelper.GetDpi(this).DpiScaleX);
            RootBorder.Margin = new Thickness(inset);
            RootBorder.BorderThickness = new Thickness(0);
            MaximizeButton.Content = "";
            MaximizeButton.ToolTip = "Restore";
        }
        else
        {
            RootBorder.Margin = new Thickness(0);
            RootBorder.BorderThickness = new Thickness(1);
            MaximizeButton.Content = "";
            MaximizeButton.ToolTip = "Maximize";
        }

        if (WindowState != WindowState.Minimized && _settings.AlwaysOnTop)
            NativeMethods.SetTopMost(this, true);
    }

    private void RestoreWindowBounds()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (_settings.RememberSize &&
            _settings.WindowWidth is double w && _settings.WindowHeight is double h &&
            w >= MinWidth && h >= MinHeight)
        {
            Width = Math.Min(w, SystemParameters.VirtualScreenWidth);
            Height = Math.Min(h, SystemParameters.VirtualScreenHeight);
        }

        if (_settings.RememberPosition && _settings.WindowLeft is double left && _settings.WindowTop is double top)
        {
            // Only restore if the title bar would still be on a visible screen.
            var screen = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            var titleBar = new Rect(left + 20, top, Math.Max(40, Width - 40), 24);
            if (screen.IntersectsWith(titleBar))
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = left;
                Top = top;
            }
        }
    }

    private void CaptureWindowBounds()
    {
        var bounds = WindowState == WindowState.Normal
            ? new Rect(Left, Top, ActualWidth, ActualHeight)
            : RestoreBounds;

        if (bounds.IsEmpty || double.IsNaN(bounds.Left) || double.IsInfinity(bounds.Width)) return;

        _settings.WindowLeft = _settings.RememberPosition ? bounds.Left : null;
        _settings.WindowTop = _settings.RememberPosition ? bounds.Top : null;
        _settings.WindowWidth = _settings.RememberSize ? bounds.Width : null;
        _settings.WindowHeight = _settings.RememberSize ? bounds.Height : null;
    }

    private void SaveSettings()
    {
        CaptureWindowBounds();
        SettingsService.Save(_settings);
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _displayTimer.Stop();
        _clockTimer.Stop();
        ShutdownCountdown();
        SaveSettings();
    }
}
