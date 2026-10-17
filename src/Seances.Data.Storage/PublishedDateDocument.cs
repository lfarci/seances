namespace Seances.Data.Storage;

/// <summary>Published-date coverage fields in the versioned local snapshot file.</summary>
internal sealed record PublishedDateDocument(string CinemaId, DateOnly Date, string SourceUrl, DateTimeOffset FetchedAt, int ScreeningCount);
