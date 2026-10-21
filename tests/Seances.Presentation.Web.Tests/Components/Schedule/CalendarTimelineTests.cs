using Seances.Domain;
using Seances.Presentation.Web.Components.Schedule;
using Xunit;

namespace Seances.Presentation.Web.Tests.Components.Schedule;

public sealed class CalendarTimelineTests
{
    [Fact]
    public void Blocks_follow_elapsed_time_and_finish_including_after_midnight()
    {
        var shortShowing = Showing("short", "2026-10-17T22:15:00+02:00", 90);
        var longShowing = Showing("long", "2026-10-18T00:00:00+02:00", 180);
        var timeline = CalendarTimeline.Create([shortShowing, longShowing]);
        var blocks = timeline.ForRoom([shortShowing, longShowing]);

        Assert.Equal(300, timeline.Minutes);
        Assert.Equal(15, blocks[0].OffsetMinutes);
        Assert.Equal(120, blocks[1].OffsetMinutes);
        Assert.Equal(90, blocks[0].DurationMinutes);
        Assert.Equal(180, blocks[1].DurationMinutes);
        Assert.Equal("03:00", timeline.Ticks[^1].Text);
        Assert.All(blocks, block => Assert.Equal(1, block.LaneCount));
    }

    [Fact]
    public void Overlapping_screenings_get_separate_lanes_and_reuse_available_lanes()
    {
        var first = Showing("first", "2026-10-17T18:00:00+02:00", 120);
        var overlap = Showing("overlap", "2026-10-17T19:00:00+02:00", 90);
        var next = Showing("next", "2026-10-17T20:00:00+02:00", 90);
        var timeline = CalendarTimeline.Create([first, overlap, next]);
        var blocks = timeline.ForRoom([next, overlap, first]);

        Assert.Equal(new[] { 0, 1, 0 }, blocks.Select(block => block.Lane));
        Assert.All(blocks, block => Assert.Equal(2, block.LaneCount));
    }

    private static SchedulePageModel.ShowingView Showing(string id, string start, int minutes)
    {
        var startsAt = DateTimeOffset.Parse(start);
        var screening = new Screening(id, "1", id, new DateOnly(2026, 10, 17), startsAt,
            null, "Room 1", "VO", null, null, "https://example.test", startsAt, null);
        var finish = new FinishInfo(startsAt.AddMinutes(minutes), FinishBasis.RuntimeEstimate, null);
        return new SchedulePageModel.ShowingView(screening, new Film(id, id, minutes, "fixture"),
            "Room 1", "", "", "", "", finish);
    }
}
