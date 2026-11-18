using Backend.Content;
using Backend.Data;
using Backend.Models;
using Backend.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Ydelser: læses af alle, oprettes/rettes/slettes kun af admin.
// Filer (forsidebillede, billeder og videoer) ligger i wwwroot/gallery/services - samme volume som galleriet.
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private const int MaxTitle = 120;
    private const int MaxShortText = 2000;
    private const int MaxBody = 20000;
    private const int MaxMediaPerService = 100;
    private const long MaxMediaRequestBytes = 220L * 1024 * 1024; // fx 2 videoer á 100 MB eller mange billeder

    private readonly DataContext _db;
    private readonly IWebHostEnvironment _env;

    public ServicesController(DataContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private string Folder => MediaFiles.Folder(_env);

    // GET /api/services -> alle ydelser i rækkefølge, inkl. billeder/videoer
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await ServiceData.ListAsync(_db, ct));

    // GET /api/services/tagarbejde
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var service = await _db.Services.AsNoTracking().Include(s => s.Media).FirstOrDefaultAsync(s => s.Slug == slug, ct);
        return service == null ? NotFound() : Ok(ServiceData.ToDto(service));
    }

    // POST /api/services { title, slug?, shortText, body, icon } -> den nye ydelse
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ServiceInputDto dto, CancellationToken ct)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        var slug = Slug.From(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Title : dto.Slug);
        if (slug.Length == 0) return BadRequest("Titlen skal indeholde bogstaver eller tal, så siden kan få en adresse.");

        // Findes adressen allerede, tilføjes -2, -3 ...
        var taken = await _db.Services.Where(s => s.Slug == slug || s.Slug.StartsWith(slug + "-")).Select(s => s.Slug).ToListAsync(ct);
        var unique = slug;
        for (var n = 2; taken.Contains(unique); n++)
        {
            var suffix = $"-{n}";
            unique = (slug.Length + suffix.Length > Slug.MaxLength ? slug[..(Slug.MaxLength - suffix.Length)].TrimEnd('-') : slug) + suffix;
        }

        var nextOrder = (await _db.Services.MaxAsync(s => (int?)s.SortOrder, ct) ?? -1) + 1;
        var now = DateTime.UtcNow;
        var service = new Service
        {
            Slug = unique,
            Title = dto.Title.Trim(),
            ShortText = dto.ShortText.Trim(),
            Body = dto.Body.Trim(),
            Icon = dto.Icon,
            SortOrder = nextOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.Services.Add(service);
        await _db.SaveChangesAsync(ct);
        return Ok(ServiceData.ToDto(service));
    }

    // PUT /api/services/5 { title, slug, shortText, body, icon } -> alle ydelser
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceInputDto dto, CancellationToken ct)
    {
        var service = await _db.Services.FindAsync([id], ct);
        if (service == null) return NotFound();

        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        // Adressen ændres kun, hvis admin selv har rettet den (så links til siden ikke går i stykker)
        if (!string.IsNullOrWhiteSpace(dto.Slug))
        {
            var slug = Slug.From(dto.Slug);
            if (slug.Length == 0) return BadRequest("Adressen skal indeholde bogstaver eller tal.");
            if (slug != service.Slug && await _db.Services.AnyAsync(s => s.Slug == slug && s.Id != id, ct))
                return BadRequest($"Adressen /produkter/{slug} bruges allerede af et andet produkt.");
            service.Slug = slug;
        }

        service.Title = dto.Title.Trim();
        service.ShortText = dto.ShortText.Trim();
        service.Body = dto.Body.Trim();
        service.Icon = dto.Icon;
        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // DELETE /api/services/5 -> sletter ydelsen og alle dens filer
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var service = await _db.Services.Include(s => s.Media).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (service == null) return NotFound();

        var files = service.Media.Select(m => m.FileName).Append(service.CoverImage).ToList();
        _db.Services.Remove(service); // medierne slettes med (cascade)
        await _db.SaveChangesAsync(ct);
        foreach (var file in files) MediaFiles.TryDelete(Folder, file);

        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // PUT /api/services/order { "ids": [3, 1, 2] }
    [HttpPut("order")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reorder([FromBody] ReorderDto dto, CancellationToken ct)
    {
        var services = await _db.Services.ToListAsync(ct);
        if (!IsPermutation(dto.Ids, services.Select(s => s.Id)))
            return BadRequest("Listen skal indeholde alle produkters id'er præcis én gang.");

        var byId = services.ToDictionary(s => s.Id);
        for (var i = 0; i < dto.Ids.Count; i++) byId[dto.Ids[i]].SortOrder = i;
        await _db.SaveChangesAsync(ct);
        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // POST /api/services/5/cover (multipart, felt "file") -> nyt forsidebillede
    [HttpPost("{id:int}/cover")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MediaFiles.MaxImageBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MediaFiles.MaxImageBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadCover(int id, IFormFile? file, CancellationToken ct)
    {
        var service = await _db.Services.FindAsync([id], ct);
        if (service == null) return NotFound();
        if (file == null) return BadRequest("Ingen fil modtaget.");

        var (_, error) = await MediaFiles.ValidateAsync(file, imagesOnly: true);
        if (error != null) return BadRequest(error);
        var spaceError = MediaFiles.CheckSpace(_env, file.Length);
        if (spaceError != null) return BadRequest(spaceError);

        var hash = await MediaHashes.OfUploadAsync(file, ct);
        var fileName = await MediaFiles.SaveAsync(file, Folder, ct);
        var old = service.CoverImage;
        service.CoverImage = fileName;
        service.CoverHash = hash;
        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        MediaFiles.TryDelete(Folder, old);

        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // DELETE /api/services/5/cover
    [HttpDelete("{id:int}/cover")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCover(int id, CancellationToken ct)
    {
        var service = await _db.Services.FindAsync([id], ct);
        if (service == null) return NotFound();

        var old = service.CoverImage;
        service.CoverImage = null;
        service.CoverHash = null;
        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        MediaFiles.TryDelete(Folder, old);

        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // POST /api/services/5/media (multipart, felt "files") -> billeder og/eller videoer (mp4/webm)
    [HttpPost("{id:int}/media")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(MaxMediaRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxMediaRequestBytes)]
    public async Task<IActionResult> UploadMedia(int id, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        var service = await _db.Services.Include(s => s.Media).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (service == null) return NotFound();
        if (files == null || files.Count == 0) return BadRequest("Ingen filer modtaget.");
        if (service.Media.Count + files.Count > MaxMediaPerService)
            return BadRequest($"Et produkt kan højst have {MaxMediaPerService} billeder og videoer.");

        // Valider alle filer, før noget gemmes
        var kinds = new List<string>();
        foreach (var file in files)
        {
            var (kind, error) = await MediaFiles.ValidateAsync(file, imagesOnly: false);
            if (error != null) return BadRequest(error);
            kinds.Add(kind!);
        }

        var spaceError = MediaFiles.CheckSpace(_env, files.Sum(f => f.Length));
        if (spaceError != null) return BadRequest(spaceError);

        var nextOrder = service.Media.Count == 0 ? 0 : service.Media.Max(m => m.SortOrder) + 1;
        // En fil, som ydelsen allerede har (samme indhold), gemmes ikke igen
        var known = service.Media.Where(m => m.ContentHash != null).Select(m => m.ContentHash!).ToHashSet(StringComparer.Ordinal);
        var saved = new List<string>();
        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                var hash = await MediaHashes.OfUploadAsync(files[i], ct);
                if (!known.Add(hash)) continue;
                var fileName = await MediaFiles.SaveAsync(files[i], Folder, ct);
                saved.Add(fileName);
                service.Media.Add(new ServiceMedia { Kind = kinds[i], FileName = fileName, ContentHash = hash, SortOrder = nextOrder++, CreatedAt = DateTime.UtcNow });
            }
            service.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var fileName in saved) MediaFiles.TryDelete(Folder, fileName);
            throw;
        }

        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // POST /api/services/5/media/youtube { "url": "https://www.youtube.com/watch?v=..." }
    [HttpPost("{id:int}/media/youtube")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddYouTube(int id, [FromBody] YouTubeLinkDto dto, CancellationToken ct)
    {
        var service = await _db.Services.Include(s => s.Media).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (service == null) return NotFound();
        if (service.Media.Count >= MaxMediaPerService)
            return BadRequest($"Et produkt kan højst have {MaxMediaPerService} billeder og videoer.");

        var videoId = MediaFiles.ParseYouTubeId(dto.Url);
        if (videoId == null) return BadRequest("Det ligner ikke et YouTube-link. Kopiér adressen på videoen fra YouTube (fx https://youtu.be/...).");

        var nextOrder = service.Media.Count == 0 ? 0 : service.Media.Max(m => m.SortOrder) + 1;
        service.Media.Add(new ServiceMedia { Kind = "youtube", YouTubeId = videoId, SortOrder = nextOrder, CreatedAt = DateTime.UtcNow });
        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // DELETE /api/services/5/media/12
    [HttpDelete("{id:int}/media/{mediaId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMedia(int id, int mediaId, CancellationToken ct)
    {
        var media = await _db.ServiceMedia.FirstOrDefaultAsync(m => m.Id == mediaId && m.ServiceId == id, ct);
        if (media == null) return NotFound();

        var fileName = media.FileName;
        _db.ServiceMedia.Remove(media);
        var service = await _db.Services.FindAsync([id], ct);
        if (service != null) service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        MediaFiles.TryDelete(Folder, fileName);

        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    // PUT /api/services/5/media/order { "ids": [12, 10, 11] }
    [HttpPut("{id:int}/media/order")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReorderMedia(int id, [FromBody] ReorderDto dto, CancellationToken ct)
    {
        var media = await _db.ServiceMedia.Where(m => m.ServiceId == id).ToListAsync(ct);
        if (!IsPermutation(dto.Ids, media.Select(m => m.Id)))
            return BadRequest("Listen skal indeholde alle billed- og video-id'er præcis én gang.");

        var byId = media.ToDictionary(m => m.Id);
        for (var i = 0; i < dto.Ids.Count; i++) byId[dto.Ids[i]].SortOrder = i;
        await _db.SaveChangesAsync(ct);
        return Ok(await ServiceData.ListAsync(_db, ct));
    }

    private static string? Validate(ServiceInputDto dto)
    {
        var title = (dto.Title ?? "").Trim();
        if (title.Length == 0) return "Produktet skal have et navn.";
        if (title.Length > MaxTitle) return $"Navnet må højst være {MaxTitle} tegn.";
        if ((dto.ShortText ?? "").Trim().Length > MaxShortText) return $"Teksten på kortet må højst være {MaxShortText} tegn.";
        if ((dto.Body ?? "").Trim().Length > MaxBody) return $"Teksten på siden må højst være {MaxBody} tegn.";
        if (!ServiceData.Icons.Contains(dto.Icon ?? "")) return "Vælg et af ikonerne.";
        dto.ShortText ??= "";
        dto.Body ??= "";
        return null;
    }

    private static bool IsPermutation(List<int> ids, IEnumerable<int> existing)
    {
        var set = existing.ToHashSet();
        return ids.Count == set.Count && ids.Distinct().Count() == ids.Count && ids.All(set.Contains);
    }
}
