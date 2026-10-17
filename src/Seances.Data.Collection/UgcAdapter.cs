using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Seances.Data.Collection;

public sealed class UgcAdapter(HttpClient http, TimeProvider clock)
{
    private const string DeBrouckereCinemaId = "2";
    private const string ToisonDorCinemaId = "1";
    private const string OfficialSiteBaseUrl = "https://www.ugc.be";
    private const string BrusselsRegionId = "1";
    private const string CinemaListUrl = OfficialSiteBaseUrl + "/cinemasAjaxAction!getCinemasList.action?id=" + BrusselsRegionId + "&latitude=&longitude=";
    private const string FragmentEndpoint = OfficialSiteBaseUrl + "/showingsCinemaAjaxAction!getShowingsForCinemaPage.action";
    private const string PageParameter = "30007";
    private const string SourceTimeZoneId = "Europe/Brussels";
    private const int RequestIntervalSeconds = 1;
    private const int MaximumRequestAttempts = 3;
    private const int InitialRetryDelaySeconds = 1;
    private static readonly (string Id, string Name, string Address)[] Cinemas =
    [
        (DeBrouckereCinemaId, "UGC De Brouckère", "Place De Brouckère 38, 1000 Bruxelles"),
        (ToisonDorCinemaId, "UGC Toison d’Or", "Avenue de la Toison d’Or 8 et 17, 1050 Bruxelles")
    ];
    private DateTimeOffset? _lastRequestAt;

    public async Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(CancellationToken cancellationToken, DateOnly? requestedDate = null)
    {
        var results = new List<UgcScheduleDateDto>();
        var cinemaListHtml = await GetAsync(CinemaListUrl, cancellationToken);
        var cinemaList = new HtmlParser().ParseDocument(cinemaListHtml);
        foreach (var cinema in Cinemas)
        {
            var place = cinema.Id == DeBrouckereCinemaId ? "brouck" : "toison";
            var cinemaLink = cinemaList.QuerySelectorAll("a[href]").FirstOrDefault(link =>
                (link.TextContent + " " + link.GetAttribute("href")).Contains(place, StringComparison.OrdinalIgnoreCase));
            if (cinemaLink?.GetAttribute("href") is not { } href) throw new InvalidDataException($"Could not discover the UGC page for {cinema.Name} from the cinema list.");
            var pageUrl = new Uri(new Uri(OfficialSiteBaseUrl), href).ToString();
            var page = await GetAsync(pageUrl, cancellationToken);
            var dates = FindDates(page);
            if (dates.Count == 0) throw new InvalidDataException($"No published dates found for {cinema.Name}.");
            if (requestedDate is { } selected && !dates.Contains(selected))
                throw new InvalidDataException($"{cinema.Name} has not published service date {selected:yyyy-MM-dd}. " +
                    $"Published dates: {string.Join(", ", dates.Select(date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))}.");
            IEnumerable<DateOnly> requestedDates = requestedDate is { } requested ? new[] { requested } : dates;
            foreach (var date in requestedDates)
            {
                var url = $"{FragmentEndpoint}?cinemaId={Uri.EscapeDataString(cinema.Id)}&date={date:yyyy-MM-dd}&page={PageParameter}";
                var html = await GetAsync(url, cancellationToken);
                var fetchedAt = clock.GetUtcNow();
                var parsed = ParseFragment(html, new UgcFetchContext(cinema.Id, date, url, fetchedAt));
                results.Add(new UgcScheduleDateDto(cinema.Id, cinema.Name, cinema.Address, date, url, fetchedAt, parsed.Films, parsed.Screenings));
            }
        }
        return results;
    }

