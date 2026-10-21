using System.Globalization;
using System.Text;

namespace Seances.Data.Collection.Tmdb;

/// <summary>Normalizes titles and names so UGC and TMDB spellings can be compared and searched.</summary>
internal static class TmdbText
{
    private static readonly string[] FormatSuffixes = ["3D", "IMAX", "4DX", "VOST", "VOSTFR", "VF", "VO"];

    public static string SearchTitle(string title)
    {
        var withoutBrackets = System.Text.RegularExpressions.Regex.Replace(title, @"\s*[\(\[].*?[\)\]]\s*", " ");
        var dash = withoutBrackets.IndexOf(" - ", StringComparison.Ordinal);
        var main = dash > 0 ? withoutBrackets[..dash] : withoutBrackets;
        var words = main.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (words.Count > 1 && FormatSuffixes.Contains(words[^1], StringComparer.OrdinalIgnoreCase))
        {
            words.RemoveAt(words.Count - 1);
        }
        return string.Join(' ', words);
    }

    public static string Comparable(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && builder.Length > 0 &&
                builder[^1] != ' ')
            {
                builder.Append(' ');
            }
        }
        return builder.ToString().Trim();
    }
}
