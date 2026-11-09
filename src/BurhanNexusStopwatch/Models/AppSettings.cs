namespace BurhanNexusStopwatch.Models;

/// <summary>How many fractional digits the stopwatch shows.</summary>
public enum DisplayPrecision
{
    Seconds,
    Tenths,
    Hundredths,
    Milliseconds
}

public enum AppMode
{
    Stopwatch,
    Timer
}

public enum AppTheme
{
    ClassicLcd,
    Dark,
    Light
}

/// <summary>User preferences, stored as JSON in %AppData%\BurhanNexus\Stopwatch.</summary>
public sealed class AppSettings
{
    public bool AlwaysOnTop { get; set; }
    public bool Use24HourClock { get; set; }
    public bool ShowClock { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    public DisplayPrecision Precision { get; set; } = DisplayPrecision.Hundredths;
    public AppTheme Theme { get; set; } = AppTheme.ClassicLcd;
    public bool RememberPosition { get; set; } = true;
    public bool RememberSize { get; set; } = true;
    public bool StartWithWindows { get; set; }

    // Countdown timer
    public AppMode LastMode { get; set; } = AppMode.Stopwatch;
    public int TimerSeconds { get; set; } = 300; // last duration the user set

    // Last window bounds (device-independent units). Null = never saved.
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }

    public AppSettings Clone() => (AppSettings)MemberwiseClone();
}
