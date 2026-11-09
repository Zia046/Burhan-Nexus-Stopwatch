using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Services;

/// <summary>Loads and saves <see cref="AppSettings"/> to a local JSON file.</summary>
public static class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BurhanNexus", "Stopwatch");

    public static string SettingsFile { get; } = Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            // Corrupt or unreadable file: fall back to defaults.
        }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var tmp = SettingsFile + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(settings, JsonOptions));
            File.Move(tmp, SettingsFile, overwrite: true);
        }
        catch
        {
            // Settings are a convenience; never crash the app because of them.
        }
    }
}
