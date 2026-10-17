using Seances.Domain;
using Xunit;

namespace Seances.Domain.Tests;

public sealed class SchedulePolicyTests
{
    [Fact]
    public void Known_runtime_adds_the_pre_show_allowance()
    {
        var start = DateTimeOffset.Parse("2026-10-17T18:00:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(start), new Film("film", "Film", 100, "fixture"), 20);

        Assert.Equal(start.AddMinutes(120), finish.At);
        Assert.Equal(FinishBasis.RuntimeEstimate, finish.Basis);
        Assert.Null(finish.Label);
    }

    [Fact]
    public void Missing_runtime_uses_the_named_assumption_and_label()
    {
        var start = DateTimeOffset.Parse("2026-10-17T18:00:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(start), new Film("film", "Film", null, "unavailable"), 15);

        Assert.Equal(start.AddMinutes(195), finish.At);
        Assert.Equal(FinishBasis.AssumedRuntime, finish.Basis);
        Assert.Equal(SchedulePolicy.UnknownRuntimeLabel, finish.Label);
    }

    [Fact]
    public void Source_reported_finish_takes_precedence_over_runtime_calculation()
    {
        var sourceFinish = DateTimeOffset.Parse("2026-10-17T21:30:00Z");
        var finish = SchedulePolicy.Finish(ScreeningAt(DateTimeOffset.Parse("2026-10-17T18:00:00Z"), sourceFinish),
            new Film("film", "Film", 100, "fixture"), 20);

        Assert.Equal(sourceFinish, finish.At);
        Assert.Equal(FinishBasis.SourceReportedEstimate, finish.Basis);
    }

    [Fact]
    public void Negative_pre_show_allowance_is_rejected()
    {
        var start = DateTimeOffset.Parse("2026-10-17T18:00:00Z");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SchedulePolicy.Finish(ScreeningAt(start), new Film("film", "Film", 100, "fixture"), -1));
    }

    private static Screening ScreeningAt(DateTimeOffset start, DateTimeOffset? reportedFinish = null) =>
        new("screening", "1", "film", DateOnly.FromDateTime(start.UtcDateTime), start, reportedFinish,
            null, "Original", null, null, "https://www.ugc.be/showings", start, null);
}
