namespace Seances.Domain;

/// <summary>A complete, validated schedule for the cinemas and dates collected together.</summary>
public sealed record ScheduleSnapshot(
    DateTimeOffset CheckedAt,
    string Source,
    IReadOnlyList<Cinema> Cinemas,
    IReadOnlyList<PublishedDate> PublishedDates,
    IReadOnlyList<Film> Films,
    IReadOnlyList<Screening> Screenings);
