using System.Globalization;

namespace Seances.Presentation.Web.Components.Schedule;

public sealed record CalendarTimeline(DateTimeOffset StartsAt, int Minutes, IReadOnlyList<CalendarTimeline.Tick> Ticks)
{
    public sealed record Tick(string Text, int OffsetMinutes);

    public sealed record Block(SchedulePageModel.ShowingView Showing, int OffsetMinutes, double DurationMinutes, int Lane, int LaneCount);

    public static CalendarTimeline Create(IReadOnlyList<SchedulePageModel.ShowingView> screenings)
    {
        var first = screenings.Min(item => item.Screening.StartsAt);
        var start = first.AddMinutes(-first.Minute).AddSeconds(-first.Second);
        var finish = screenings.Max(item => item.Finish.At);
        var minutes = Math.Max(60, (int)Math.Ceiling((finish - start).TotalMinutes / 60) * 60);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(Seances.Domain.Cinema.BrusselsTimeZoneId);
        var ticks = Enumerable.Range(0, minutes / 30 + 1)
            .Select(index => new Tick(TimeZoneInfo.ConvertTime(start.AddMinutes(index * 30), zone)
                .ToString("HH:mm", CultureInfo.InvariantCulture), index * 30)).ToArray();
        return new CalendarTimeline(start, minutes, ticks);
    }

    public IReadOnlyList<Block> ForRoom(IEnumerable<SchedulePageModel.ShowingView> screenings)
    {
        var laneFinishes = new List<DateTimeOffset>();
        var blocks = new List<Block>();
        foreach (var showing in screenings.OrderBy(item => item.Screening.StartsAt))
        {
            var lane = laneFinishes.FindIndex(finish => finish <= showing.Screening.StartsAt);
            if (lane < 0)
            {
                lane = laneFinishes.Count;
                laneFinishes.Add(showing.Finish.At);
            }
            else
            {
                laneFinishes[lane] = showing.Finish.At;
            }

            blocks.Add(new Block(showing, (int)(showing.Screening.StartsAt - StartsAt).TotalMinutes,
                (showing.Finish.At - showing.Screening.StartsAt).TotalMinutes, lane, 0));
        }

        return blocks.Select(block => block with { LaneCount = laneFinishes.Count }).ToArray();
    }
}
