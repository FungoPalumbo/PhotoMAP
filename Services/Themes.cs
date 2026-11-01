using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace PhotoMap.Services;

public sealed class AppTheme
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    // brush resources, by key
    public required Dictionary<string, string> Colors { get; init; }

    public required string FontBody { get; init; }
    public required string FontHead { get; init; }
    public required string FontLabel { get; init; }
    public double BaseSize { get; init; } = 14;
    public double BtnSize { get; init; } = 14;
    public double LabelSize { get; init; } = 12;
    public double SizeBrand { get; init; } = 18;
    public double SizeH1 { get; init; } = 24;
    public double SizeH2 { get; init; } = 18;
    public double SizeH3 { get; init; } = 16;
    public bool UpperLabels { get; init; }

    // corner radii (px)
    public double RadBtn { get; init; } = 6;
    public double RadCard { get; init; } = 8;
    public double RadSmall { get; init; } = 4;
    public double RadBadge { get; init; } = 10;
    public double RadBar { get; init; } = 2;
    public double ClipR { get; init; } = 5;

    public string? GlowColor { get; init; }

    public required string MarkerColor { get; init; }
    public required string NoGpsColor { get; init; }
    public required string[] Swatches { get; init; }

    // css variables handed to the map page
    public required Dictionary<string, string> Map { get; init; }
}

public static class Themes
{
    const string StdFont = "DM Sans, Segoe UI Variable Text, Segoe UI";
    const string AcaBody = "Crimson Pro, Palatino Linotype, Georgia";
    const string AcaHead = "Cormorant Garamond, Garamond, Palatino Linotype, Georgia";
    const string AcaLabel = "Cinzel, Constantia, Palatino Linotype";
    const string RetroFont = "Cascadia Mono, Consolas, Courier New";

    public static readonly AppTheme Standard = new()
    {
        Id = "standard", Name = "Standard",
        Colors = new()
        {
            ["Ground"] = "#FBF7F8", ["Surface"] = "#FFFFFF", ["Raised"] = "#F3F4F6", ["Border"] = "#E5E7EB", ["Control"] = "#D1D5DB",
            ["Text"] = "#111827", ["Muted"] = "#5F6672",
            ["Accent"] = "#B8336A", ["AccentHover"] = "#A12C5D", ["AccentSoft"] = "#FDF2F7", ["AccentTrack"] = "#F8DBE8", ["AccentInk"] = "#6B1D3E",
            ["OnAccent"] = "#FFFFFF", ["AccentPressed"] = "#6B1D3E", ["FocusRing"] = "#6B1D3E",
            ["OutlineFg"] = "#111827", ["OutlineBorder"] = "#D1D5DB", ["OutlineBg"] = "#FFFFFF",
            ["RowBorder"] = "#00000000", ["RowBorderSel"] = "#00000000",
            ["BrandFill"] = "#B8336A", ["BrandStroke"] = "#00000000", ["BrandGlyph"] = "#FFFFFF",
            ["BadgeFg"] = "#374151", ["CheckBg"] = "#FFFFFF",
        },
        FontBody = StdFont, FontHead = StdFont, FontLabel = StdFont,
        MarkerColor = "#B8336A", NoGpsColor = "#2563EB",
        Swatches = new[] { "#B8336A", "#2563EB", "#16A34A", "#EA580C", "#0D9488", "#DC2626", "#D97706", "#111827" },
        Map = new()
        {
            ["--font"] = "\"DM Sans\", \"Segoe UI Variable Text\", \"Segoe UI\", sans-serif",
            ["--label-font"] = "\"DM Sans\", \"Segoe UI Variable Text\", \"Segoe UI\", sans-serif",
            ["--map-bg"] = "#eceeef", ["--text"] = "#111827", ["--muted"] = "#5f6672",
            ["--frame"] = "#ffffff", ["--on-accent"] = "#ffffff", ["--nogps-fg"] = "#ffffff",
            ["--cl-weight"] = "600", ["--nums"] = "tabular-nums",
            ["--pin-radius"] = "6px", ["--pin-bg"] = "#ffffff", ["--pin-filter"] = "none",
            ["--tile-filter"] = "none", ["--fx"] = "none",
            ["--panel-bg"] = "#ffffff", ["--panel-border"] = "transparent", ["--panel-radius"] = "6px", ["--panel-size"] = "14px",
            ["--zoom-bg"] = "#ffffff", ["--zoom-fg"] = "#111827", ["--zoom-hover"] = "#f4f4f4", ["--zoom-border"] = "#d1d5db", ["--zoom-radius"] = "4px",
            ["--attr-bg"] = "rgba(255,255,255,.8)", ["--attr-fg"] = "#333333", ["--attr-link"] = "#0078a8",
            ["--glow"] = "none", ["--glow-nogps"] = "none",
        },
    };

