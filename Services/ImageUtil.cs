using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMap.Services;

public static class ImageUtil
{
    /// <summary>Decodes an image (optionally downscaled) and applies its EXIF orientation.</summary>
    public static BitmapSource Load(string path, int decodeWidth, int orientation)
    {
        using var fs = File.OpenRead(path);
        var bi = new BitmapImage();
        bi.BeginInit();
        bi.CacheOption = BitmapCacheOption.OnLoad;
        if (decodeWidth > 0) bi.DecodePixelWidth = decodeWidth;
        bi.StreamSource = fs;
        bi.EndInit();
        bi.Freeze();
        return Orient(bi, orientation);
    }

    public static BitmapSource Orient(BitmapSource s, int o)
    {
        BitmapSource r = o switch
        {
            2 => Flip(s),
            3 => Rot(s, 180),
            4 => Flip(Rot(s, 180)),
            5 => Flip(Rot(s, 90)),
            6 => Rot(s, 90),
            7 => Flip(Rot(s, 270)),
            8 => Rot(s, 270),
            _ => s
        };
        if (!r.IsFrozen) r.Freeze();
        return r;
    }

    static BitmapSource Rot(BitmapSource s, double deg) => new TransformedBitmap(s, new RotateTransform(deg));
    static BitmapSource Flip(BitmapSource s) => new TransformedBitmap(s, new ScaleTransform(-1, 1));

    public static BitmapSource CropSquare(BitmapSource s)
    {
        int side = Math.Min(s.PixelWidth, s.PixelHeight);
        var c = new CroppedBitmap(s, new Int32Rect((s.PixelWidth - side) / 2, (s.PixelHeight - side) / 2, side, side));
        c.Freeze();
        return c;
    }

    public static string ToJpegDataUri(BitmapSource s, int quality = 80)
    {
        var enc = new JpegBitmapEncoder { QualityLevel = quality };
        enc.Frames.Add(BitmapFrame.Create(s));
        using var ms = new MemoryStream();
        enc.Save(ms);
        return "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
    }
}
