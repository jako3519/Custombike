using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using Backend.Content;
using Backend.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Backend.Contact;

// Sender en kopi af henvendelsen via SMTP (Gmail: smtp.gmail.com:587 STARTTLS).
// Mangler værtsoplysninger, login eller modtager, eller fejler sendingen, logges det
// bare – henvendelsen er allerede gemt, og API'et svarer stadig ok.
public sealed class ContactEmailSender(IConfiguration config, IWebHostEnvironment env, ILogger<ContactEmailSender> logger)
{
    // Gmail afviser hele beskeden over 25 MB. Base64 øger størrelsen, så de rå filer holdes under 18 MB.
    public const long MaxRawAttachmentBytes = 18L * 1024 * 1024;
    private const string FromName = "Henvendelse – Custombike";

    public async Task TrySendAsync(ContactRequest request, CancellationToken ct)
    {
        var host = First(config["SMTP_HOST"], config["Smtp:Host"]) ?? "smtp.gmail.com";
        var port = ParsePort(First(config["SMTP_PORT"], config["Smtp:Port"]), 587);
        var username = First(config["SMTP_USERNAME"], config["Smtp:Username"]);
        // Gmail-app-adgangskoder er 16 tegn uden mellemrum; brugere skriver dem ofte med mellemrum.
        var password = StripSpaces(First(config["SMTP_PASSWORD"], config["Smtp:Password"]));
        var to = First(config["CONTACT_TO_EMAIL"], config["Contact:ToEmail"]);
        var from = First(config["CONTACT_FROM_EMAIL"], config["Contact:FromEmail"], username);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(to))
        {
            logger.LogInformation("Kontakt-e-mail er ikke sat op (SMTP_USERNAME / SMTP_PASSWORD / CONTACT_TO_EMAIL mangler) – henvendelse {Id} er gemt uden at sende mail.", request.Id);
            return;
        }