    public static readonly AppTheme Academia = new()
    {
        Id = "academia", Name = "Academia",
        Colors = new()
        {
            ["Ground"] = "#1C1714", ["Surface"] = "#251E19", ["Raised"] = "#3D332B", ["Border"] = "#4A3F35", ["Control"] = "#6B5B4B",
            ["Text"] = "#E8DFD4", ["Muted"] = "#9C8B7A",
            ["Accent"] = "#C9A962", ["AccentHover"] = "#D9BC7A", ["AccentSoft"] = "#3A2F24", ["AccentTrack"] = "#3D332B", ["AccentInk"] = "#1C1714",
            ["OnAccent"] = "#1C1714", ["AccentPressed"] = "#A98B48", ["FocusRing"] = "#E8DFD4",
            ["OutlineFg"] = "#C9A962", ["OutlineBorder"] = "#C9A962", ["OutlineBg"] = "#00000000",
            ["RowBorder"] = "#4A3F35", ["RowBorderSel"] = "#C9A962",
            ["BrandFill"] = "#8B2635", ["BrandStroke"] = "#C9A962", ["BrandGlyph"] = "#C9A962",
            ["BadgeFg"] = "#E8DFD4", ["CheckBg"] = "#1C1714",
        },
        FontBody = AcaBody, FontHead = AcaHead, FontLabel = AcaLabel,
        BaseSize = 16, BtnSize = 13, LabelSize = 11, SizeBrand = 26, SizeH1 = 28, SizeH2 = 24, SizeH3 = 22, UpperLabels = true,
        RadBtn = 3, RadCard = 4, RadSmall = 2, RadBadge = 3, RadBar = 0, ClipR = 2,
        MarkerColor = "#C9A962", NoGpsColor = "#3D332B",
        Swatches = new[] { "#C9A962", "#8B2635", "#A66A3F", "#6E7F5B", "#4F6D7A", "#7A5C8E", "#E8DFD4", "#3D332B" },
        Map = new()
        {
            ["--font"] = "\"Crimson Pro\", \"Palatino Linotype\", Georgia, serif",
            ["--label-font"] = "\"Cinzel\", Constantia, \"Palatino Linotype\", serif",
            ["--map-bg"] = "#1c1714", ["--text"] = "#e8dfd4", ["--muted"] = "#9c8b7a",
            ["--frame"] = "#e8dfd4", ["--on-accent"] = "#1c1714", ["--nogps-fg"] = "#e8dfd4",
            ["--cl-weight"] = "700", ["--nums"] = "lining-nums",
            ["--pin-radius"] = "50% 50% 3px 3px / 38% 38% 3px 3px", ["--pin-bg"] = "#251e19", ["--pin-filter"] = "sepia(.35)",
            ["--tile-filter"] = "sepia(.75) saturate(.8) brightness(.82) contrast(1.05)",
            ["--fx"] = "radial-gradient(ellipse at center, rgba(28,23,20,0) 55%, rgba(28,23,20,.55) 100%)",
            ["--panel-bg"] = "#251e19", ["--panel-border"] = "#c9a962", ["--panel-radius"] = "3px", ["--panel-size"] = "16px",
            ["--zoom-bg"] = "#251e19", ["--zoom-fg"] = "#c9a962", ["--zoom-hover"] = "#3d332b", ["--zoom-border"] = "#c9a962", ["--zoom-radius"] = "3px",
            ["--attr-bg"] = "rgba(28,23,20,.8)", ["--attr-fg"] = "#9c8b7a", ["--attr-link"] = "#c9a962",
            ["--glow"] = "none", ["--glow-nogps"] = "none",
        },
    };

