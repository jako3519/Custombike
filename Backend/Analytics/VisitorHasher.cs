using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Analytics;

// Anonym besoegs-id: SHA-256(dagens salt + IP + browser). Salten er tilfaeldig, ligger kun paa serveren
// (i databasen, saa den overlever genstart) og skiftes hver dag kl. 00 dansk tid; gamle salte slettes.
// Derfor kan samme besoegende ikke genkendes fra dag til dag, og hashen kan ikke foeres tilbage til en IP.
public sealed class VisitorHasher
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateOnly _day;
    private byte[]? _salt;

    public async Task<string> HashAsync(DataContext db, string ip, string userAgent, DateTime utcNow, CancellationToken ct)
    {
        var salt = await GetSaltAsync(db, DanishTime.Today(utcNow), ct);
        var input = Encoding.UTF8.GetBytes(ip + "\n" + userAgent);
        var data = new byte[salt.Length + input.Length];
        salt.CopyTo(data, 0);
        input.CopyTo(data, salt.Length);
        return Convert.ToHexStringLower(SHA256.HashData(data));
    }

    private async Task<byte[]> GetSaltAsync(DataContext db, DateOnly day, CancellationToken ct)
    {
        if (_salt != null && _day == day) return _salt;
        await _lock.WaitAsync(ct);
        try
        {
            if (_salt != null && _day == day) return _salt;

            var salt = await db.AnalyticsSalts.AsNoTracking()
                .Where(s => s.Day == day).Select(s => s.Salt).FirstOrDefaultAsync(ct);
            if (salt == null)
            {
                // Ny dag: lav en ny salt og slet de gamle. ON CONFLICT goer det sikkert, hvis to
                // processer naar det samtidig - begge ender med at bruge den samme raekke.
                var fresh = RandomNumberGenerator.GetBytes(32);
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"""INSERT INTO "AnalyticsSalts" ("Day", "Salt") VALUES ({day}, {fresh}) ON CONFLICT ("Day") DO NOTHING""", ct);
                await db.AnalyticsSalts.Where(s => s.Day < day).ExecuteDeleteAsync(ct);
                salt = await db.AnalyticsSalts.AsNoTracking()
                    .Where(s => s.Day == day).Select(s => s.Salt).FirstAsync(ct);
            }
            _day = day;
            _salt = salt;
            return salt;
        }
        finally
        {
            _lock.Release();
        }
    }
}
