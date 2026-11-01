using System.IO;
using System.Text.Json;

namespace PhotoMap.Services;

public sealed class Settings
{
    public string? LastFolder { get; set; }
    public bool IncludeSubfolders { get; set; } = true;
    public string Theme { get; set; } = "academia";

    public const int DefaultMarkerSize = 46;

    public int MarkerSize { get; set; } = DefaultMarkerSize;

    // null = the colour of the current theme
    public string? MarkerColor { get; set; }
    public string? NoGpsColor { get; set; }

    public string MarkerColorOrDefault() => MarkerColor ?? Themes.Current.MarkerColor;
    public string NoGpsColorOrDefault() => NoGpsColor ?? Themes.Current.NoGpsColor;

    public void ResetMarkers()
    {
        MarkerSize = DefaultMarkerSize;
        MarkerColor = null;
        NoGpsColor = null;
    }

    static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fotomappa", "settings.json");

    public static Settings Load()
    {
        try
        {
            var s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new();
            // colours saved by earlier versions that equal a theme default follow the theme
            if (s.MarkerColor is { } m && Themes.IsDefaultMarker(m)) s.MarkerColor = null;
            if (s.NoGpsColor is { } n && Themes.IsDefaultNoGps(n)) s.NoGpsColor = null;
            return s;
        }
        catch { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch { }
    }
}
