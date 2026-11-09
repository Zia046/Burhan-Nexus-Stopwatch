using System.Diagnostics;

namespace BurhanNexusStopwatch.Services;

public enum CountdownState
{
    Idle,      // set, not started (shows the full duration)
    Running,
    Paused,
    Finished   // reached zero, alarm ringing
}

/// <summary>
/// Countdown timer based on a monotonic high-resolution <see cref="Stopwatch"/>.
/// Remaining time is always computed as (duration - elapsed), never by counting UI ticks,
/// so a slow or late screen refresh cannot make the timer drift.
/// </summary>
public sealed class CountdownEngine
{
    public static readonly TimeSpan MaxDuration = new(99, 59, 59);

    private readonly Stopwatch _stopwatch = new();

    public TimeSpan Duration { get; private set; } = TimeSpan.FromMinutes(5);
    public CountdownState State { get; private set; } = CountdownState.Idle;

    public TimeSpan Remaining
    {
        get
        {
            var r = Duration - _stopwatch.Elapsed;
            return r > TimeSpan.Zero ? r : TimeSpan.Zero;
        }
    }

    /// <summary>Sets a new duration and returns to Idle.</summary>
    public void SetDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) duration = TimeSpan.FromSeconds(1);
        if (duration > MaxDuration) duration = MaxDuration;
        Duration = duration;
        Reset();
    }

    public void Start()
    {
        if (State is CountdownState.Running or CountdownState.Finished) return;
        _stopwatch.Start();
        State = CountdownState.Running;
    }

    public void Pause()
    {
        if (State != CountdownState.Running) return;
        _stopwatch.Stop();
        State = CountdownState.Paused;
    }

    /// <summary>Stops and restores the full duration.</summary>
    public void Reset()
    {
        _stopwatch.Reset();
        State = CountdownState.Idle;
    }

    /// <summary>Call when the remaining time hits zero. Returns true only the first time.</summary>
    public bool TryFinish()
    {
        if (State != CountdownState.Running || Remaining > TimeSpan.Zero) return false;
        _stopwatch.Stop();
        State = CountdownState.Finished;
        return true;
    }

    /// <summary>HH:MM:SS, rounded up so "00:00:00" appears exactly when the time is up.</summary>
    public static string Format(TimeSpan remaining)
    {
        long totalSeconds = (long)Math.Ceiling(remaining.TotalSeconds - 0.0005);
        if (totalSeconds < 0) totalSeconds = 0;
        long h = totalSeconds / 3600, m = totalSeconds / 60 % 60, s = totalSeconds % 60;
        return $"{h:00}:{m:00}:{s:00}";
    }
}
