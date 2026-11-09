using System.Windows;
using System.Windows.Threading;
using BurhanNexusStopwatch.Models;
using BurhanNexusStopwatch.Services;
using BurhanNexusStopwatch.Views;

namespace BurhanNexusStopwatch;

/// <summary>Countdown timer + alarm. Independent of the stopwatch; both can run at the same time.</summary>
public partial class MainWindow
{
    private readonly CountdownEngine _countdown = new();
    private AlarmPlayer? _alarm;
    private DispatcherTimer _timerDisplayTimer = null!;
    private DispatcherTimer _timerFinishTimer = null!;
    private DispatcherTimer _blinkTimer = null!;
    private bool _blinkOn;
    private bool _uiReady;

    private AppMode CurrentMode => TimerTab.IsChecked == true ? AppMode.Timer : AppMode.Stopwatch;

    private void InitializeCountdown()
    {
        // Screen refresh only (reads the engine; never used for counting).
        _timerDisplayTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(100) };
        _timerDisplayTimer.Tick += (_, _) => OnTimerDisplayTick();

        // One-shot timer aimed at the exact finish moment, so the alarm fires on time
        // even when the window is minimised and nothing is being drawn.
        _timerFinishTimer = new DispatcherTimer(DispatcherPriority.Send);
        _timerFinishTimer.Tick += (_, _) => CheckCountdownFinished();

