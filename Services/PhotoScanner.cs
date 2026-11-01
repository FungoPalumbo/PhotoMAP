using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoMap.Models;

namespace PhotoMap.Services;

public static class PhotoScanner
{
    static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".heic", ".webp" };

    public const int BatchSize = 300;

    /// <summary>Reads photos in batches of <see cref="BatchSize"/>, handing each batch to <paramref name="onBatch"/>.
    /// Cancelling stops after the batch in progress; photos already delivered are kept.</summary>
    public static Task ScanAsync(string root, bool recurse, IProgress<(int Done, int Total)>? progress,
        Func<List<PhotoItem>, Task> onBatch, CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            var opts = new EnumerationOptions { RecurseSubdirectories = recurse, IgnoreInaccessible = true };
            var files = System.IO.Directory.EnumerateFiles(root, "*", opts)
                .Where(f => Extensions.Contains(System.IO.Path.GetExtension(f)))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            int done = 0, nextId = 0;
            progress?.Report((0, files.Length));
            var po = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount / 2) };

            for (int start = 0; start < files.Length && !ct.IsCancellationRequested; start += BatchSize)
            {
                int n = Math.Min(BatchSize, files.Length - start);
                var results = new PhotoItem?[n];
                try
                {
                    Parallel.For(0, n, po, i =>
                    {
                        results[i] = Read(files[start + i], root);
                        int d = Interlocked.Increment(ref done);
                        if (d % 8 == 0) progress?.Report((d, files.Length));
                    });
                }
                catch (OperationCanceledException) { break; }

                var batch = results.Where(r => r != null).Select(r => r!).ToList();
                foreach (var p in batch) p.Id = nextId++;
                progress?.Report((done, files.Length));
                if (batch.Count > 0) await onBatch(batch);
            }
        });
    }

    static PhotoItem? Read(string path, string root)
    {
        double? lat = null, lng = null;
        DateTime? taken = null;
        string? camera = null, exposure = null;
        int orientation = 1;

        try
        {
            var dirs = ImageMetadataReader.ReadMetadata(path);

            var gps = dirs.OfType<GpsDirectory>().FirstOrDefault();
            if (gps?.GetGeoLocation() is { } g && !g.IsZero)
            {
                lat = g.Latitude;
                lng = g.Longitude;
            }

            var sub = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (sub != null)
            {
                if (sub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dt)) taken = dt;
                var parts = new[]
                {
                    sub.GetDescription(ExifDirectoryBase.TagFNumber),
                    sub.GetDescription(ExifDirectoryBase.TagExposureTime),
                    sub.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso) ? $"ISO {iso}" : null,
                    sub.GetDescription(ExifDirectoryBase.TagFocalLength),
                }.Where(s => !string.IsNullOrWhiteSpace(s));
                exposure = string.Join(" · ", parts);
            }

            var ifd0 = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0 != null)
            {
                var make = ifd0.GetDescription(ExifDirectoryBase.TagMake)?.Trim();
                var model = ifd0.GetDescription(ExifDirectoryBase.TagModel)?.Trim();
                camera = model != null && make != null && !model.StartsWith(make, StringComparison.OrdinalIgnoreCase)
                    ? $"{make} {model}" : model ?? make;
                if (ifd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out var o) && o is >= 1 and <= 8) orientation = o;
                if (taken == null && ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dt2)) taken = dt2;
            }
        }
        catch
        {
            // unreadable metadata: the photo is still listed, just without position
        }

        System.Windows.Media.Imaging.BitmapSource? thumb = null;
        string? uri = null;
        try
        {
            thumb = ImageUtil.CropSquare(ImageUtil.Load(path, 140, orientation));
            if (lat.HasValue) uri = ImageUtil.ToJpegDataUri(thumb);
        }
        catch
        {
            // format not decodable by this machine (e.g. HEIC without codec)
            if (lat.HasValue) return null;
        }

        var dir = System.IO.Path.GetDirectoryName(path) ?? root;
        var rel = System.IO.Path.GetRelativePath(root, dir);
        return new PhotoItem
        {
            Path = path,
            FileName = System.IO.Path.GetFileName(path),
            Folder = rel == "." ? System.IO.Path.GetFileName(root.TrimEnd('\\', '/')) is { Length: > 0 } n ? n : root : rel,
            Latitude = lat,
            Longitude = lng,
            Taken = taken,
            Camera = camera,
            Exposure = exposure,
            Orientation = orientation,
            Thumb = thumb,
            ThumbUri = uri,
        };
    }
}
