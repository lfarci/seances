using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;

namespace Seances.Data.Collection.Ugc.Parsing;

public static class UgcFilmParser
{
    private static readonly Uri OfficialSite = new(UgcSourceConfiguration.BaseUrl);

    internal static UgcFilmDto Parse(IElement? element, UgcFilmDto film)
    {
        if (element is null)
        {
            return film;
        }
        var originalTitle = FindOriginalTitle(element);
        var link = element.QuerySelector(".block--title a[href], .title-film a[href], a.film-title[href], .film-title a[href], h2 a[href], h3 a[href], a[href*='film.html?id='], a[href*='film-']");
        var detailUrl = ParseDetailUrl(link?.GetAttribute("href"));
        return film.MergeMissing(new UgcFilmDto(film.Id, film.Title, originalTitle, detailUrl));
    }

    public static UgcFilmDto ParseDetails(string html, UgcFilmDto film)
    {
        var document = new HtmlParser().ParseDocument(html);
        var originalTitle = FindOriginalTitle(document.DocumentElement);
        var details = UgcFilmJsonLdParser.Parse(document);
        return film.MergeMissing(new UgcFilmDto(film.Id, film.Title, originalTitle, Details: details));
    }

    private static IEnumerable<IElement> Candidates(IElement element, string selector)
    {
        if (element.Matches(selector))
        {
            yield return element;
        }
        foreach (var candidate in element.QuerySelectorAll(selector))
        {
            yield return candidate;
        }
    }

    private static string? FindOriginalTitle(IElement element)
    {
        foreach (var candidate in Candidates(element, "[data-original-title], .original-title, .film-original-title, [itemprop='originalTitle']"))
        {
            var value = Clean(candidate.GetAttribute("data-original-title") ?? candidate.GetAttribute("content") ?? candidate.TextContent);
            value = Regex.Replace(value, @"^(?:titre\s+(?:original|d['’]origine)|original\s+title)\s*:?\s*", "", RegexOptions.IgnoreCase);
            if (value.Length > 0)
            {
                return value;
            }
        }
        foreach (var candidate in Candidates(element, "dt, li, p, strong, b, span, label"))
        {
            var match = Regex.Match(Clean(candidate.TextContent), @"^(?:titre\s+(?:original|d['’]origine)|original\s+title)\s*:\s*(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return Clean(match.Groups[1].Value);
            }
            if (!Regex.IsMatch(Clean(candidate.TextContent), @"^(?:titre\s+(?:original|d['’]origine)|original\s+title)\s*:?$", RegexOptions.IgnoreCase))
            {
                continue;
            }
            if (candidate.NextSibling is IText text && Clean(text.TextContent).TrimStart(':', ' ') is { Length: > 0 } siblingTitle)
            {
                return siblingTitle;
            }
            var next = candidate.NextElementSibling;
            if (next is not null && ((candidate.LocalName == "dt" && next.LocalName == "dd") ||
                (candidate.LocalName is "strong" or "b" or "label" && next.LocalName is "span" or "a")))
            {
                var value = Clean(next.TextContent);
                if (value.Length > 0)
                {
                    return value;
                }
            }
        }
        return null;
    }

    private static Uri? ParseDetailUrl(string? href) =>
        Uri.TryCreate(OfficialSite, href, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
        uri.Host.Equals(OfficialSite.Host, StringComparison.OrdinalIgnoreCase) && uri.IsDefaultPort &&
        string.IsNullOrEmpty(uri.UserInfo) && uri.AbsolutePath != "/" ? uri : null;

    private static string Clean(string value) => Regex.Replace(value, @"\s+", " ").Trim();
}
