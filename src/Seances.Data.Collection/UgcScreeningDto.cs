namespace Seances.Data.Collection;

/// <summary>Screening fields extracted from a UGC HTML response.</summary>
public sealed record UgcScreeningDto(
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
