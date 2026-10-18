using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Seances.Data.Storage;
using Seances.Domain;

namespace Seances.Presentation.Web.Components.Schedule;

internal sealed class SchedulePageModelFactory(IConfiguration configuration)
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-GB");
    private static readonly TimeZoneInfo BrusselsTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(Cinema.BrusselsTimeZoneId);

    public SchedulePageModel Create(ScheduleSnapshot snapshot, Uri requestUri, DateTimeOffset now)
    {
        var cinemas = snapshot.Cinemas;
        var dates = snapshot.PublishedDates.Select(item => item.Date).Distinct().Order().ToArray();
        var query = QueryHelpers.ParseQuery(requestUri.Query);
        var cinema = SelectCinema(cinemas, query);
        var date = SelectDate(dates, query, now);
        var dateIndex = Array.IndexOf(dates, date);
        var hasCoverage = snapshot.PublishedDates.Any(item => item.CinemaId == cinema.Id && item.Date == date);
        var allowance = AllowanceFor(cinema.Id);
        var filmsById = snapshot.Films.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var screenings = snapshot.Screenings
            .Where(item => item.CinemaId == cinema.Id && item.ServiceDate == date)
            .Select(item => CreateShowing(item, filmsById[item.FilmId], allowance))
            .OrderBy(item => item.Screening.StartsAt)
            .ThenBy(item => RoomNumber(item.Room))
            .ThenBy(item => item.Room, StringComparer.Ordinal)
            .ToArray();
        var rooms = screenings.Select(item => item.Room).Distinct(StringComparer.Ordinal)
            .OrderBy(RoomNumber)
            .ThenBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var screeningsBySlot = screenings
            .GroupBy(item => (item.Screening.StartsAt, item.Room))
            .ToDictionary(group => group.Key, group => (IReadOnlyList<SchedulePageModel.ShowingView>)group.ToArray());
        var calendarRows = screenings.Select(item => item.Screening.StartsAt).Distinct().Order()
            .Select(time => new SchedulePageModel.CalendarRow(time, FormatTime(time), rooms.Select(room =>
                new SchedulePageModel.CalendarCell(screeningsBySlot.GetValueOrDefault((time, room), []))).ToArray()))
            .ToArray();

        return new SchedulePageModel(cinemas, cinema, date, dates, dateIndex, hasCoverage,
            IsStale(snapshot.CheckedAt, now), FormatDateTime(snapshot.CheckedAt), rooms, calendarRows, screenings);
    }

    private static Cinema SelectCinema(IReadOnlyList<Cinema> cinemas, IDictionary<string, Microsoft.Extensions.Primitives.StringValues> query)
    {
        if (query.TryGetValue("cinema", out var value))
            return cinemas.FirstOrDefault(item => item.Id == value.ToString()) ?? cinemas[0];

        return cinemas[0];
    }

    private static DateOnly SelectDate(DateOnly[] dates,
        IDictionary<string, Microsoft.Extensions.Primitives.StringValues> query, DateTimeOffset now)
    {
        if (dates.Length == 0)
            return BrusselsToday(now);

        if (query.TryGetValue("date", out var value) && DateOnly.TryParse(value.ToString(), out var parsed) && dates.Contains(parsed))
            return parsed;

        var today = BrusselsToday(now);
        return dates.FirstOrDefault(date => date >= today, dates[^1]);
    }

    private static SchedulePageModel.ShowingView CreateShowing(Screening screening, Film film, int allowance)
    {
        var finish = SchedulePolicy.Finish(screening, film, allowance);
        return new SchedulePageModel.ShowingView(screening, film, RoomName(screening.Room),
            FormatTime(screening.StartsAt), FormatFinish(finish.At, screening.ServiceDate),
            FinishDescription(finish, allowance), RuntimeText(film), finish);
    }

    private static DateOnly BrusselsToday(DateTimeOffset now) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, BrusselsTimeZone).DateTime);

    private static string RoomName(string? room) =>
        string.IsNullOrWhiteSpace(room) ? "Room unavailable" : room.Trim();

    private static int RoomNumber(string room)
    {
        var digits = new string(room.SkipWhile(character => !char.IsDigit(character))
            .TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var number)
            ? number
            : room == "Room unavailable" ? int.MaxValue : int.MaxValue - 1;
    }

    private static bool IsStale(DateTimeOffset checkedAt, DateTimeOffset now) =>
        now - checkedAt > TimeSpan.FromHours(36);

    private static string FormatDateTime(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, BrusselsTimeZone).ToString("ddd d MMM, HH:mm", DisplayCulture);

    private static string FormatTime(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, BrusselsTimeZone).ToString("HH:mm", DisplayCulture);

    private static string FormatFinish(DateTimeOffset instant, DateOnly serviceDate)
    {
        var local = TimeZoneInfo.ConvertTime(instant, BrusselsTimeZone);
        return DateOnly.FromDateTime(local.DateTime) == serviceDate
            ? local.ToString("HH:mm", DisplayCulture)
            : local.ToString("ddd d MMM, HH:mm", DisplayCulture);
    }

    private static string RuntimeText(Film film) => film.RuntimeMinutes is { } runtime
        ? $"{runtime} minutes · {film.RuntimeProvenance}"
        : $"Runtime unavailable · {film.RuntimeProvenance}";

    private static string FinishDescription(FinishInfo finish, int allowance) => finish.Basis switch
    {
        FinishBasis.SourceReportedEstimate => "source-reported estimate · not a guaranteed exit time",
        FinishBasis.RuntimeEstimate => $"runtime estimate + {allowance}-minute allowance",
        _ => $"assumed runtime + {allowance}-minute allowance"
    };

    private int AllowanceFor(string cinemaId)
    {
        var configured = configuration[$"Schedule:Allowances:{cinemaId}"];
        return int.TryParse(configured, out var minutes) && minutes >= 0 ? minutes : 20;
    }
}
