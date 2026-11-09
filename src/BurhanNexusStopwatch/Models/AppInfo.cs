using System.Reflection;

namespace BurhanNexusStopwatch.Models;

/// <summary>
/// Branding and contact details shown in the About dialog.
/// Official contact details are kept here in one place.
/// </summary>
public static class AppInfo
{
    public const string ProductName = "Burhan Nexus Stopwatch";
    public const string Developer = "Burhan Nexus";
    public const string Description =
        "A lightweight, elegant, and accurate digital stopwatch for Windows desktops, inspired by classic Casio digital watches.";

    // Official Burhan Nexus contact details
    public const string Mobile = "+923475093446";
    public const string Email = "ziaullah.046@gmail.com";

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
