using System.Globalization;
using System.Text;

namespace Backend.Content;

// Laver en pæn, dansk-venlig adresse ud fra en titel: "Vinduer og døre" -> "vinduer-og-doere"
public static class Slug
{
    public const int MaxLength = 80;

    public static string From(string? input)
    {
        var s = (input ?? "").Trim().ToLowerInvariant()
            .Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa")
            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");

        // Øvrige accenter fjernes (é -> e)
        s = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > MaxLength) slug = slug[..MaxLength].TrimEnd('-');
        return slug;
    }

    public static bool IsValid(string slug) =>
        slug.Length is > 0 and <= MaxLength && slug == From(slug);
}
