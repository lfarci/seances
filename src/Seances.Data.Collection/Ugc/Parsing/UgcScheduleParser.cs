using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Seances.Data.Collection.Ugc.Models;
using Seances.Domain;

namespace Seances.Data.Collection.Ugc.Parsing;

public static class UgcScheduleParser
{
    private const string EmptyScheduleSelector = ".no-showings, .no-screenings, [data-empty='true']";
    private const string ScreeningButtonSelector = "button[data-showing]";

    public static UgcParsedFragmentDto ParseFragment(string html, UgcFetchContext context)
    {
        var document = new HtmlParser().ParseDocument(html);
        var buttons = document.QuerySelectorAll(ScreeningButtonSelector);
        if (buttons.Length == 0 && !IsExplicitlyEmpty(document))
        {
            throw new InvalidDataException("UGC returned an empty or unrecognized screening fragment.");
        }

        var parsed = new Dictionary<string, UgcScreeningDto>(StringComparer.Ordinal);
        var films = new Dictionary<string, UgcFilmDto>(StringComparer.Ordinal);
        foreach (var button in buttons)
        {
            var screening = ParseScreening(button, context, films);
            parsed.TryAdd(screening.Id, screening);
        }

        return new UgcParsedFragmentDto(parsed.Values.ToArray(), films.Values.ToArray());
    }

    private static bool IsExplicitlyEmpty(IDocument document)
    {
        if (document.QuerySelector(EmptyScheduleSelector) is not null)
        {
            return true;
        }
        var content = document.Body?.TextContent.Trim() ?? string.Empty;
        return content.Contains("aucune séance", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("aucune seance", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("no screenings", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("prochaine séance le", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("prochaine seance le", StringComparison.OrdinalIgnoreCase);
    }

    private static UgcScreeningDto ParseScreening(IElement button, UgcFetchContext context,
        IDictionary<string, UgcFilmDto> films)
    {
        var id = RequiredAttribute(button, "data-showing", "data-showing-id", "data-seance-id");
        var sourceFilmId = Attribute(button, "data-film-id", "data-filmid", "data-id-film");
        var filmId = sourceFilmId ?? RequiredAttribute(button, "data-film");
        var filmBlock = button.Closest("[id*='bloc-showing-film'], [class*='bloc-showing-film']");
        var title = Attribute(button, "data-title", "data-film-title") ??
            filmBlock?.QuerySelector(".title-film, .film-title, .block--title a, h2, h3")?.TextContent.Trim() ??
            (sourceFilmId is null ? null : Attribute(button, "data-film"));
        var cinemaId = Attribute(button, "data-cinema-id", "data-cinemaid", "data-id-cinema") ??
            RequiredAttribute(button, "data-cinema");
        var serviceDateValue = RequiredAttribute(button, "data-date", "data-seancedate");
        var startValue = Attribute(button, "data-time", "data-start", "data-start-time", "data-seancehour") ??
            button.QuerySelector(".screening-time-start")?.TextContent.Trim();
        var versionLabel = Attribute(button, "data-version", "data-language", "data-label") ??
            button.QuerySelector(".screening-version, .screening-language, [class*='version']")?.TextContent;

        RequirePresent(id, filmId, title, cinemaId, serviceDateValue, startValue, versionLabel);
        EnsureCinema(id, cinemaId!, context.CinemaId);
        var serviceDate = ParseServiceDate(id!, serviceDateValue!, context.ServiceDate);
        var startTime = ParseStartTime(id!, startValue!);
        var startsAt = ResolveLocal(serviceDate, startTime);
        MergeFilm(filmId!, title!, filmBlock, films);

        return new UgcScreeningDto(id!, cinemaId!, filmId!, serviceDate, startsAt,
            ParseFinish(button, serviceDate, startTime),
            OptionalText(button.QuerySelector(".screening-room")?.TextContent), versionLabel!,
            ParseBookingUrl(BookingHref(button)), context.SourceUrl, context.FetchedAt,
            OptionalText(Attribute(button, "data-status") ?? button.QuerySelector(".screening-status")?.TextContent));
    }

    private static void MergeFilm(string filmId, string title, IElement? filmBlock,
        IDictionary<string, UgcFilmDto> films)
    {
        var film = UgcFilmParser.Parse(filmBlock,
            new UgcFilmDto(filmId, title.Trim()));
        films[filmId] = films.TryGetValue(filmId, out var existing) ? existing.MergeMissing(film) : film;
    }

    private static string? BookingHref(IElement button) =>
        Attribute(button, "data-booking-url", "data-reservation-url", "data-url") ??
        button.QuerySelector("a[href*='reservationSeances']")?.GetAttribute("href") ??
        button.GetAttribute("href") ?? button.GetAttribute("onclick");

    private static Uri? ParseBookingUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        var match = Regex.Match(value, """(?:https?://[^'"\s]+)?/?reservationSeances\.html\?id=\d+""");
        if (!match.Success)
        {
            return null;
        }
        var url = match.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? match.Value
            : $"{UgcSourceConfiguration.BaseUrl}/{match.Value.TrimStart('/')}";
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
    }

    private static DateTimeOffset? ParseFinish(IElement button, DateOnly serviceDate, TimeOnly startTime)
    {
        var finishText = button.QuerySelector(".screening-time-end")?.TextContent.Trim();
        if (finishText is null)
        {
            return null;
        }
        var match = Regex.Match(finishText, @"\d{1,2}:\d{2}");
        if (!TimeOnly.TryParse(match.Value, CultureInfo.InvariantCulture, out var finishTime))
        {
            throw new InvalidDataException($"Invalid source finish time '{finishText}'.");
        }
        var finishDate = finishTime < startTime ? serviceDate.AddDays(1) : serviceDate;
        return ResolveLocal(finishDate, finishTime);
    }

    private static DateOnly ParseServiceDate(string screeningId, string value, DateOnly expected)
    {
        if (!DateOnly.TryParseExact(value, ["yyyy-MM-dd", "dd/MM/yyyy"], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var serviceDate) || serviceDate != expected)
        {
            throw new InvalidDataException($"Invalid or unexpected service date '{value}' for screening {screeningId}.");
        }
        return serviceDate;
    }

    private static TimeOnly ParseStartTime(string screeningId, string value)
    {
        if (!TimeOnly.TryParse(value, CultureInfo.InvariantCulture, out var startTime))
        {
            throw new InvalidDataException($"Invalid start time '{value}' for screening {screeningId}.");
        }
        return startTime;
    }

    private static DateTimeOffset ResolveLocal(DateOnly date, TimeOnly time)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(UgcSourceConfiguration.SourceTimeZoneId);
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
        {
            throw new InvalidDataException($"Source local time {local:O} is ambiguous or invalid in {UgcSourceConfiguration.SourceTimeZoneId}.");
        }
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    private static string? Attribute(IElement element, params string[] names) =>
        names.Select(element.GetAttribute).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string RequiredAttribute(IElement element, params string[] names) =>
        Attribute(element, names) ?? string.Empty;

    private static string? OptionalText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void RequirePresent(params string?[] values)
    {
        if (values.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException("A screening button is missing a required source field.");
        }
    }

    private static void EnsureCinema(string? screeningId, string cinemaId, string expectedCinemaId)
    {
        if (!cinemaId.Equals(expectedCinemaId, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Screening {screeningId} belongs to unexpected cinema {cinemaId}.");
        }
    }
}
