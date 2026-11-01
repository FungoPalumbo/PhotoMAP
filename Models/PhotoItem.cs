using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace PhotoMap.Models;

public sealed class PhotoItem : INotifyPropertyChanged
{
    bool _isSelected;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    static readonly CultureInfo It = CultureInfo.GetCultureInfo("it-IT");

    public int Id { get; set; }
    public required string Path { get; init; }
    public required string FileName { get; init; }
    public required string Folder { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public DateTime? Taken { get; init; }
    public string? Camera { get; init; }
    public string? Exposure { get; init; }
    public int Orientation { get; init; } = 1;
    public BitmapSource? Thumb { get; init; }
    public string? ThumbUri { get; init; }

    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;
    public bool NoLocation => !HasLocation;

    public string Subtitle => Taken is { } t
        ? $"{t.ToString("d MMM yyyy", It)} · {Folder}"
        : Folder;

    public string DateText => Taken is { } t
        ? t.ToString("dddd d MMMM yyyy, HH:mm", It)
        : "Data di scatto non registrata";

    public string CoordText => HasLocation
        ? FormattableString.Invariant($"{Math.Abs(Latitude!.Value):0.00000}° {(Latitude >= 0 ? "N" : "S")}, {Math.Abs(Longitude!.Value):0.00000}° {(Longitude >= 0 ? "E" : "O")}")
        : "Nessun dato GPS nel file";

    public string CameraText => string.IsNullOrWhiteSpace(Camera) ? "—" : Camera!;
    public string ExposureText => string.IsNullOrWhiteSpace(Exposure) ? "—" : Exposure!;
}
