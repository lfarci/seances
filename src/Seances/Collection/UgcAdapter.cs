using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Seances.Domain;

namespace Seances.Collection;

public sealed class UgcAdapter(HttpClient http, TimeProvider clock)
{
    private const string FragmentEndpoint = "https://www.ugc.be/showingsCinemaAjaxAction!getShowingsForCinemaPage.action";
    private const string PageParameter = "30007";
    private static readonly (string Id, string Name, string Address)[] Cinemas =
    [
        ("2", "UGC De Brouckère", "Place De Brouckère 38, 1000 Bruxelles"),
        ("1", "UGC Toison d’Or", "Avenue de la Toison d’Or 8 et 17, 1050 Bruxelles")
    ];
    private DateTimeOffset? _lastRequestAt;

    public async Task<IReadOnlyList<CollectedDate>> CollectAsync(CancellationToken cancellationToken, DateOnly? requestedDate = null)
    {
        var results = new List<CollectedDate>();
        var indexHtml = await GetAsync("https://www.ugc.be/cinemas.html", cancellationToken);
        var index = new HtmlParser().ParseDocument(indexHtml);
        foreach (var cinema in Cinemas)
        {
            var place = cinema.Id == "2" ? "brouck" : "toison";
            var cinemaLink = index.QuerySelectorAll("a[href]").FirstOrDefault(link =>
                (link.TextContent + " " + link.GetAttribute("href")).Contains(place, StringComparison.OrdinalIgnoreCase));
            if (cinemaLink?.GetAttribute("href") is not { } href) throw new InvalidDataException($"Could not discover the UGC page for {cinema.Name} from the cinema index.");
            var pageUrl = new Uri(new Uri("https://www.ugc.be/cinemas.html"), href).ToString();
            var page = await GetAsync(pageUrl, cancellationToken);
            var dates = FindDates(page);
            if (dates.Count == 0) throw new InvalidDataException($"No published dates found for {cinema.Name}.");
            if (requestedDate is { } selected && !dates.Contains(selected))
                throw new InvalidDataException($"{cinema.Name} has not published service date {selected:yyyy-MM-dd}.");
            IEnumerable<DateOnly> requestedDates = requestedDate is { } requested ? new[] { requested } : dates;
            foreach (var date in requestedDates)
            {
                var url = $"{FragmentEndpoint}?cinemaId={Uri.EscapeDataString(cinema.Id)}&date={date:yyyy-MM-dd}&page={PageParameter}";
                var html = await GetAsync(url, cancellationToken);
                var fetchedAt = clock.GetUtcNow();
                var parsed = ParseFragment(html, new SourceFetchContext(cinema.Id, date, url, fetchedAt));
                results.Add(new CollectedDate(cinema.Id, cinema.Name, cinema.Address, date, url, fetchedAt, parsed.Screenings, parsed.Films));
            }
        }
        return results;
    }

