namespace Seances.Data.Collection.Ugc.Models;

/// <summary>Published service dates discovered for one UGC cinema.</summary>
public sealed record UgcCinemaCalendarDto(string CinemaId, string CinemaName, IReadOnlyList<DateOnly> PublishedDates);
