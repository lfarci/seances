using Seances.Domain;
using Seances.Presentation.Web.Components.Schedule;
using Xunit;

namespace Seances.Presentation.Web.Tests.Components.Schedule;

public sealed class ShowingViewTests
{
    [Fact]
    public void Identifies_other_screenings_of_the_selected_film()
    {
        var selected = Showing("s1", "film-a");
        var sameFilm = Showing("s2", "film-a");
        var otherFilm = Showing("s3", "film-b");

        Assert.True(sameFilm.IsOtherScreeningOf(selected));
        Assert.False(selected.IsOtherScreeningOf(selected));
        Assert.False(otherFilm.IsOtherScreeningOf(selected));
        Assert.False(sameFilm.IsOtherScreeningOf(null));
    }

    private static SchedulePageModel.ShowingView Showing(string id, string filmId)
    {
        var startsAt = DateTimeOffset.Parse("2026-10-17T18:00:00+02:00");
        var screening = new Screening(id, "1", filmId, new DateOnly(2026, 10, 17), startsAt,
            null, "Room 1", "VO", null, null, "https://example.test", startsAt, null);
        var finish = new FinishInfo(startsAt.AddMinutes(90), FinishBasis.RuntimeEstimate, null);
        return new SchedulePageModel.ShowingView(screening, new Film(filmId, filmId, 90, "fixture"),
            "Room 1", "", "", "", "", finish);
    }
}
