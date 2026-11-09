using System.Diagnostics;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Services;

/// <summary>
/// Stopwatch logic built on <see cref="System.Diagnostics.Stopwatch"/> (monotonic,
/// high-resolution). The UI only reads <see cref="Elapsed"/>; it never counts ticks,
/// so display refresh rate has no effect on accuracy.
/// </summary>
public sealed class StopwatchEngine
{
    private readonly Stopwatch _stopwatch = new();

    public bool IsRunning => _stopwatch.IsRunning;
    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public bool IsZero => !_stopwatch.IsRunning && _stopwatch.ElapsedTicks == 0;

    /// <summary>Starts from zero, or resumes from the paused time.</summary>
    public void Start() => _stopwatch.Start();

    /// <summary>Pauses and keeps the elapsed time.</summary>
    public void Pause() => _stopwatch.Stop();

    /// <summary>Stops and clears to zero.</summary>
    public void Reset() => _stopwatch.Reset();

    public static string Format(TimeSpan t, DisplayPrecision precision)
    {
        long hours = (long)t.TotalHours;
        string main = $"{hours:00}:{t.Minutes:00}:{t.Seconds:00}";
        return precision switch
        {
            DisplayPrecision.Seconds => main,
            DisplayPrecision.Tenths => $"{main}.{t.Milliseconds / 100}",
            DisplayPrecision.Milliseconds => $"{main}.{t.Milliseconds:000}",
            _ => $"{main}.{t.Milliseconds / 10:00}",
        };
    }

    /// <summary>A sensible screen refresh interval for each precision (keeps CPU low).</summary>
    public static TimeSpan RefreshInterval(DisplayPrecision precision) => precision switch
    {
        DisplayPrecision.Seconds => TimeSpan.FromMilliseconds(100),
        DisplayPrecision.Tenths => TimeSpan.FromMilliseconds(50),
        _ => TimeSpan.FromMilliseconds(33), // ~30 fps
    };
}
