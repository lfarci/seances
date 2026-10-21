using Seances.Domain;

namespace Seances.Data.Storage;

/// <summary>Checks the invariants required before a snapshot can be persisted or served.</summary>
public static class SnapshotValidator
{
    public static void Validate(ScheduleSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Require(snapshot.CheckedAt != default && !string.IsNullOrWhiteSpace(snapshot.Source), "Snapshot check provenance is required.");

        var cinemas = ValidateCinemas(snapshot.Cinemas);
        var films = ValidateFilms(snapshot.Films);
        var dates = ValidatePublishedDates(snapshot.PublishedDates, cinemas);
        ValidateScreenings(snapshot.Screenings, cinemas, films, dates);
        ValidatePublishedCounts(snapshot.PublishedDates, snapshot.Screenings);
    }

    private static HashSet<string> ValidateCinemas(IReadOnlyList<Cinema> cinemas)
    {
        Require(cinemas.Count > 0, "The snapshot must cover at least one cinema.");
        Require(cinemas.All(cinema => !string.IsNullOrWhiteSpace(cinema.Id)), "Cinema identities are required.");
        var ids = cinemas.Select(cinema => cinema.Id).ToHashSet(StringComparer.Ordinal);
        Require(ids.Count == cinemas.Count, "Cinema identities must be unique.");

        foreach (var cinema in cinemas)
            Require(!string.IsNullOrWhiteSpace(cinema.Name) && !string.IsNullOrWhiteSpace(cinema.Address) &&
                    cinema.TimeZone == Cinema.BrusselsTimeZoneId, "Cinema identity, location, and timezone are required.");
        return ids;
    }

    private static HashSet<string> ValidateFilms(IReadOnlyList<Film> films)
    {
        var ids = films.Select(film => film.Id).ToHashSet(StringComparer.Ordinal);
        Require(ids.Count == films.Count, "Film identities must be unique.");
        foreach (var film in films)
        {
            Require(!string.IsNullOrWhiteSpace(film.Id) && !string.IsNullOrWhiteSpace(film.Title) &&
                    film.RuntimeMinutes is not <= 0 && !string.IsNullOrWhiteSpace(film.RuntimeProvenance), "Film fields or runtime provenance are invalid.");
            Require(film.OriginalTitle is null || !string.IsNullOrWhiteSpace(film.OriginalTitle), "Original titles must not be blank.");
            Require(film.SourceTitle is null || !string.IsNullOrWhiteSpace(film.SourceTitle), "Source titles must not be blank.");
            Require(film.DetailUrl is null || IsHttpsUrl(film.DetailUrl), "Film detail links must use HTTPS.");
            if (film.Details is not null) ValidateDetails(film.Details);
            if (film.Tmdb is not null) ValidateTmdb(film.Tmdb);
        }
        return ids;
    }

    private static void ValidateDetails(FilmDetails details)
    {
        Require(details.Genres.Concat(details.Directors).Concat(details.Cast).All(value => !string.IsNullOrWhiteSpace(value)),
            "Film genres, directors, and cast must not be blank.");
        Require(details.Synopsis is null || !string.IsNullOrWhiteSpace(details.Synopsis), "Film synopses must not be blank.");
        Require(details.PosterUrl is null || IsHttpsUrl(details.PosterUrl), "Film poster links must use HTTPS.");
    }

    private static void ValidateTmdb(TmdbFilmMetadata metadata)
    {
        Require(metadata.TmdbId > 0 && metadata.RetrievedAt != default, "TMDB metadata requires a movie ID and retrieval time.");
        Require(metadata.TrailerUrl is null || IsHttpsUrl(metadata.TrailerUrl), "Trailer links must use HTTPS.");
        Require(metadata.PosterUrl is null || IsHttpsUrl(metadata.PosterUrl), "TMDB poster links must use HTTPS.");
        Require(metadata.Rating is null or >= 0 and <= 10 && metadata.VoteCount is null or >= 0, "TMDB ratings are out of range.");
        Require(metadata.Synopsis is null || !string.IsNullOrWhiteSpace(metadata.Synopsis), "TMDB synopses must not be blank.");
    }

    private static HashSet<(string CinemaId, DateOnly Date)> ValidatePublishedDates(
        IReadOnlyList<PublishedDate> publishedDates, HashSet<string> cinemas)
    {
        var keys = publishedDates.Select(date => (date.CinemaId, date.Date)).ToHashSet();
        Require(keys.Count == publishedDates.Count, "Published-date coverage entries must be unique.");
        foreach (var date in publishedDates)
        {
            Require(date.Date != default, "Published-date coverage requires a service date.");
            Require(cinemas.Contains(date.CinemaId) && date.FetchedAt != default && date.ScreeningCount >= 0 &&
                    IsHttpsUrl(date.SourceUrl), "Published-date coverage provenance is invalid.");
        }
        return keys;
    }

    private static void ValidateScreenings(IReadOnlyList<Screening> screenings, HashSet<string> cinemas,
        HashSet<string> films, HashSet<(string CinemaId, DateOnly Date)> dates)
    {
        var ids = screenings.Select(screening => screening.Id).ToHashSet(StringComparer.Ordinal);
        Require(ids.Count == screenings.Count, "Screening identities must be unique.");

        foreach (var screening in screenings)
        {
            Require(!string.IsNullOrWhiteSpace(screening.Id) && cinemas.Contains(screening.CinemaId) && films.Contains(screening.FilmId) &&
                    dates.Contains((screening.CinemaId, screening.ServiceDate)) && screening.StartsAt != default &&
                    !string.IsNullOrWhiteSpace(screening.VersionLabel) && screening.FetchedAt != default &&
                    IsHttpsUrl(screening.SourceUrl), "A screening has a missing field or unresolved reference.");
            Require(screening.SourceReportedEndsAt is null || screening.SourceReportedEndsAt > screening.StartsAt,
                "A source-reported finish must follow the screening start.");
            Require(screening.BookingUrl is null || IsHttpsUrl(screening.BookingUrl), "Booking links must use HTTPS.");
        }
    }

    private static void ValidatePublishedCounts(IReadOnlyList<PublishedDate> dates, IReadOnlyList<Screening> screenings)
    {
        var counts = screenings.GroupBy(screening => (screening.CinemaId, screening.ServiceDate))
            .ToDictionary(group => group.Key, group => group.Count());
        foreach (var date in dates)
            Require(counts.GetValueOrDefault((date.CinemaId, date.Date)) == date.ScreeningCount,
                "Published-date screening counts must match the validated snapshot.");
    }

    private static bool IsHttpsUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && IsHttpsUrl(uri);

    private static bool IsHttpsUrl(Uri uri) =>
        uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrWhiteSpace(uri.Host);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