    public static readonly AppTheme Retro = new()
    {
        Id = "retro", Name = "Retro-Futurism",
        Colors = new()
        {
            ["Ground"] = "#0A0A14", ["Surface"] = "#14142A", ["Raised"] = "#1F1F3D", ["Border"] = "#34346B", ["Control"] = "#5D34D0",
            ["Text"] = "#E4F9FF", ["Muted"] = "#9A9AD0",
            ["Accent"] = "#00FFFF", ["AccentHover"] = "#7FFFFF", ["AccentSoft"] = "#1B2F4E", ["AccentTrack"] = "#1F1F3D", ["AccentInk"] = "#0A0A14",
            ["OnAccent"] = "#0A0A14", ["AccentPressed"] = "#00B8CC", ["FocusRing"] = "#FF006E",
            ["OutlineFg"] = "#00FFFF", ["OutlineBorder"] = "#00FFFF", ["OutlineBg"] = "#00000000",
            ["RowBorder"] = "#1F1F3D", ["RowBorderSel"] = "#00FFFF",
            ["BrandFill"] = "#14142A", ["BrandStroke"] = "#FF006E", ["BrandGlyph"] = "#00FFFF",
            ["BadgeFg"] = "#E4F9FF", ["CheckBg"] = "#0A0A14",
        },
        FontBody = RetroFont, FontHead = RetroFont, FontLabel = RetroFont,
        BaseSize = 14, BtnSize = 13, LabelSize = 11, SizeBrand = 20, SizeH1 = 26, SizeH2 = 18, SizeH3 = 16, UpperLabels = true,
        RadBtn = 0, RadCard = 0, RadSmall = 0, RadBadge = 0, RadBar = 0, ClipR = 0,
        GlowColor = "#00FFFF",
        MarkerColor = "#00FFFF", NoGpsColor = "#FF006E",
        Swatches = new[] { "#00FFFF", "#FF006E", "#0080FF", "#5D34D0", "#FFD700", "#C0C0C0", "#1A1A2E", "#FFFFFF" },
        Map = new()
        {
            ["--font"] = "\"Cascadia Mono\", Consolas, \"Courier New\", monospace",
            ["--label-font"] = "\"Cascadia Mono\", Consolas, \"Courier New\", monospace",
            ["--map-bg"] = "#0a0a14", ["--text"] = "#e4f9ff", ["--muted"] = "#9a9ad0",
            ["--frame"] = "#e4f9ff", ["--on-accent"] = "#0a0a14", ["--nogps-fg"] = "#0a0a14",
            ["--cl-weight"] = "700", ["--nums"] = "tabular-nums",
            ["--pin-radius"] = "0px", ["--pin-bg"] = "#14142a", ["--pin-filter"] = "saturate(1.25) contrast(1.05)",
            ["--tile-filter"] = "invert(1) hue-rotate(195deg) saturate(1.3) brightness(.78) contrast(1.1)",
            ["--fx"] = "repeating-linear-gradient(0deg, rgba(0,0,0,.28) 0px, rgba(0,0,0,.28) 1px, transparent 1px, transparent 3px), radial-gradient(ellipse at center, rgba(10,10,20,0) 60%, rgba(10,10,20,.6) 100%)",
            ["--panel-bg"] = "#14142a", ["--panel-border"] = "#00ffff", ["--panel-radius"] = "0px", ["--panel-size"] = "14px",
            ["--zoom-bg"] = "#14142a", ["--zoom-fg"] = "#00ffff", ["--zoom-hover"] = "#1f1f3d", ["--zoom-border"] = "#00ffff", ["--zoom-radius"] = "0px",
            ["--attr-bg"] = "rgba(10,10,20,.85)", ["--attr-fg"] = "#9a9ad0", ["--attr-link"] = "#00ffff",
            ["--glow"] = "0 0 12px var(--accent)", ["--glow-nogps"] = "0 0 12px var(--nogps)",
        },
    };

    public static readonly AppTheme[] All = { Standard, Academia, Retro };

    public static AppTheme Current { get; private set; } = Academia;

    public static AppTheme Get(string? id) => All.FirstOrDefault(t => t.Id == id) ?? Academia;

    public static bool IsDefaultMarker(string hex) => All.Any(t => string.Equals(t.MarkerColor, hex, StringComparison.OrdinalIgnoreCase));
    public static bool IsDefaultNoGps(string hex) => All.Any(t => string.Equals(t.NoGpsColor, hex, StringComparison.OrdinalIgnoreCase));

    public static void Apply(string? id)
    {
        var t = Current = Get(id);
        var r = Application.Current.Resources;

        foreach (var (key, hex) in t.Colors)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            b.Freeze();
            r[key] = b;
        }

        r["FontBody"] = new FontFamily(t.FontBody);
        r["FontHead"] = new FontFamily(t.FontHead);
        r["FontLabel"] = new FontFamily(t.FontLabel);
        r["BaseSize"] = t.BaseSize;
        r["BtnSize"] = t.BtnSize;
        r["LabelSize"] = t.LabelSize;
        r["SizeBrand"] = t.SizeBrand;
        r["SizeH1"] = t.SizeH1;
        r["SizeH2"] = t.SizeH2;
        r["SizeH3"] = t.SizeH3;

        r["RadiusBtn"] = new CornerRadius(t.RadBtn);
        r["RadiusCard"] = new CornerRadius(t.RadCard);
        r["RadiusSmall"] = new CornerRadius(t.RadSmall);
        r["RadiusBadge"] = new CornerRadius(t.RadBadge);
        r["RadiusBar"] = new CornerRadius(t.RadBar);
        var top = Math.Max(0, t.RadCard - 1);
        r["RadiusTop"] = new CornerRadius(top, top, 0, 0);
        r["ClipR"] = t.ClipR;

        var glow = new DropShadowEffect
        {
            ShadowDepth = 0,
            BlurRadius = t.GlowColor is null ? 0 : 12,
            Opacity = t.GlowColor is null ? 0 : 0.6,
            Color = t.GlowColor is null ? Colors.Transparent : (Color)ColorConverter.ConvertFromString(t.GlowColor),
        };
        glow.Freeze();
        r["Glow"] = glow;
    }
}
