using Backend.Content;
using Backend.Data;
using Backend.Models;
using Backend.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Galleriet ("Billeder" på forsiden): billeder, videoer (mp4/webm) og YouTube-links.
// Samme regler som ydelsernes medier (Content/MediaFiles.cs): endelse, størrelse og filens første bytes
// tjekkes, brugerens filnavn bruges aldrig på disken, og samme fil (SHA-256) gemmes kun én gang.
[ApiController]
[Route("api/gallery")]
public class GalleryController : ControllerBase
{
    private const long MaxRequestBytes = 220L * 1024 * 1024; // fx 2 videoer á 100 MB eller mange billeder

    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;

    public GalleryController(DataContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private string GalleryFolder => MediaFiles.GalleryFolder(_env);

    private static string UrlFor(string fileName) => $"/gallery/{Uri.EscapeDataString(fileName)}";

    private static object ToDto(GalleryImage item) => new
    {
        id = item.Id,
        kind = item.Kind,
        url = item.FileName == null ? null : UrlFor(item.FileName),
        youTubeId = item.YouTubeId,
        posterUrl = item.PosterFileName == null ? null : MediaFiles.PosterUrlFor(item.PosterFileName),
        hash = item.ContentHash
    };

    // GET /api/gallery -> [{ id, kind, url, youTubeId, hash }] i den valgte rækkefølge
    [HttpGet]
    public async Task<IActionResult> GetImages()
    {
        await SyncWithFolderAsync();
        return Ok(await GetOrderedAsync());
    }

    // GET /api/gallery/storage -> plads på serverens disk (kun admin)
    [HttpGet("storage")]
    [Authorize(Roles = "Admin")]
    public IActionResult Storage()
    {
        var space = MediaFiles.DiskSpace(_env);
        var used = Directory.EnumerateFiles(GalleryFolder, "*", SearchOption.AllDirectories)
            .Sum(f => { try { return new FileInfo(f).Length; } catch (IOException) { return 0L; } });
        return Ok(new { usedBytes = used, freeBytes = space?.Free, totalBytes = space?.Total });
    }

    // POST /api/gallery (multipart/form-data, felt "files") -> billeder og/eller videoer (mp4/webm)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> files, CancellationToken ct)
    {
        if (files == null || files.Count == 0)
            return BadRequest("Ingen filer modtaget.");

        // Valider alle filer før noget gemmes
        var kinds = new List<string>();
        foreach (var file in files)
        {
            var (kind, error) = await MediaFiles.ValidateAsync(file, imagesOnly: false);
            if (error != null) return BadRequest(error);
            kinds.Add(kind!);
        }

        var spaceError = MediaFiles.CheckSpace(_env, files.Sum(f => f.Length));
        if (spaceError != null) return BadRequest(spaceError);

        await SyncWithFolderAsync();
        var nextOrder = (await _context.GalleryImages.MaxAsync(i => (int?)i.SortOrder, ct) ?? -1) + 1;

        // En fil, der allerede ligger i galleriet (samme indhold), gemmes ikke igen
        var known = (await _context.GalleryImages.Where(i => i.ContentHash != null).Select(i => i.ContentHash!).ToListAsync(ct))
            .ToHashSet(StringComparer.Ordinal);

        var saved = new List<string>();
        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                var hash = await MediaHashes.OfUploadAsync(files[i], ct);
                if (!known.Add(hash)) continue;

                var fileName = await MediaFiles.SaveAsync(files[i], GalleryFolder, ct);
                saved.Add(fileName);
                _context.GalleryImages.Add(new GalleryImage
                {
                    Kind = kinds[i],
                    FileName = fileName,
                    ContentHash = hash,
                    SortOrder = nextOrder++,
                    UploadedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var fileName in saved) MediaFiles.TryDelete(GalleryFolder, fileName);
            throw;
        }

        return Ok(await GetOrderedAsync());
    }

    // POST /api/gallery/youtube { "url": "https://www.youtube.com/watch?v=..." }
    [HttpPost("youtube")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddYouTube([FromBody] YouTubeLinkDto dto, CancellationToken ct)
    {
        var videoId = MediaFiles.ParseYouTubeId(dto.Url);
        if (videoId == null) return BadRequest("Det ligner ikke et YouTube-link. Kopiér adressen på videoen fra YouTube (fx https://youtu.be/...).");

        // Samme video tilføjes kun én gang
        if (!await _context.GalleryImages.AnyAsync(i => i.Kind == "youtube" && i.YouTubeId == videoId, ct))
        {
            var nextOrder = (await _context.GalleryImages.MaxAsync(i => (int?)i.SortOrder, ct) ?? -1) + 1;
            _context.GalleryImages.Add(new GalleryImage { Kind = "youtube", YouTubeId = videoId, SortOrder = nextOrder, UploadedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync(ct);
        }

        return Ok(await GetOrderedAsync());
    }

    // POST /api/gallery/5/poster (multipart, felt "file") -> stillbillede til en video (jpg/png/webp)
    [HttpPost("{id:int}/poster")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MediaFiles.MaxImageBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MediaFiles.MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadPoster(int id, IFormFile? file, CancellationToken ct)
    {
        var item = await _context.GalleryImages.FindAsync([id], ct);
        if (item == null) return NotFound();
        if (item.Kind != "video") return BadRequest("Kun videoer kan have et stillbillede.");
        if (file == null) return BadRequest("Ingen fil modtaget.");

        var (_, error) = await MediaFiles.ValidateAsync(file, imagesOnly: true);
        if (error != null) return BadRequest(error);
        var spaceError = MediaFiles.CheckSpace(_env, file.Length);
        if (spaceError != null) return BadRequest(spaceError);

        var folder = MediaFiles.PosterFolder(_env);
        var fileName = await MediaFiles.SaveAsync(file, folder, ct);
        var old = item.PosterFileName;
        item.PosterFileName = fileName;
        await _context.SaveChangesAsync(ct);
        MediaFiles.TryDelete(folder, old);

        return Ok(await GetOrderedAsync());
    }

    // DELETE /api/gallery/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.GalleryImages.FindAsync(id);
        if (item == null) return NotFound();

        if (item.FileName != null)
        {
            // Var det forsidebilledet, nulstilles det
            var hero = await _context.SiteTexts.FindAsync("hero.image");
            if (hero != null && hero.Value == UrlFor(item.FileName))
            {
                hero.Value = "";
            }
        }

        var fileName = item.FileName;
        var poster = item.PosterFileName;
        _context.GalleryImages.Remove(item);
        await _context.SaveChangesAsync();
        MediaFiles.TryDelete(GalleryFolder, fileName);
        MediaFiles.TryDelete(MediaFiles.PosterFolder(_env), poster);

        return Ok(await GetOrderedAsync());
    }

    // PUT /api/gallery/order { "ids": [3, 1, 2] } -> ny rækkefølge
    [HttpPut("order")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reorder([FromBody] ReorderDto dto)
    {
        var images = await _context.GalleryImages.ToListAsync();
        var existing = images.Select(i => i.Id).ToHashSet();

        if (dto.Ids.Count != existing.Count || dto.Ids.Distinct().Count() != dto.Ids.Count || !dto.Ids.All(existing.Contains))
            return BadRequest("Listen skal indeholde alle id'er præcis én gang.");

        var byId = images.ToDictionary(i => i.Id);
        for (var index = 0; index < dto.Ids.Count; index++)
        {
            byId[dto.Ids[index]].SortOrder = index;
        }

        await _context.SaveChangesAsync();
        return Ok(await GetOrderedAsync());
    }

    private async Task<List<object>> GetOrderedAsync()
    {
        var images = await _context.GalleryImages
            .AsNoTracking()
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .ToListAsync();
        return images.Select(ToDto).ToList();
    }

    // Holder databasen i takt med mappen: filer lagt ind manuelt tilføjes, slettede filer fjernes.
    // YouTube-rækker har ingen fil og røres ikke.
    private async Task SyncWithFolderAsync()
    {
        var filesOnDisk = Directory.EnumerateFiles(GalleryFolder)
            .Select(f => Path.GetFileName(f)!)
            .Where(f => MediaFiles.KindFromExtension(f) != null)
            .ToHashSet(StringComparer.Ordinal);

        var inDb = await _context.GalleryImages.ToListAsync();
        var changed = false;

        foreach (var image in inDb.Where(i => i.FileName != null && !filesOnDisk.Contains(i.FileName)))
        {
            _context.GalleryImages.Remove(image);
            changed = true;
        }

        var known = inDb.Where(i => i.FileName != null).Select(i => i.FileName!).ToHashSet(StringComparer.Ordinal);
        var nextOrder = inDb.Count == 0 ? 0 : inDb.Max(i => i.SortOrder) + 1;

        foreach (var name in filesOnDisk.Where(n => !known.Contains(n)).OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            _context.GalleryImages.Add(new GalleryImage
            {
                Kind = MediaFiles.KindFromExtension(name)!,
                FileName = name,
                ContentHash = MediaHashes.OfFile(GalleryFolder, name),
                SortOrder = nextOrder++
            });
            changed = true;
        }

        // Filer fra før hashes blev gemt
        foreach (var image in inDb.Where(i => i.ContentHash == null && i.FileName != null && filesOnDisk.Contains(i.FileName)))
        {
            image.ContentHash = MediaHashes.OfFile(GalleryFolder, image.FileName);
            if (image.ContentHash != null) changed = true;
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
    }
}