    public UgcParsedFragmentDto ParseFragment(string html, UgcFetchContext context)
    {
        var document = new HtmlParser().ParseDocument(html);
        if (document.QuerySelectorAll("button[data-showing]").Length == 0)
        {
            var content = document.Body?.TextContent.Trim() ?? string.Empty;
            var explicitlyEmpty = document.QuerySelector(".no-showings, .no-screenings, [data-empty='true']") is not null ||
                content.Contains("aucune séance", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("aucune seance", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("no screenings", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("prochaine séance le", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("prochaine seance le", StringComparison.OrdinalIgnoreCase);
            if (!explicitlyEmpty) throw new InvalidDataException("UGC returned an empty or unrecognized screening fragment.");
        }

        var parsed = new Dictionary<string, UgcScreeningDto>(StringComparer.Ordinal);
        var films = new Dictionary<string, UgcFilmDto>(StringComparer.Ordinal);
        foreach (var button in document.QuerySelectorAll("button[data-showing]"))
        {
            var screeningId = Attribute(button, "data-showing", "data-showing-id", "data-seance-id");
            var sourceFilmId = Attribute(button, "data-film-id", "data-filmid", "data-id-film");
            var filmId = sourceFilmId ?? Attribute(button, "data-film");
            var filmBlock = button.Closest("[id*='bloc-showing-film'], [class*='bloc-showing-film']");
            var title = Attribute(button, "data-title", "data-film-title") ??
                filmBlock?.QuerySelector(".title-film, .film-title, .block--title a, h2, h3")?.TextContent.Trim() ??
                (sourceFilmId is null ? null : Attribute(button, "data-film"));
            var cinemaSourceId = Attribute(button, "data-cinema-id", "data-cinemaid", "data-id-cinema") ?? Attribute(button, "data-cinema");
            var dateValue = Attribute(button, "data-date", "data-seancedate");
            var timeValue = Attribute(button, "data-time", "data-start", "data-start-time", "data-seancehour") ??
                button.QuerySelector(".screening-time-start")?.TextContent.Trim();
            var version = Attribute(button, "data-version", "data-language", "data-label") ??
                button.QuerySelector(".screening-version, .screening-language, [class*='version']")?.TextContent;
            if (string.IsNullOrWhiteSpace(screeningId) || string.IsNullOrWhiteSpace(filmId) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(cinemaSourceId) || string.IsNullOrWhiteSpace(dateValue) || string.IsNullOrWhiteSpace(timeValue) || string.IsNullOrWhiteSpace(version))
                throw new InvalidDataException("A screening button is missing a required source field.");
            if (!cinemaSourceId.Equals(context.CinemaId, StringComparison.Ordinal)) throw new InvalidDataException($"Screening {screeningId} belongs to unexpected cinema {cinemaSourceId}.");
            if (!TryParseServiceDate(dateValue, out var serviceDate) || serviceDate != context.ServiceDate)
                throw new InvalidDataException($"Invalid or unexpected service date '{dateValue}' for screening {screeningId}.");
            if (!TimeOnly.TryParse(timeValue, CultureInfo.InvariantCulture, out var startTime))
                throw new InvalidDataException($"Invalid start time '{timeValue}' for screening {screeningId}.");
            var start = ResolveLocal(serviceDate, startTime);
            var runtime = ParseRuntime(filmBlock);
            films.TryAdd(filmId, new UgcFilmDto(filmId, title, runtime, runtime is null ? "unavailable from source" : "UGC film block"));
            var finishText = button.QuerySelector(".screening-time-end")?.TextContent.Trim();
            var finish = finishText is null ? null : ResolveFinish(serviceDate, startTime, finishText);
            var room = button.QuerySelector(".screening-room")?.TextContent.Trim();
            var format = Attribute(button, "data-format") ?? button.QuerySelector(".screening-format")?.TextContent.Trim();
            var sourceStatus = Attribute(button, "data-status") ?? button.QuerySelector(".screening-status")?.TextContent;
            var bookingHref = Attribute(button, "data-booking-url", "data-reservation-url", "data-url") ??
                button.QuerySelector("a[href*='reservationSeances']")?.GetAttribute("href") ?? button.GetAttribute("href") ?? button.GetAttribute("onclick");
            var bookingUrl = ParseBookingUrl(bookingHref);
            var screening = new UgcScreeningDto(screeningId, cinemaSourceId, filmId, serviceDate, start, finish,
                string.IsNullOrWhiteSpace(room) ? null : room, version, string.IsNullOrWhiteSpace(format) ? null : format,
                bookingUrl, context.SourceUrl, context.FetchedAt, string.IsNullOrWhiteSpace(sourceStatus) ? null : sourceStatus);
            parsed.TryAdd(screeningId, screening);
        }
        return new UgcParsedFragmentDto(parsed.Values.ToArray(), films.Values.ToArray());
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

    private static bool TryParseServiceDate(string value, out DateOnly serviceDate) =>
        DateOnly.TryParseExact(value, ["yyyy-MM-dd", "dd/MM/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out serviceDate);

    private static DateTimeOffset ResolveLocal(DateOnly date, TimeOnly time)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(SourceTimeZoneId);
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
            throw new InvalidDataException($"Source local time {local:O} is ambiguous or invalid in Europe/Brussels.");
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    private static DateTimeOffset? ResolveFinish(DateOnly date, TimeOnly start, string finishText)
    {
        var match = Regex.Match(finishText, @"\d{1,2}:\d{2}");
        if (!TimeOnly.TryParse(match.Value, CultureInfo.InvariantCulture, out var finish))
            throw new InvalidDataException($"Invalid source finish time '{finishText}'.");
        var finishDate = finish < start ? date.AddDays(1) : date;
        return ResolveLocal(finishDate, finish);
    }

    private static string? Attribute(IElement element, params string[] names) => names.Select(element.GetAttribute).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static Uri? ParseBookingUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = Regex.Match(value, """(?:https?://[^'"\s]+)?/?reservationSeances\.html\?id=\d+""");
        if (!match.Success) return null;
        var absolute = match.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? match.Value
            : OfficialSiteBaseUrl + "/" + match.Value.TrimStart('/');
        return Uri.TryCreate(absolute, UriKind.Absolute, out var uri) ? uri : null;
    }

    private async Task<string> GetAsync(string url, CancellationToken cancellationToken)
    {
        Exception? lastError = null;
        for (var attempt = 0; attempt < MaximumRequestAttempts; attempt++)
        {
            try
            {
                if (_lastRequestAt is { } last)
                {
                    var wait = TimeSpan.FromSeconds(RequestIntervalSeconds) - (clock.GetUtcNow() - last);
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
            if (attempt < MaximumRequestAttempts - 1)
                await Task.Delay(TimeSpan.FromSeconds(InitialRetryDelaySeconds * Math.Pow(2, attempt)), cancellationToken);
        }
        throw new HttpRequestException($"UGC request failed after bounded retries: {url}", lastError);
    }
}
