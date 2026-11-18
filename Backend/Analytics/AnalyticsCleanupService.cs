using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Analytics;

// Sletter statistik der er mere end 2 aar gammel og salte fra tidligere dage. Koerer kort efter
// opstart og derefter hver 12. time.
public sealed class AnalyticsCleanupService(IServiceScopeFactory scopes, ILogger<AnalyticsCleanupService> logger) : BackgroundService
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
                var now = DateTime.UtcNow;
                var cutoff = now.AddYears(-2);
                var events = await db.AnalyticsEvents.Where(e => e.CreatedAt < cutoff).ExecuteDeleteAsync(stoppingToken);
                var today = DanishTime.Today(now);
                var salts = await db.AnalyticsSalts.Where(s => s.Day < today).ExecuteDeleteAsync(stoppingToken);
                if (events > 0 || salts > 0)
                    logger.LogInformation("Statistik ryddet op: {Events} gamle haendelser, {Salts} gamle salte slettet.", events, salts);
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Oprydning af statistik fejlede - proever igen senere.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}
