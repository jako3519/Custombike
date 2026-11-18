using System.Net.Mail;
using System.Text.RegularExpressions;
using Backend.Contact;
using Backend.Content;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Contact;

[ApiController]
[Route("api/contact")]
public sealed partial class ContactController(DataContext db, IWebHostEnvironment env, ContactEmailSender mail, ILogger<ContactController> logger) : ControllerBase
{
    public const int MaxImages = 5;
    public const int MaxMessageLength = 4000;

    public sealed class ContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public string ServiceKey { get; set; } = "";
        public string? ServiceTitle { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? HandledAt { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    public sealed class AttachmentDto
    {
        public int Id { get; set; }
        public string OriginalName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
    }

    // Offentlig formular. Honeypot (hp_note_x) der er udfyldt, behandles som en succes uden at gemme.
    public const int MinFillMilliseconds = 3000;

    [AllowAnonymous]
    [HttpPost]
    [RequestSizeLimit((MaxImages * MediaFiles.MaxImageBytes) + 1_000_000)]
    public async Task<IActionResult> Submit(
        [FromForm] string? name,
        [FromForm] string? phone,
        [FromForm] string? email,
        [FromForm] string? service,
        [FromForm] string? message,
        [FromForm] string? hp_note_x,
        [FromForm] string? hp_t,
        [FromForm] IFormFileCollection? files,
        CancellationToken ct)
    {
        // Honningkrukke: bots udfylder ofte skjulte felter. Vi siger ja, men gemmer ingenting.
        if (!string.IsNullOrWhiteSpace(hp_note_x))
        {
            logger.LogInformation("Kontaktformular: honeypot udfyldt – henvendelsen er ignoreret.");
            return Ok(new { ok = true });
        }

        // For hurtig udfyldning logges kun: en rigtig kunde kan nå det på under 3 sekunder,
        // og formularen først vises når boblen åbnes.
        if (LooksTooFast(hp_t, out var elapsedMs))
            logger.LogInformation("Kontaktformular: udfyldt på {Elapsed} ms (under {Min} ms) – gemmes alligevel.", elapsedMs, MinFillMilliseconds);
        else if (string.IsNullOrWhiteSpace(hp_t))
            logger.LogInformation("Kontaktformular: intet tidsstempel medsendt – gemmes alligevel.");

        var error = await ValidateAndBuildAsync(name, phone, email, service, message, files, ct);
        if (error.Error != null) return BadRequest(error.Error);

        var request = error.Request!;
        var saved = new List<string>();
        var folder = MediaFiles.ContactFolder(env);
        try
        {
            if (error.Uploads.Count > 0)
            {
                var space = MediaFiles.CheckSpace(env, error.Uploads.Sum(f => f.Length));
                if (space != null) return BadRequest(space);
            }

            db.ContactRequests.Add(request);
            await db.SaveChangesAsync(ct);

            foreach (var file in error.Uploads)
            {
                var fileName = await MediaFiles.SaveAsync(file, folder, ct);
                saved.Add(fileName);
                request.Attachments.Add(new ContactAttachment
                {
                    FileName = fileName,
                    OriginalName = Path.GetFileName(file.FileName) is { Length: > 0 and <= 255 } n ? n : fileName,
                    ContentType = MediaFiles.ContactContentType(file),
                    Size = file.Length,
                });
            }
            if (saved.Count > 0) await db.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var fileName in saved) MediaFiles.TryDelete(folder, fileName);
            throw;
        }

        // Mail-fejl må ikke få kaldet til at fejle
        try { await mail.TrySendAsync(request, ct); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Uventet fejl ved sending af kontakt-e-mail for henvendelse {Id}.", request.Id);
        }

        return Ok(new { ok = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<ContactDto>>> List(CancellationToken ct)
    {
        var rows = await db.ContactRequests
            .AsNoTracking()
            .Include(r => r.Attachments)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/handled")]
    public async Task<IActionResult> MarkHandled(int id, CancellationToken ct)
    {
        var row = await db.ContactRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (row == null) return NotFound();
        row.HandledAt = row.HandledAt == null ? DateTime.UtcNow : null;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(row));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var row = await db.ContactRequests.Include(r => r.Attachments).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (row == null) return NotFound();
        var folder = MediaFiles.ContactFolder(env);
        foreach (var file in row.Attachments) MediaFiles.TryDelete(folder, file.FileName);
        db.ContactRequests.Remove(row);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> Download(int id, int attachmentId, CancellationToken ct)
    {
        var file = await db.ContactAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ContactRequestId == id, ct);
        if (file == null) return NotFound();
        var path = Path.Combine(MediaFiles.ContactFolder(env), Path.GetFileName(file.FileName));
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, file.ContentType, file.OriginalName);
    }

    private async Task<(string? Error, ContactRequest? Request, List<IFormFile> Uploads)> ValidateAndBuildAsync(
        string? name, string? phone, string? email, string? service, string? message, IFormFileCollection? files, CancellationToken ct)
    {
        var uploads = new List<IFormFile>();
        name = (name ?? "").Trim();
        phone = (phone ?? "").Trim();
        email = (email ?? "").Trim();
        service = (service ?? "").Trim();
        message = (message ?? "").Trim();

        if (name.Length is < 2 or > 120) return ("Skriv dit navn (højst 120 tegn).", null, uploads);
        if (!LooksLikePhone(phone)) return ("Skriv et telefonnummer, vi kan ringe til.", null, uploads);
        if (phone.Length > 40) return ("Telefonnummeret er for langt.", null, uploads);
        if (email.Length > 0 && !LooksLikeEmail(email)) return ("E-mailadressen ser ikke rigtig ud.", null, uploads);
        if (message.Length == 0) return ("Skriv en kort besked om opgaven.", null, uploads);
        if (message.Length > MaxMessageLength) return ($"Beskeden må højst være {MaxMessageLength} tegn.", null, uploads);

        string? serviceTitle;
        if (service == "andet")
        {
            serviceTitle = "Andet";
        }
        else if (service.Length is > 0 and <= 80)
        {
            var svc = await db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Slug == service, ct);
            if (svc == null) return ("Vælg et produkt, eller vælg Andet.", null, uploads);
            serviceTitle = svc.Title;
        }
        else
        {
            return ("Vælg et produkt, eller vælg Andet.", null, uploads);
        }

        if (files != null)
        {
            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;
                uploads.Add(file);
            }
        }
        // Bestilling af et produkt kræver mindst ét billede af cyklen. "Andet" er almindelig kontakt.
        if (service != "andet" && uploads.Count < 1)
            return ("Ved bestilling af et produkt skal du vedhæfte mindst ét billede af cyklen fra siden.", null, uploads);
        if (uploads.Count > MaxImages) return ($"Du kan højst vedhæfte {MaxImages} billeder.", null, uploads);
        foreach (var file in uploads)
        {
            var err = await MediaFiles.ValidateContactImageAsync(file);
            if (err != null) return (err, null, uploads);
        }

        var request = new ContactRequest
        {
            Name = name,
            Phone = phone,
            Email = email.Length == 0 ? null : email,
            ServiceKey = service,
            ServiceTitle = serviceTitle,
            Message = message,
            CreatedAt = DateTime.UtcNow,
        };
        return (null, request, uploads);
    }

    private static ContactDto ToDto(ContactRequest r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Phone = r.Phone,
        Email = r.Email,
        ServiceKey = r.ServiceKey,
        ServiceTitle = r.ServiceTitle,
        Message = r.Message,
        CreatedAt = r.CreatedAt,
        HandledAt = r.HandledAt,
        Attachments = r.Attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            OriginalName = a.OriginalName,
            ContentType = a.ContentType,
            Size = a.Size,
        }).ToList(),
    };

    private static bool LooksTooFast(string? raw, out long elapsedMs)
    {
        elapsedMs = 0;
        if (!long.TryParse(raw, out var started) || started <= 0) return false;
        elapsedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - started;
        if (elapsedMs < 0 || elapsedMs > TimeSpan.FromDays(1).TotalMilliseconds) return false;
        return elapsedMs < MinFillMilliseconds;
    }

    private static bool LooksLikePhone(string phone)
    {
        var digits = PhoneDigitsRegex().Replace(phone, "");
        return digits.Length is >= 8 and <= 15;
    }

    private static bool LooksLikeEmail(string email)
    {
        if (email.Length > 254) return false;
        try { _ = new MailAddress(email); }
        catch (FormatException) { return false; }
        return email.Contains('.');
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex PhoneDigitsRegex();
}
