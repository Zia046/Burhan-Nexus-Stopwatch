using System.Windows;
using BurhanNexusStopwatch.Models;

namespace BurhanNexusStopwatch.Services;

/// <summary>Swaps the colour dictionary at runtime. All styles use DynamicResource.</summary>
public static class ThemeManager
{
    private static ResourceDictionary? _current;
    private static AppTheme? _currentTheme;

    public static void Apply(AppTheme theme)
    {
        if (_currentTheme == theme && _current is not null) return;

        var name = theme switch
        {
            AppTheme.Dark => "Dark",
            AppTheme.Light => "Light",
            _ => "ClassicLcd",
        };

        var dict = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/BurhanNexusStopwatch;component/Themes/{name}.xaml", UriKind.Absolute)
        };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_current is not null) merged.Remove(_current);
        merged.Insert(0, dict); // Styles.xaml only references colours via DynamicResource
        _current = dict;
        _currentTheme = theme;
    }
}
