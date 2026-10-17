using Seances.Domain;

namespace Seances.Data.Collection;

/// <summary>Maps collected UGC DTOs into the application schedule model.</summary>
public static class UgcScheduleMapper
{
    public static ScheduleSnapshot ToSnapshot(IReadOnlyList<UgcScheduleDateDto> collected, DateTimeOffset checkedAt, string source)
    {
        var cinemas = collected
            .Select(item => new Cinema(item.CinemaId, item.CinemaName, item.Address, Cinema.BrusselsTimeZoneId))
            .DistinctBy(cinema => cinema.Id)
            .ToArray();
        var publishedDates = collected
            .Select(item => new PublishedDate(item.CinemaId, item.ServiceDate, item.SourceUrl, item.FetchedAt, item.Screenings.Count))
            .ToArray();
        var films = collected.SelectMany(item => item.Films).Select(ToDomain).DistinctBy(film => film.Id).ToArray();
        var screenings = collected.SelectMany(item => item.Screenings).Select(ToDomain).DistinctBy(screening => screening.Id).ToArray();

        return new ScheduleSnapshot(checkedAt, source, cinemas, publishedDates, films, screenings);
    }

    private static Film ToDomain(UgcFilmDto film) => new(film.Id, film.Title, film.RuntimeMinutes, film.RuntimeProvenance);

    private static Screening ToDomain(UgcScreeningDto screening) => new(
        screening.Id,
        screening.CinemaId,
        screening.FilmId,
        screening.ServiceDate,
        screening.StartsAt,
        screening.SourceReportedEndsAt,
        screening.Room,
        screening.VersionLabel,
        screening.Format,
        screening.BookingUrl,
        screening.SourceUrl,
        screening.FetchedAt,
        screening.SourceStatus);
}