        try
        {
            var service = string.IsNullOrWhiteSpace(request.ServiceTitle) ? request.ServiceKey : request.ServiceTitle;
            var subject = $"Henvendelse fra {request.Name} – {service}";
            var (html, text, attach) = BuildBody(request, service);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(FromName, from!));
            message.To.Add(MailboxAddress.Parse(to));
            if (!string.IsNullOrWhiteSpace(request.Email) && LooksLikeEmail(request.Email))
                message.ReplyTo.Add(MailboxAddress.Parse(request.Email));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = html, TextBody = text };
            if (attach)
            {
                var folder = MediaFiles.ContactFolder(env);
                foreach (var file in request.Attachments)
                {
                    var path = Path.Combine(folder, Path.GetFileName(file.FileName));
                    if (!File.Exists(path)) continue;
                    var type = string.IsNullOrWhiteSpace(file.ContentType)
                        ? new ContentType("application", "octet-stream")
                        : ContentType.Parse(file.ContentType);
                    var name = string.IsNullOrWhiteSpace(file.OriginalName) ? file.FileName : Path.GetFileName(file.OriginalName);
                    var bytes = await File.ReadAllBytesAsync(path, ct);
                    builder.Attachments.Add(name, bytes, type);
                }
            }
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            logger.LogInformation("Kontakt-e-mail sendt for henvendelse {Id} via SMTP {Host}:{Port} til {To}.", request.Id, host, port, to);
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning(ex, "SMTP-login blev afvist for henvendelse {Id} ({Host}:{Port}) – henvendelsen er gemt alligevel.", request.Id, host, port);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log aldrig adgangskoden; MailKit putter den typisk ikke i beskeden.
            logger.LogWarning(ex, "Kunne ikke sende kontakt-e-mail for henvendelse {Id} via SMTP {Host}:{Port} ({Type}: {Message}) – henvendelsen er gemt alligevel.", request.Id, host, port, ex.GetType().Name, ex.Message);
        }
    }

    private (string Html, string Text, bool AttachFiles) BuildBody(ContactRequest request, string service)
    {
        var e = HtmlEncoder.Default;
        var site = (First(config["Frontend:Url"], config["Site:Url"]) ?? "").TrimEnd('/');
        var adminUrl = site + "/admin";
        var telHref = TelHref(request.Phone);
        var rawSize = request.Attachments.Sum(a => a.Size);
        var hasFiles = request.Attachments.Count > 0;
        var attach = hasFiles && rawSize <= MaxRawAttachmentBytes;

        string filesHtml;
        string filesText;
        if (!hasFiles)
        {
            filesHtml = "<p>Ingen billeder vedhæftet.</p>";
            filesText = "Ingen billeder vedhæftet.";
        }
        else if (attach)
        {
            var n = request.Attachments.Count;
            filesHtml = $"<p>{n} billede(r) er vedhæftet denne mail. De ligger også under Henvendelser, når du er logget ind.</p>";
            filesText = $"{n} billede(r) er vedhæftet denne mail. De ligger også under Henvendelser, når du er logget ind.";
        }
        else
        {
            filesHtml = "<p>Billederne er for store til at vedhæfte (Gmail tillader højst 25 MB i en mail). De ligger under Henvendelser, når du er logget ind.</p>";
            filesText = "Billederne er for store til at vedhæfte (Gmail tillader højst 25 MB i en mail). De ligger under Henvendelser, når du er logget ind.";
        }

        var emailHtml = string.IsNullOrWhiteSpace(request.Email)
            ? "Ikke oplyst"
            : $"<a href=\"mailto:{e.Encode(request.Email)}\">{e.Encode(request.Email)}</a>";
        var emailText = string.IsNullOrWhiteSpace(request.Email) ? "Ikke oplyst" : request.Email;
        var phoneHtml = string.IsNullOrWhiteSpace(telHref)
            ? e.Encode(request.Phone)
            : $"<a href=\"{e.Encode(telHref)}\">{e.Encode(request.Phone)}</a>";

        var html = $"""
            <p>Ny henvendelse fra kontaktformularen.</p>
            <p><strong>Navn:</strong> {e.Encode(request.Name)}<br/>
            <strong>Telefon:</strong> {phoneHtml}<br/>
            <strong>E-mail:</strong> {emailHtml}<br/>
            <strong>Ydelse:</strong> {e.Encode(service)}</p>
            <p><strong>Besked:</strong><br/>{e.Encode(request.Message).Replace("\n", "<br/>")}</p>
            {filesHtml}
            <p>Se og besvar henvendelsen under <strong>Henvendelser</strong>, når du er logget ind:<br/>
            <a href="{e.Encode(adminUrl)}">{e.Encode(adminUrl)}</a></p>
            """;

        var text = new StringBuilder()
            .AppendLine("Ny henvendelse fra kontaktformularen.")
            .AppendLine()
            .AppendLine($"Navn: {request.Name}")
            .AppendLine($"Telefon: {request.Phone}")
            .AppendLine($"E-mail: {emailText}")
            .AppendLine($"Ydelse: {service}")
            .AppendLine()
            .AppendLine("Besked:")
            .AppendLine(request.Message)
            .AppendLine()
            .AppendLine(filesText)
            .AppendLine()
            .AppendLine("Se og besvar henvendelsen under Henvendelser, når du er logget ind:")
            .Append(adminUrl)
            .ToString();

        return (html, text, attach);
    }

    private static string? TelHref(string phone)
    {
        var sb = new StringBuilder();
        foreach (var c in phone)
        {
            if (char.IsDigit(c) || (c == '+' && sb.Length == 0)) sb.Append(c);
        }
        return sb.Length >= 8 ? "tel:" + sb : null;
    }

    private static int ParsePort(string? value, int fallback)
        => int.TryParse(value, out var port) && port is > 0 and <= 65535 ? port : fallback;

    private static bool LooksLikeEmail(string email)
    {
        try { _ = new MailAddress(email); }
        catch (FormatException) { return false; }
        return email.Contains('.');
    }

    private static string? StripSpaces(string? value)
        => string.IsNullOrEmpty(value) ? value : string.Concat(value.Where(c => !char.IsWhiteSpace(c)));

    private static string? First(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }
}
