namespace Backend.Security;

// De ENESTE konti der må logge ind og redigere siden.
// Ligger bevidst i koden, så listen ikke kan ændres via .env eller appsettings.
public static class AdminWhitelist
{
    private static readonly HashSet<string> Emails = new(StringComparer.OrdinalIgnoreCase)
    {
        "jako3519@gmail.com",
    };

    public static bool Contains(string? email) =>
        !string.IsNullOrWhiteSpace(email) && Emails.Contains(email.Trim());
}
