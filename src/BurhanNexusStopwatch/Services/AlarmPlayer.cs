using System.IO;
using System.Media;

namespace BurhanNexusStopwatch.Services;

/// <summary>
/// Plays a looping digital-watch style alarm ("beep-beep-beep-beep ... pause").
/// The sound is generated in memory at startup (no audio file needed), and only one
/// alarm can ever play at a time.
/// </summary>
public sealed class AlarmPlayer : IDisposable
{
    private const int SampleRate = 44100;

    private readonly MemoryStream _wav;
    private readonly SoundPlayer _player;

    public bool IsPlaying { get; private set; }

    public AlarmPlayer()
    {
        _wav = new MemoryStream(BuildAlarmWav());
        _player = new SoundPlayer(_wav);
        _player.Load(); // initialise audio up front so the alarm starts instantly
    }

    public void Play()
    {
        if (IsPlaying) return;  // never stack alarms
        _player.PlayLooping();
        IsPlaying = true;
    }

    public void Stop()
    {
        if (!IsPlaying) return;
        _player.Stop();
        IsPlaying = false;
    }

    public void Dispose()
    {
        Stop();
        _player.Dispose();
        _wav.Dispose();
    }

    /// <summary>One loop: 4 short beeps (two tones) followed by a short silence. 16-bit mono PCM.</summary>
    private static byte[] BuildAlarmWav()
    {
        var samples = new List<short>();

        void Tone(double freq, double seconds, double volume)
        {
            int n = (int)(SampleRate * seconds);
            int fade = (int)(SampleRate * 0.006); // soft edges, no clicks
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SampleRate;
                double env = Math.Min(1.0, Math.Min(i, n - 1 - i) / (double)fade);
                // Fundamental + a little 3rd harmonic: bright, "digital watch" character.
                double v = Math.Sin(2 * Math.PI * freq * t) + 0.25 * Math.Sin(2 * Math.PI * freq * 3 * t);
                samples.Add((short)(v / 1.25 * env * volume * short.MaxValue));
            }
        }

        void Silence(double seconds)
        {
            int n = (int)(SampleRate * seconds);
            for (int i = 0; i < n; i++) samples.Add(0);
        }

        for (int i = 0; i < 4; i++)
        {
            Tone(i % 2 == 0 ? 2093 : 2349, 0.11, 0.85); // C7 / D7
            Silence(0.07);
        }
        Silence(0.45);

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        int dataBytes = samples.Count * 2;
        w.Write("RIFF"u8.ToArray());
        w.Write(36 + dataBytes);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray());
        w.Write(16);                 // PCM chunk size
        w.Write((short)1);           // PCM
        w.Write((short)1);           // mono
        w.Write(SampleRate);
        w.Write(SampleRate * 2);     // byte rate
        w.Write((short)2);           // block align
        w.Write((short)16);          // bits per sample
        w.Write("data"u8.ToArray());
        w.Write(dataBytes);
        foreach (var s in samples) w.Write(s);
        w.Flush();
        return ms.ToArray();
    }
}
