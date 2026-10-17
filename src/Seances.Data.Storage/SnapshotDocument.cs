namespace Seances.Data.Storage;

/// <summary>Versioned JSON representation of the persisted schedule snapshot.</summary>
internal sealed record SnapshotDocument(
    int SchemaVersion,
    DateTimeOffset CheckedAt,
    string Source,
    CinemaDocument[] Cinemas,
    PublishedDateDocument[] PublishedDates,
    FilmDocument[] Films,
    ScreeningDocument[] Screenings)
{
    public const int CurrentSchemaVersion = 1;
}
