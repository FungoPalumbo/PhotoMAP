using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhotoMap.Services;

namespace PhotoMap;

public partial class SettingsWindow : Window
{
    static readonly Regex Hex = new("^#[0-9a-fA-F]{6}$");

    readonly Settings _s;
    readonly Action _apply;
    bool _loading = true;

    public SettingsWindow(Settings settings, Action apply)
    {
        InitializeComponent();
        _s = settings;
        _apply = apply;
        BuildSwatches(MarkerSwatches, c => _s.MarkerColor = c);
        BuildSwatches(NoGpsSwatches, c => _s.NoGpsColor = c);
        Sync();
        Closing += (_, _) => _s.Save();
    }

    void BuildSwatches(WrapPanel host, Action<string> set)
    {
        foreach (var color in Themes.Current.Swatches)
        {
            var b = new Button
            {
                Style = (Style)FindResource("Swatch"),
                Background = Brush(color),
                Tag = color,
                ToolTip = color,
            };
            System.Windows.Automation.AutomationProperties.SetName(b, "Colore " + color);
            b.Click += (_, _) => { set(color); Sync(); _apply(); };
            host.Children.Add(b);
        }
    }

    static SolidColorBrush Brush(string hex) => (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;

    void Sync()
    {
        _loading = true;
        SizeSlider.Value = _s.MarkerSize;
        SizeText.Text = _s.MarkerSize + " px";
        MarkerHex.Text = _s.MarkerColorOrDefault();
        NoGpsHex.Text = _s.NoGpsColorOrDefault();
        Ring(MarkerSwatches, _s.MarkerColorOrDefault());
        Ring(NoGpsSwatches, _s.NoGpsColorOrDefault());
        _loading = false;
    }

    void Ring(WrapPanel host, string current)
    {
        foreach (Button b in host.Children)
            b.BorderBrush = string.Equals((string)b.Tag, current, StringComparison.OrdinalIgnoreCase)
                ? (Brush)FindResource("Text") : Brushes.Transparent;
    }

    void Size_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loading || !IsLoaded) return;
        _s.MarkerSize = (int)SizeSlider.Value;
        SizeText.Text = _s.MarkerSize + " px";
        _apply();
    }

    void MarkerHex_Changed(object sender, TextChangedEventArgs e)
    {
        if (_loading || !Hex.IsMatch(MarkerHex.Text)) return;
        _s.MarkerColor = MarkerHex.Text.ToUpperInvariant();
        Ring(MarkerSwatches, _s.MarkerColorOrDefault());
        _apply();
    }

    void NoGpsHex_Changed(object sender, TextChangedEventArgs e)
    {
        if (_loading || !Hex.IsMatch(NoGpsHex.Text)) return;
        _s.NoGpsColor = NoGpsHex.Text.ToUpperInvariant();
        Ring(NoGpsSwatches, _s.NoGpsColorOrDefault());
        _apply();
    }

    void Reset_Click(object sender, RoutedEventArgs e)
    {
        _s.ResetMarkers();
        Sync();
        _apply();
    }

    void Close_Click(object sender, RoutedEventArgs e) => Close();
}
