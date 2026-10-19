using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;

namespace Seances.Data.Collection.Ugc.Parsing;

internal static class UgcCinemaPageParser
{
    public static Uri FindCinemaPage(string html, UgcCinema cinema)
    {
        var document = new HtmlParser().ParseDocument(html);
        var link = document.QuerySelectorAll("a[href]").FirstOrDefault(candidate =>
            (candidate.TextContent + " " + candidate.GetAttribute("href"))
            .Contains(cinema.PageName, StringComparison.OrdinalIgnoreCase));
        if (link?.GetAttribute("href") is not { } path)
        {
            throw new InvalidDataException($"Could not discover the UGC page for {cinema.Name} from the cinema list.");
        }
        return UgcSourceConfiguration.CinemaPageUrl(path);
    }

    public static IReadOnlyList<DateOnly> FindPublishedDates(string html)
    {
        var dates = Regex.Matches(html, UgcSourceConfiguration.CalendarDatePattern, RegexOptions.IgnoreCase)
            .Select(match => match.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        foreach (Match calendar in Regex.Matches(html, UgcSourceConfiguration.CalendarFunctionPattern,
                     RegexOptions.Singleline))
        {
            foreach (Match date in Regex.Matches(calendar.Groups["args"].Value, UgcSourceConfiguration.IsoDatePattern))
            {
                dates.Add(date.Value);
            }
        }

        return dates.Select(value => DateOnly.ParseExact(value, UgcSourceConfiguration.ServiceDateFormat,
            CultureInfo.InvariantCulture)).Order().ToArray();
    }
}
