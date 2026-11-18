using System.Security.Cryptography;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Content;

// SHA-256 af filernes indhold. Samme fil uploadet flere gange (i galleriet og/eller på en ydelse)
// får samme hash, så den kun vises én gang dér, hvor billederne vises sammen.
public static class MediaHashes
{
    public static async Task<string> OfUploadAsync(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, ct));
    }

    // null hvis filen mangler eller ikke kan læses
    public static string? OfFile(string folder, string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        var path = Path.Combine(folder, Path.GetFileName(fileName));
        try
        {
            if (!File.Exists(path)) return null;
            using var stream = File.OpenRead(path);
            return Convert.ToHexStringLower(SHA256.HashData(stream));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    // Udfylder manglende hashes for filer, der fandtes før hashes blev gemt (køres ved opstart)
    public static async Task<int> BackfillAsync(DataContext db, IWebHostEnvironment env, CancellationToken ct)
    {
        var galleryFolder = MediaFiles.GalleryFolder(env);
        var servicesFolder = MediaFiles.Folder(env);
        var updated = 0;

        foreach (var image in await db.GalleryImages.Where(i => i.FileName != null && i.ContentHash == null).ToListAsync(ct))
        {
            image.ContentHash = OfFile(galleryFolder, image.FileName);
            if (image.ContentHash != null) updated++;
        }

        foreach (var media in await db.ServiceMedia.Where(m => m.FileName != null && m.ContentHash == null).ToListAsync(ct))
        {
            media.ContentHash = OfFile(servicesFolder, media.FileName);
            if (media.ContentHash != null) updated++;
        }

        foreach (var service in await db.Services.Where(s => s.CoverImage != null && s.CoverHash == null).ToListAsync(ct))
        {
            service.CoverHash = OfFile(servicesFolder, service.CoverImage);
            if (service.CoverHash != null) updated++;
        }

        if (updated > 0) await db.SaveChangesAsync(ct);
        return updated;
    }
}
