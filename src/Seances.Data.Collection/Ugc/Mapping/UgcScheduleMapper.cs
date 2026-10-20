using Seances.Data.Collection.Ugc.Models;
using Seances.Domain;

namespace Seances.Data.Collection.Ugc.Mapping;

/// <summary>Maps collected UGC DTOs into the application schedule model.</summary>
public static class UgcScheduleMapper
{
    private const string RuntimeNotCollected = "not collected";

    public static ScheduleSnapshot ToSnapshot(IReadOnlyList<UgcScheduleDateDto> collected, DateTimeOffset checkedAt, string source,
        IReadOnlyList<UgcFilmDto>? enrichedFilms = null)
    {
        var cinemas = collected
            .Select(item => new Cinema(item.CinemaId, item.CinemaName, item.Address, Cinema.BrusselsTimeZoneId))
            .DistinctBy(cinema => cinema.Id)
            .ToArray();
        var publishedDates = collected
            .Select(item => new PublishedDate(item.CinemaId, item.ServiceDate, item.SourceUrl, item.FetchedAt, item.Screenings.Count))
            .ToArray();
        var films = MergeFilms(enrichedFilms ?? collected.SelectMany(item => item.Films)).Select(ToDomain).ToArray();
        var screenings = collected.SelectMany(item => item.Screenings).Select(ToDomain).DistinctBy(screening => screening.Id).ToArray();

        return new ScheduleSnapshot(checkedAt, source, cinemas, publishedDates, films, screenings);
    }

    public static IReadOnlyList<UgcFilmDto> MergeFilms(IEnumerable<UgcFilmDto> films) => films
        .GroupBy(film => film.Id, StringComparer.Ordinal)
        .Select(group => group.Aggregate((current, next) => current.MergeMissing(next))).ToArray();

    private static Film ToDomain(UgcFilmDto film) => new(film.Id, film.OriginalTitle ?? film.Title,
        null, RuntimeNotCollected, film.OriginalTitle, film.Title, film.DetailUrl);

    private static Screening ToDomain(UgcScreeningDto screening) => new(
        screening.Id,
        screening.CinemaId,
        screening.FilmId,
        screening.ServiceDate,
        screening.StartsAt,
        screening.SourceReportedEndsAt,
        screening.Room,
        screening.VersionLabel,
        null,
        screening.BookingUrl,
        screening.SourceUrl,
        screening.FetchedAt,
        screening.SourceStatus);
}
