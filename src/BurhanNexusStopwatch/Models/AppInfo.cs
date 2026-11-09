using System.Reflection;

namespace BurhanNexusStopwatch.Models;

/// <summary>
/// Branding and contact details shown in the About dialog.
/// Edit the placeholder values below with your real details, then rebuild.
/// </summary>
public static class AppInfo
{
    public const string ProductName = "Burhan Nexus Stopwatch";
    public const string Developer = "Burhan Nexus";
    public const string Description =
        "A lightweight, elegant, and easy-to-use desktop stopwatch application designed for everyday timing needs.";

    // ---- Edit these ----
    public const string Mobile = "[INSERT YOUR MOBILE NUMBER]";
    public const string Email = "[INSERT YOUR EMAIL ADDRESS]";
    // --------------------

    public const string Copyright = "Copyright © 2026 Burhan Nexus. All rights reserved.";

    public static string Version
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is null ? "1.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }
}
