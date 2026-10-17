namespace Seances.Domain;

/// <summary>One showing of a film at a cinema and service time.</summary>
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
