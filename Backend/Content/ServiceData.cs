using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Content;

// Ydelser som de sendes til frontenden (GET /api/services og den forudrenderede side)
public record ServiceMediaDto(int Id, string Kind, string? Url, string? YouTubeId, string? Hash);

public record ServiceDto(
    int Id,
    string Slug,
    string Title,
    string ShortText,
    string Body,
    string Icon,
    string? CoverUrl,
    string? CoverHash,
    DateTime UpdatedAt,
    List<ServiceMediaDto> Media);

public static class ServiceData
{
    // Ikonsættet fra ydelseskortene (tegnes i frontend/src/services/icons.ts)
    public static readonly HashSet<string> Icons = new(StringComparer.Ordinal)
    {
        "mount", "bike", "radar", "roof", "deck", "carport", "window", "interior", "facade"
    };

    public static async Task<List<ServiceDto>> ListAsync(DataContext db, CancellationToken ct)
    {
        var services = await db.Services
            .AsNoTracking()
            .Include(s => s.Media)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync(ct);
        return services.Select(ToDto).ToList();
    }

    public static ServiceDto ToDto(Service s) => new(
        s.Id,
        s.Slug,
        s.Title,
        s.ShortText,
        s.Body,
        s.Icon,
        s.CoverImage == null ? null : MediaFiles.UrlFor(s.CoverImage),
        s.CoverImage == null ? null : s.CoverHash,
        s.UpdatedAt,
        s.Media
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Id)
            .Select(m => new ServiceMediaDto(m.Id, m.Kind, m.FileName == null ? null : MediaFiles.UrlFor(m.FileName), m.YouTubeId, m.ContentHash))
            .ToList());
}
