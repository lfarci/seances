using Seances.Domain;

namespace Seances.Presentation.Web.Components.Schedule;

public sealed record SchedulePageModel(
    IReadOnlyList<Cinema> Cinemas,
    Cinema Cinema,
    DateOnly Date,
    IReadOnlyList<DateOnly> Dates,
    int DateIndex,
    bool HasCoverage,
    bool IsStale,
    string CheckedAtText,
    IReadOnlyList<string> Rooms,
    IReadOnlyList<SchedulePageModel.CalendarRow> CalendarRows,
    IReadOnlyList<SchedulePageModel.ShowingView> Screenings)
{
    public sealed record CalendarRow(DateTimeOffset StartsAt, string StartText, IReadOnlyList<CalendarCell> Cells);

    public sealed record CalendarCell(IReadOnlyList<ShowingView> Screenings);

    public sealed record ShowingView(
        Screening Screening,
        Film Film,
        string Room,
        string StartText,
        string FinishText,
        string FinishDescription,
        string RuntimeText,
        FinishInfo Finish);
}
