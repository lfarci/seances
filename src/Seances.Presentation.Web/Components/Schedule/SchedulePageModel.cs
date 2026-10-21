using Seances.Domain;

namespace Seances.Presentation.Web.Components.Schedule;

public sealed record SchedulePageModel(
    IReadOnlyList<Cinema> Cinemas,
    Cinema Cinema,
    DateOnly Date,
    IReadOnlyList<DateOnly> Dates,
    int DateIndex,
    DateOnly? Today,
    bool HasCoverage,
    string CheckedAtText,
    IReadOnlyList<string> Rooms,
    string SelectedRoom,
    IReadOnlyList<SchedulePageModel.ShowingView> Screenings)
{
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
