namespace Backend.Models;

// En anonym haendelse i den indbyggede besoegsstatistik (ingen cookies, ingen IP-adresser).
public class AnalyticsEvent
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }        // UTC
    public string Type { get; set; } = "";         // "pageview" | "click"
    public string Path { get; set; } = "";         // kun stien, ingen query
    public string? Target { get; set; }            // klik: "phone","email","contact","facebook","service:<key>","gallery:<id>"
    public string VisitorHash { get; set; } = "";  // SHA-256(salt + IP + UA); salten skifter hver dag, IP gemmes aldrig
    public string? Referrer { get; set; }          // kun domaenet, null = direkte/intern
    public string Device { get; set; } = "";       // "mobile" | "desktop"
    public string? Country { get; set; }           // ISO-landekode, fx "DK"
    public string? City { get; set; }
}
