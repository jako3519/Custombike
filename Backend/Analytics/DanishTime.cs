namespace Backend.Analytics;

// Dansk tid (Europe/Copenhagen) til "i dag", "denne uge" osv. og til den daglige salt.
public static class DanishTime
{
    public static readonly TimeZoneInfo Zone = FindZone();

    private static TimeZoneInfo FindZone()
    {
        foreach (var id in new[] { "Europe/Copenhagen", "Romance Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        // Reserve hvis styresystemet ikke har tidszonedata: CET/CEST efter EU's regler
        var start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday);
        var end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday);
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1), start, end);
        return TimeZoneInfo.CreateCustomTimeZone("CET-CEST", TimeSpan.FromHours(1), "Danmark", "CET", "CEST", [rule]);
    }

    public static DateTime Now(DateTime utcNow) => TimeZoneInfo.ConvertTimeFromUtc(utcNow, Zone);

    public static DateOnly Today(DateTime utcNow) => DateOnly.FromDateTime(Now(utcNow));

    // Midnat (dansk tid) paa en given dato, som UTC
    public static DateTime MidnightUtc(DateOnly day) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Zone);
}
