using System.Text;
using System.Text.RegularExpressions;

namespace Backend.Content;

// Fælles regler for upload af billeder og videoer til ydelserne.
// Samme strenghed som galleriet: tilladt endelse, størrelse og filens første bytes ("magic bytes")
// tjekkes, og brugerens filnavn bruges aldrig på disken.
public static partial class MediaFiles
{
    public const long MaxImageBytes = 15L * 1024 * 1024;   // 15 MB pr. billede (som galleriet)
    public const long MaxVideoBytes = 100L * 1024 * 1024;  // 100 MB pr. video

    public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif"
    };

    public static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm" };

    // Plads der altid skal være tilbage på disken (volumen er lille: 1 GB på Fly)
    public const long MinFreeBytes = 25L * 1024 * 1024;

    // Endelse -> den eneste tilladte content type
    private static readonly Dictionary<string, string> VideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
    };

    // Almindelige MP4-"brands" (ikke QuickTime/MOV, HEIC eller AVIF)
    private static readonly HashSet<string> Mp4Brands = new(StringComparer.Ordinal)
    {
        "isom", "iso2", "iso3", "iso4", "iso5", "iso6", "mp41", "mp42", "avc1", "dash", "M4V ", "M4VP", "mmp4", "MSNV", "f4v "
    };

    // Mappen til ydelsernes filer: wwwroot/gallery/services (ligger på samme volume som galleriet)
    public static string Folder(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "gallery", "services");
        Directory.CreateDirectory(folder);
        return folder;
    }

    // Galleriets mappe: wwwroot/gallery
    public static string GalleryFolder(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "gallery");
        Directory.CreateDirectory(folder);
        return folder;
    }

    // "image" / "video" ud fra endelsen (til filer, der er lagt i mappen manuelt), ellers null
    public static string? KindFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ImageExtensions.Contains(ext) ? "image" : VideoExtensions.Contains(ext) ? "video" : null;
    }

    // Diskplads på volumen med galleriet (og ydelsernes filer). null hvis den ikke kan læses.
    public static (long Total, long Free)? DiskSpace(IWebHostEnvironment env)
    {
        try
        {
            var drive = new DriveInfo(GalleryFolder(env));
            return (drive.TotalSize, drive.AvailableFreeSpace);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return null;
        }
    }

    // Fejltekst hvis der ikke er plads til filerne, ellers null
    public static string? CheckSpace(IWebHostEnvironment env, long incomingBytes)
    {
        var space = DiskSpace(env);
        if (space == null) return null;
        if (space.Value.Free - incomingBytes >= MinFreeBytes) return null;
        var freeMb = Math.Max(0, space.Value.Free - MinFreeBytes) / (1024 * 1024);
        return $"Der er ikke plads nok på serveren (ca. {freeMb} MB tilbage). Slet nogle billeder eller videoer først.";
    }

    // Videoernes stillbilleder: wwwroot/gallery/posters (en undermappe, så galleriet ikke tager dem med som billeder)
    public static string PosterFolder(IWebHostEnvironment env)
    {
        var folder = Path.Combine(GalleryFolder(env), "posters");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string PosterUrlFor(string fileName) => $"/gallery/posters/{Uri.EscapeDataString(fileName)}";

    // Vedhæftede billeder fra kontaktformularen: samme volume som galleriet, men en undermappe
    // som galleri-synkroniseringen ikke tager med, og som ikke serveres offentligt.
    public static string ContactFolder(IWebHostEnvironment env)
    {
        var folder = Path.Combine(GalleryFolder(env), "contact");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static readonly HashSet<string> ContactImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"
    };

    // jpg/png/webp/heic til kontaktformularen (ikke galleriets øvrige formater)
    public static async Task<string?> ValidateContactImageAsync(IFormFile file)
    {
        var name = file.FileName;
        var ext = Path.GetExtension(name);
        if (file.Length == 0) return $"'{name}' er tom.";
        if (!ContactImageExtensions.Contains(ext))
            return $"'{name}' er ikke et tilladt billedformat (jpg, png, webp eller heic).";
        if (file.Length > MaxImageBytes) return $"'{name}' er større end 15 MB.";
        if (!await LooksLikeContactImageAsync(file, ext)) return $"'{name}' ligner ikke et rigtigt billede.";
        return null;
    }

    public static string ContactContentType(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" or ".heif" => "image/heic",
            _ => "application/octet-stream",
        };
    }

    public static string UrlFor(string fileName) => $"/gallery/services/{Uri.EscapeDataString(fileName)}";

    // Returnerer "image" eller "video" - eller en fejltekst til admin
    public static async Task<(string? Kind, string? Error)> ValidateAsync(IFormFile file, bool imagesOnly)
    {
        var name = file.FileName;
        var ext = Path.GetExtension(name);
        if (file.Length == 0) return (null, $"'{name}' er tom.");

        if (ImageExtensions.Contains(ext))
        {
            if (file.Length > MaxImageBytes) return (null, $"'{name}' er større end 15 MB.");
            if (!await LooksLikeImageAsync(file, ext)) return (null, $"'{name}' ligner ikke et rigtigt billede.");
            return ("image", null);
        }

        if (!imagesOnly && VideoTypes.TryGetValue(ext, out var expectedType))
        {
            var contentType = (file.ContentType ?? "").Split(';')[0].Trim();
            if (!contentType.Equals(expectedType, StringComparison.OrdinalIgnoreCase))
                return (null, $"'{name}' har ikke den rigtige filtype ({expectedType}).");
            if (file.Length > MaxVideoBytes) return (null, $"'{name}' er større end 100 MB.");
            if (!await LooksLikeVideoAsync(file, ext)) return (null, $"'{name}' ligner ikke en rigtig {ext.TrimStart('.')}-video.");
            return ("video", null);
        }

        return (null, imagesOnly
            ? $"'{name}' er ikke et tilladt billedformat (jpg, png, webp, avif, gif)."
            : $"'{name}' er ikke tilladt. Brug billeder (jpg, png, webp, avif, gif) eller video (mp4, webm).");
    }

    // Gemmer filen under et tilfældigt navn og returnerer navnet
    public static async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct)
    {
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);
        try
        {
            await using var stream = File.Create(path);
            await file.CopyToAsync(stream, ct);
        }
        catch
        {
            TryDelete(folder, fileName);
            throw;
        }
        return fileName;
    }

    public static void TryDelete(string folder, string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        var path = Path.Combine(folder, Path.GetFileName(fileName));
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException)
        {
            // Filen ryddes op senere; databasen er det, der bestemmer hvad der vises
        }
    }

    // YouTube-link (watch, youtu.be, shorts, embed, live) eller selve id'et -> 11-tegns video-id
    public static string? ParseYouTubeId(string? input)
    {
        var s = (input ?? "").Trim();
        if (s.Length == 0 || s.Length > 500) return null;
        if (YouTubeIdRegex().IsMatch(s)) return s;
        if (!s.Contains("://")) s = "https://" + s;
        if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http")) return null;

        var host = uri.Host.ToLowerInvariant();
        foreach (var prefix in new[] { "www.", "m.", "music." })
            if (host.StartsWith(prefix)) host = host[prefix.Length..];

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? id = null;
        if (host == "youtu.be")
        {
            id = segments.FirstOrDefault();
        }
        else if (host is "youtube.com" or "youtube-nocookie.com")
        {
            if (segments.Length == 1 && segments[0] == "watch")
                id = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("v", out var v) ? v.ToString() : null;
            else if (segments.Length >= 2 && segments[0] is "embed" or "shorts" or "live" or "v")
                id = segments[1];
        }
        return id != null && YouTubeIdRegex().IsMatch(id) ? id : null;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex YouTubeIdRegex();

    private static async Task<byte[]> ReadHeaderAsync(IFormFile file, int length)
    {
        var header = new byte[length];
        await using var stream = file.OpenReadStream();
        var read = 0;
        while (read < length)
        {
            var n = await stream.ReadAsync(header.AsMemory(read, length - read));
            if (n == 0) break;
            read += n;
        }
        return read == length ? header : header[..read];
    }

    // Lidt strengere end galleriet: indholdet skal passe til endelsen (en GIF med navnet .jpg afvises),
    // så filen altid serveres med den rigtige Content-Type.
    private static async Task<bool> LooksLikeImageAsync(IFormFile file, string ext)
    {
        var header = await ReadHeaderAsync(file, 12);
        if (header.Length < 12) return false;

        bool StartsWith(params byte[] sig) => header.Take(sig.Length).SequenceEqual(sig);
        string Ascii(int start, int len) => Encoding.ASCII.GetString(header, start, len);

        return ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => StartsWith(0xFF, 0xD8, 0xFF),
            ".png" => StartsWith(0x89, 0x50, 0x4E, 0x47),
            ".gif" => Ascii(0, 4) == "GIF8",
            ".webp" => Ascii(0, 4) == "RIFF" && Ascii(8, 4) == "WEBP",
            ".avif" => Ascii(4, 4) == "ftyp" && Ascii(8, 4) is "avif" or "avis" or "mif1" or "msf1",
            _ => false,
        };
    }

    private static async Task<bool> LooksLikeContactImageAsync(IFormFile file, string ext)
    {
        var lower = ext.ToLowerInvariant();
        if (lower is ".heic" or ".heif")
        {
            var header = await ReadHeaderAsync(file, 16);
            if (header.Length < 12) return false;
            if (Encoding.ASCII.GetString(header, 4, 4) != "ftyp") return false;
            var brand = Encoding.ASCII.GetString(header, 8, 4);
            return brand is "heic" or "heix" or "heif" or "heim" or "heis" or "hevc" or "hevx" or "mif1" or "msf1";
        }
        return await LooksLikeImageAsync(file, ext);
    }

    private static async Task<bool> LooksLikeVideoAsync(IFormFile file, string ext)
    {
        var header = await ReadHeaderAsync(file, 64);
        if (header.Length < 12) return false;
        string Ascii(int start, int len) => Encoding.ASCII.GetString(header, start, len);

        if (ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            return Ascii(4, 4) == "ftyp" && Mp4Brands.Contains(Ascii(8, 4));

        // WebM: EBML-header (1A 45 DF A3) med DocType "webm"
        return header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3
            && Encoding.ASCII.GetString(header).Contains("webm", StringComparison.Ordinal);
    }
}
