namespace Seances.Domain;

/// <summary>A date for which a source successfully returned a schedule.</summary>
public sealed record PublishedDate(string CinemaId, DateOnly Date, string SourceUrl, DateTimeOffset FetchedAt, int ScreeningCount);
