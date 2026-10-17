namespace Seances.Domain;

public sealed record ScheduleSnapshot(
    int SchemaVersion,
    DateTimeOffset CheckedAt,
    string Source,
    IReadOnlyList<Cinema> Cinemas,
    IReadOnlyList<PublishedDate> PublishedDates,
    IReadOnlyList<Film> Films,
    IReadOnlyList<Screening> Screenings);

public sealed record Cinema(string Id, string Name, string Address, string TimeZone);
public sealed record PublishedDate(string CinemaId, DateOnly Date, string SourceUrl, DateTimeOffset FetchedAt, int ScreeningCount);
public sealed record Film(string Id, string Title, int? RuntimeMinutes, string RuntimeProvenance);
public sealed record Screening(
    string Id,
    string CinemaId,
    string FilmId,
    DateOnly ServiceDate,
    DateTimeOffset StartsAt,
    DateTimeOffset? SourceReportedEndsAt,
    string? Room,
    string VersionLabel,
    string? Format,
    Uri? BookingUrl,
    string SourceUrl,
    DateTimeOffset FetchedAt,
    string? SourceStatus);

public static class SchedulePolicy
{
    public static FinishInfo Finish(Screening screening, Film film, int allowanceMinutes)
    {
        if (screening.SourceReportedEndsAt is { } reported)
            return new FinishInfo(reported, FinishBasis.SourceReportedEstimate, null);

        var runtime = film.RuntimeMinutes;
        var assumed = runtime is null;
        var filmMinutes = runtime ?? 180;
        return new FinishInfo(screening.StartsAt.AddMinutes(filmMinutes + allowanceMinutes),
            assumed ? FinishBasis.AssumedRuntime : FinishBasis.RuntimeEstimate,
            assumed ? "duration unknown · 3h assumed" : null);
    }
}

public enum FinishBasis { SourceReportedEstimate, RuntimeEstimate, AssumedRuntime }
public sealed record FinishInfo(DateTimeOffset At, FinishBasis Basis, string? Label);