        _blinkTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(500) };
        _blinkTimer.Tick += (_, _) => Blink();

        _countdown.SetDuration(TimeSpan.FromSeconds(Math.Max(1, _settings.TimerSeconds)));

        try { _alarm = new AlarmPlayer(); }
        catch { _alarm = null; } // no audio device: the visual alert still works

        _uiReady = true;
        if (_settings.LastMode == AppMode.Timer) TimerTab.IsChecked = true;
        ApplyMode();
        UpdateTimerDisplay();
        UpdateTimerControls();
    }

    // ---------------------------------------------------------------- Mode switching

    private void ModeTab_Checked(object sender, RoutedEventArgs e)
    {
        if (!_uiReady) return;
        ApplyMode();
    }

    private void ApplyMode()
    {
        bool timer = CurrentMode == AppMode.Timer;
        bool ringing = _countdown.State == CountdownState.Finished;

        MainDisplay.Visibility = timer ? Visibility.Collapsed : Visibility.Visible;
        TimerDisplay.Visibility = timer ? Visibility.Visible : Visibility.Collapsed;
        ModeText.Text = timer ? "TIMER" : "STOPWATCH";

        StopwatchButtons.Visibility = !timer ? Visibility.Visible : Visibility.Collapsed;
        TimerButtons.Visibility = timer && !ringing ? Visibility.Visible : Visibility.Collapsed;
        AlarmButtons.Visibility = timer && ringing ? Visibility.Visible : Visibility.Collapsed;

        TimesUpText.Visibility = timer && ringing ? Visibility.Visible : Visibility.Collapsed;
        InfoRow.Visibility = (timer && ringing) || !(_settings.ShowClock || _settings.ShowDate)
            ? Visibility.Collapsed : Visibility.Visible;

        _settings.LastMode = CurrentMode;
        UpdateControls();
        UpdateTimerControls();
    }

    // ---------------------------------------------------------------- Buttons

    private void SetTimerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SetTimerWindow(_countdown.Duration) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        StopAlarm();
        _countdown.SetDuration(dialog.SelectedDuration);
        _settings.TimerSeconds = (int)_countdown.Duration.TotalSeconds;
        SettingsService.Save(_settings);

        if (dialog.StartImmediately) StartCountdown();
        else StopCountdownTimers();

        UpdateTimerDisplay();
        UpdateTimerControls();
    }

    private void TimerStartButton_Click(object sender, RoutedEventArgs e)
    {
        switch (_countdown.State)
        {
            case CountdownState.Running:
                _countdown.Pause();
                StopCountdownTimers();
                break;
            case CountdownState.Idle:
            case CountdownState.Paused:
                StartCountdown();
                break;
        }
        UpdateTimerDisplay();
        UpdateTimerControls();
    }

    private void TimerResetButton_Click(object sender, RoutedEventArgs e)
    {
        StopAlarm();
        _countdown.Reset();
        StopCountdownTimers();
        UpdateTimerDisplay();
        UpdateTimerControls();
    }

    private void StopAlarmButton_Click(object sender, RoutedEventArgs e)
    {
        StopAlarm();
        _countdown.Reset(); // back to the set duration, ready to start again
        UpdateTimerDisplay();
        UpdateTimerControls();
        ApplyMode();
    }

    // ---------------------------------------------------------------- Engine plumbing

    private void StartCountdown()
    {
        StopAlarm();
        _countdown.Start();
        _timerDisplayTimer.Start();
        ScheduleFinishCheck();
    }

    private void StopCountdownTimers()
    {
        _timerDisplayTimer.Stop();
        _timerFinishTimer.Stop();
    }

    private void ScheduleFinishCheck()
    {
        _timerFinishTimer.Stop();
        var remaining = _countdown.Remaining;
        _timerFinishTimer.Interval = remaining > TimeSpan.FromMilliseconds(1) ? remaining : TimeSpan.FromMilliseconds(1);
        _timerFinishTimer.Start();
    }

    private void OnTimerDisplayTick()
    {
        UpdateTimerDisplay();
        if (_countdown.State == CountdownState.Running && _countdown.Remaining <= TimeSpan.Zero)
            CheckCountdownFinished();
    }

    private void CheckCountdownFinished()
    {
        if (_countdown.State != CountdownState.Running) { _timerFinishTimer.Stop(); return; }

        if (_countdown.Remaining > TimeSpan.Zero)
        {
            ScheduleFinishCheck(); // woke slightly early: aim again
            return;
        }

        if (_countdown.TryFinish()) OnTimesUp();
    }

    private void OnTimesUp()
    {
        StopCountdownTimers();

        _alarm?.Play();
        if (_alarm is null) System.Media.SystemSounds.Exclamation.Play();

        // Bring the alert in front of the user.
        TimerTab.IsChecked = true;
        ApplyMode();
        UpdateTimerDisplay();
        UpdateTimerControls();

        if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        Activate();
        if (!_settings.AlwaysOnTop)
        {
            Topmost = true;   // pop above other windows once...
            Topmost = false;  // ...without pinning it permanently
        }
        NativeMethods.FlashTaskbar(this, true);

        _blinkOn = true;
        _blinkTimer.Start();
    }

    private void StopAlarm()
    {
        _alarm?.Stop();
        _blinkTimer?.Stop();
        if (IsInitializedForBlink)
        {
            TimerDisplay.Opacity = 1;
            LcdBorder.SetResourceReference(System.Windows.Controls.Border.BorderBrushProperty, "LcdBorderBrush");
            NativeMethods.FlashTaskbar(this, false);
        }
    }

    private bool IsInitializedForBlink => TimerDisplay is not null && LcdBorder is not null;

    private void Blink()
    {
        _blinkOn = !_blinkOn;
        TimerDisplay.Opacity = _blinkOn ? 1 : 0.15;
        if (_blinkOn)
            LcdBorder.SetResourceReference(System.Windows.Controls.Border.BorderBrushProperty, "DangerBrush");
        else
            LcdBorder.SetResourceReference(System.Windows.Controls.Border.BorderBrushProperty, "LcdBorderBrush");
    }

    // ---------------------------------------------------------------- Display

    private void UpdateTimerDisplay()
    {
        var text = CountdownEngine.Format(_countdown.Remaining);
        if (TimerDisplay.Text != text) TimerDisplay.Text = text;
        UpdateOtherModeText();
    }

    private void UpdateTimerControls()
    {
        var state = _countdown.State;

        TimerStartButton.Content = state switch
        {
            CountdownState.Running => "PAUSE",
            CountdownState.Paused => "RESUME",
            _ => "START",
        };
        TimerStartButton.ToolTip = state switch
        {
            CountdownState.Running => "Pause timer",
            CountdownState.Paused => "Resume timer",
            _ => "Start timer",
        };
        TimerStartButton.IsEnabled = state != CountdownState.Finished;
        SetTimerButton.IsEnabled = state != CountdownState.Running;
        TimerResetButton.IsEnabled = state is CountdownState.Running or CountdownState.Paused;

        if (CurrentMode == AppMode.Timer)
        {
            StatusText.Text = state switch
            {
                CountdownState.Running => "RUN",
                CountdownState.Paused => "PAUSE",
                CountdownState.Finished => "ALARM",
                _ => "SET " + FormatShort(_countdown.Duration),
            };
        }
        UpdateOtherModeText();
    }

    /// <summary>In one mode, briefly shows what the other one is doing (if active).</summary>
    private void UpdateOtherModeText()
    {
        if (!_uiReady) return;
        string text = string.Empty;
        if (CurrentMode == AppMode.Stopwatch)
        {
            if (_countdown.State is CountdownState.Running or CountdownState.Paused)
                text = "TMR " + CountdownEngine.Format(_countdown.Remaining);
        }
        else if (_engine.IsRunning)
        {
            text = "SW RUN";
        }
        else if (!_engine.IsZero)
        {
            text = "SW PAUSE";
        }
        if (OtherModeText.Text != text) OtherModeText.Text = text;
    }

    private static string FormatShort(TimeSpan d)
    {
        if (d.TotalHours >= 1) return $"{(int)d.TotalHours}:{d.Minutes:00}:{d.Seconds:00}";
        return $"{d.Minutes:00}:{d.Seconds:00}";
    }

    private void ShutdownCountdown()
    {
        StopCountdownTimers();
        _blinkTimer.Stop();
        _alarm?.Dispose();
        _alarm = null;
    }
}
