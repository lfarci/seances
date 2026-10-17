namespace Seances.Data.Storage;

/// <summary>Screening fields in the versioned local snapshot file.</summary>
internal sealed record ScreeningDocument(
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
