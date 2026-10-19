using Seances.Domain;

namespace Seances.Data.Storage;

internal static class SnapshotDocumentMapper
{
    public static SnapshotDocument FromDomain(ScheduleSnapshot snapshot) => new(
        SnapshotDocument.CurrentSchemaVersion,
        snapshot.CheckedAt,
        snapshot.Source,
        snapshot.Cinemas.Select(cinema => new CinemaDocument(cinema.Id, cinema.Name, cinema.Address, cinema.TimeZone)).ToArray(),
        snapshot.PublishedDates.Select(date => new PublishedDateDocument(date.CinemaId, date.Date, date.SourceUrl, date.FetchedAt, date.ScreeningCount)).ToArray(),
        snapshot.Films.Select(film => new FilmDocument(film.Id, film.Title, film.RuntimeMinutes, film.RuntimeProvenance,
            film.OriginalTitle, film.SourceTitle, film.DetailUrl)).ToArray(),
        snapshot.Screenings.Select(ToDocument).ToArray());

    public static ScheduleSnapshot ToDomain(SnapshotDocument document)
    {
        if (document.Cinemas is null || document.PublishedDates is null || document.Films is null || document.Screenings is null ||
            document.Cinemas.Any(cinema => cinema is null) || document.PublishedDates.Any(date => date is null) ||
            document.Films.Any(film => film is null) || document.Screenings.Any(screening => screening is null))
            throw new InvalidDataException("Snapshot arrays are required.");

        return new ScheduleSnapshot(document.CheckedAt, document.Source,
            document.Cinemas.Select(cinema => new Cinema(cinema.Id, cinema.Name, cinema.Address, cinema.TimeZone)).ToArray(),
            document.PublishedDates.Select(date => new PublishedDate(date.CinemaId, date.Date, date.SourceUrl, date.FetchedAt, date.ScreeningCount)).ToArray(),
            document.Films.Select(film => new Film(film.Id, film.Title, film.RuntimeMinutes, film.RuntimeProvenance,
                film.OriginalTitle, film.SourceTitle, film.DetailUrl)).ToArray(),
            document.Screenings.Select(ToDomain).ToArray());
    }

    private static ScreeningDocument ToDocument(Screening screening) => new(
        screening.Id, screening.CinemaId, screening.FilmId, screening.ServiceDate, screening.StartsAt,
        screening.SourceReportedEndsAt, screening.Room, screening.VersionLabel, screening.Format,
        screening.BookingUrl, screening.SourceUrl, screening.FetchedAt, screening.SourceStatus);

    private static Screening ToDomain(ScreeningDocument screening) => new(
        screening.Id, screening.CinemaId, screening.FilmId, screening.ServiceDate, screening.StartsAt,
        screening.SourceReportedEndsAt, screening.Room, screening.VersionLabel, screening.Format,
        screening.BookingUrl, screening.SourceUrl, screening.FetchedAt, screening.SourceStatus);
}
