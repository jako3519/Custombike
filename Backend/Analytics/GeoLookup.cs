using System.Net;
using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Backend.Analytics;

// Land og by ud fra IP-adressen, slaaet op i en LOKAL database (DB-IP "IP to City Lite", CC BY 4.0).
// Intet sendes til tredjepart, og IP-adressen bliver aldrig gemt eller logget.
// Filen hentes ved docker build (se Dockerfile og Backend/GeoData/LAES-MIG.md).
public sealed class GeoLookup : IDisposable
{
    private readonly DatabaseReader? _reader;

    public GeoLookup(IConfiguration config, IWebHostEnvironment env, ILogger<GeoLookup> logger)
    {
        var path = config["Analytics:GeoDbPath"] ?? Path.Combine(env.ContentRootPath, "geo", "dbip-city-lite.mmdb");
        if (!File.Exists(path))
        {
            logger.LogWarning("Geo-database ikke fundet ({Path}) - statistikken gemmes uden land/by.", path);
            return;
        }
        try
        {
            // Memory-mapped: filen laeses fra disk efter behov i stedet for at fylde RAM
            _reader = new DatabaseReader(path, FileAccessMode.MemoryMapped);
            logger.LogInformation("Geo-database indlaest: {Type} ({Date:yyyy-MM-dd})",
                _reader.Metadata.DatabaseType, _reader.Metadata.BuildDate);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Kunne ikke aabne geo-databasen: {Error}", ex.GetType().Name);
        }
    }

    public bool Available => _reader != null;

    public (string? Country, string? City) Lookup(string? ip)
    {
        if (_reader == null || string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out var address))
            return (null, null);
        try
        {
            if (_reader.TryCity(address, out var r) && r != null)
            {
                var country = r.Country.IsoCode;
                var city = r.City.Name;
                return (
                    string.IsNullOrWhiteSpace(country) ? null : country.ToUpperInvariant()[..Math.Min(2, country.Length)],
                    string.IsNullOrWhiteSpace(city) ? null : city.Length > 100 ? city[..100] : city);
            }
        }
        catch
        {
            // Ukendt/ugyldig adresse: ingen geo. (Undtagelsens tekst kan indeholde IP'en, saa den logges ikke.)
        }
        return (null, null);
    }

    public void Dispose() => _reader?.Dispose();
}
