using Backend.Content;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Contact;

// Sletter henvendelser (og deres billeder) der er ældre end 12 måneder. Kører kort efter
// opstart og derefter hver 12. time.
public sealed class ContactCleanupService(IServiceScopeFactory scopes, IWebHostEnvironment env, ILogger<ContactCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(12));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                var cutoff = DateTime.UtcNow.AddMonths(-12);
                var old = await db.ContactRequests
                    .Include(r => r.Attachments)
                    .Where(r => r.CreatedAt < cutoff)
                    .ToListAsync(stoppingToken);
                if (old.Count == 0) continue;

                var folder = MediaFiles.ContactFolder(env);
                foreach (var req in old)
                    foreach (var file in req.Attachments)
                        MediaFiles.TryDelete(folder, file.FileName);

                db.ContactRequests.RemoveRange(old);
                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Henvendelser ryddet op: {Count} ældre end 12 måneder slettet.", old.Count);
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Oprydning af henvendelser fejlede – prøver igen senere.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}