    public ParsedFragment ParseFragment(string html, SourceFetchContext context)
    {
        var document = new HtmlParser().ParseDocument(html);
        if (document.QuerySelectorAll("button[data-showing]").Length == 0)
        {
            var content = document.Body?.TextContent.Trim() ?? string.Empty;
            var explicitlyEmpty = document.QuerySelector(".no-showings, .no-screenings, [data-empty='true']") is not null ||
                content.Contains("aucune séance", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("aucune seance", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("no screenings", StringComparison.OrdinalIgnoreCase);
            if (!explicitlyEmpty) throw new InvalidDataException("UGC returned an empty or unrecognized screening fragment.");
        }

        var parsed = new Dictionary<string, Screening>(StringComparer.Ordinal);
        var films = new Dictionary<string, Film>(StringComparer.Ordinal);
        foreach (var button in document.QuerySelectorAll("button[data-showing]"))
        {
            var screeningId = Attribute(button, "data-showing", "data-showing-id", "data-seance-id");
            var filmId = Attribute(button, "data-film", "data-film-id", "data-id-film");
            var title = Attribute(button, "data-title", "data-film-title") ??
                button.Closest("[class*='bloc-showing-film']")?.QuerySelector(".title-film, .film-title, h2, h3")?.TextContent.Trim();
            var cinemaSourceId = Attribute(button, "data-cinema", "data-cinema-id");
            var dateValue = Attribute(button, "data-date");
            var timeValue = Attribute(button, "data-time", "data-start", "data-start-time");
            var version = Attribute(button, "data-version", "data-language", "data-label") ??
                button.QuerySelector(".screening-version, .screening-language, [class*='version']")?.TextContent;
            if (string.IsNullOrWhiteSpace(screeningId) || string.IsNullOrWhiteSpace(filmId) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(cinemaSourceId) || string.IsNullOrWhiteSpace(dateValue) || string.IsNullOrWhiteSpace(timeValue) || string.IsNullOrWhiteSpace(version))
                throw new InvalidDataException("A screening button is missing a required source field.");
            if (!cinemaSourceId.Equals(context.CinemaId, StringComparison.Ordinal)) throw new InvalidDataException($"Screening {screeningId} belongs to unexpected cinema {cinemaSourceId}.");
            if (!DateOnly.TryParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var serviceDate) || serviceDate != context.ServiceDate)
                throw new InvalidDataException($"Invalid or unexpected service date '{dateValue}' for screening {screeningId}.");
            if (!TimeOnly.TryParse(timeValue, CultureInfo.InvariantCulture, out var startTime))
                throw new InvalidDataException($"Invalid start time '{timeValue}' for screening {screeningId}.");
            var start = ResolveLocal(serviceDate, startTime);
            var block = button.Closest("[class*='bloc-showing-film']");
            var runtime = ParseRuntime(block);
            films.TryAdd(filmId, new Film(filmId, title, runtime, runtime is null ? "unavailable from source" : "UGC film block"));
            var finishText = button.QuerySelector(".screening-time-end")?.TextContent.Trim();
            var finish = finishText is null ? null : ResolveFinish(serviceDate, startTime, finishText);
            var room = button.QuerySelector(".screening-room")?.TextContent.Trim();
            var format = Attribute(button, "data-format") ?? button.QuerySelector(".screening-format")?.TextContent.Trim();
            var sourceStatus = Attribute(button, "data-status") ?? button.QuerySelector(".screening-status")?.TextContent;
            var bookingHref = Attribute(button, "data-booking-url", "data-reservation-url", "data-url") ??
                button.QuerySelector("a[href*='reservationSeances']")?.GetAttribute("href") ?? button.GetAttribute("href") ?? button.GetAttribute("onclick");
            var bookingUrl = ParseBookingUrl(bookingHref);
            var screening = new Screening(screeningId, cinemaSourceId, filmId, serviceDate, start, finish,
                string.IsNullOrWhiteSpace(room) ? null : room, version, string.IsNullOrWhiteSpace(format) ? null : format,
                bookingUrl, context.SourceUrl, context.FetchedAt, string.IsNullOrWhiteSpace(sourceStatus) ? null : sourceStatus);
            parsed.TryAdd(screeningId, screening);
        }
        return new ParsedFragment(parsed.Values.ToArray(), films.Values.ToArray());
    }

    private static int? ParseRuntime(IElement? filmBlock)
    {
        if (filmBlock is null) return null;
        var runtimeElement = filmBlock.QuerySelector(".runtime, .film-runtime, .duration, .film-duration, [class*='duree'], [data-runtime]");
        var text = runtimeElement?.GetAttribute("data-runtime") ?? runtimeElement?.TextContent;
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = Regex.Match(text, @"(?:(\d+)\s*h\s*(\d{1,2})?|(\d+)\s*min)", RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        if (match.Groups[3].Success) return int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 60 +
            (match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : 0);
    }

    private static List<DateOnly> FindDates(string html)
    {
        var values = Regex.Matches(html, @"nav_date_(\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase)
            .Select(match => match.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        foreach (Match call in Regex.Matches(html, @"initCalendarDate\s*\((?<args>[^)]*)\)", RegexOptions.Singleline))
            foreach (Match date in Regex.Matches(call.Groups["args"].Value, @"\d{4}-\d{2}-\d{2}")) values.Add(date.Value);
        return values.Select(value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture)).Order().ToList();
    }

    private static DateTimeOffset ResolveLocal(DateOnly date, TimeOnly time)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
            throw new InvalidDataException($"Source local time {local:O} is ambiguous or invalid in Europe/Brussels.");
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    private static DateTimeOffset? ResolveFinish(DateOnly date, TimeOnly start, string finishText)
    {
        if (!TimeOnly.TryParse(finishText, CultureInfo.InvariantCulture, out var finish))
            throw new InvalidDataException($"Invalid source finish time '{finishText}'.");
        var finishDate = finish < start ? date.AddDays(1) : date;
        return ResolveLocal(finishDate, finish);
    }

    private static string? Attribute(IElement element, params string[] names) => names.Select(element.GetAttribute).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static Uri? ParseBookingUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = Regex.Match(value, """(?:https?://[^'"\s]+)?/reservationSeances\.html\?id=\d+""");
        if (!match.Success) return null;
        var absolute = match.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? match.Value : "https://www.ugc.be" + match.Value;
        return Uri.TryCreate(absolute, UriKind.Absolute, out var uri) ? uri : null;
    }

    private async Task<string> GetAsync(string url, CancellationToken cancellationToken)
    {
        Exception? lastError = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (_lastRequestAt is { } last)
                {
                    var wait = TimeSpan.FromSeconds(1) - (clock.GetUtcNow() - last);
                    if (wait > TimeSpan.Zero) await Task.Delay(wait, cancellationToken);
                }
                _lastRequestAt = clock.GetUtcNow();
                using var response = await http.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content)) throw new InvalidDataException($"UGC returned an empty response for {url}.");
                return content;
            }
            catch (HttpRequestException error) { lastError = error; }
            catch (TaskCanceledException error) when (!cancellationToken.IsCancellationRequested) { lastError = error; }
            if (attempt < 2) await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
        }
        throw new HttpRequestException($"UGC request failed after bounded retries: {url}", lastError);
    }
}

public sealed record CollectedDate(string CinemaId, string CinemaName, string Address, DateOnly Date, string SourceUrl, DateTimeOffset FetchedAt, IReadOnlyList<Screening> Screenings, IReadOnlyList<Film> Films);
public sealed record ParsedFragment(IReadOnlyList<Screening> Screenings, IReadOnlyList<Film> Films);
public sealed record SourceFetchContext(string CinemaId, DateOnly ServiceDate, string SourceUrl, DateTimeOffset FetchedAt);
