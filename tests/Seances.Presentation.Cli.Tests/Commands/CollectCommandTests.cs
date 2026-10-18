using Seances.Data.Collection;
using Seances.Presentation.Cli.Commands;
using Xunit;

namespace Seances.Presentation.Cli.Tests.Commands;

public sealed class CollectCommandTests
{
    [Fact]
    public async Task Live_collection_is_rejected_without_the_explicit_flag()
    {
        var collector = new FakeScheduleCollector([]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings(), CancellationToken.None);

        Assert.Equal(2, result);
        Assert.False(collector.WasCalled);
        Assert.Contains("requires --allow-live", error.ToString());
    }

    [Fact]
    public async Task Collection_prints_screenings_in_time_order_for_the_requested_date()
    {
        var date = new DateOnly(2026, 10, 17);
        var collector = new FakeScheduleCollector([CreateSchedule(date)]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings
        {
            AllowLive = true,
            Date = "2026-10-17"
        }, CancellationToken.None);

        var text = output.ToString();
        Assert.Equal(0, result);
        Assert.Equal(date, collector.RequestedDate);
        Assert.True(text.IndexOf("Earlier Film", StringComparison.Ordinal) <
                    text.IndexOf("Later Film", StringComparison.Ordinal));
        Assert.Contains("Collection complete", text);
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Collection_failure_returns_a_failure_code_and_writes_the_error()
    {
        var collector = new FakeScheduleCollector([], new InvalidOperationException("source unavailable"));
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings { AllowLive = true }, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Contains("Collection failed. source unavailable", error.ToString());
    }

    private static UgcScheduleDateDto CreateSchedule(DateOnly date)
    {
        var earlierFilm = new UgcFilmDto("earlier", "Earlier Film", 90, "fixture");
        var laterFilm = new UgcFilmDto("later", "Later Film", 90, "fixture");
        const string sourceUrl = "https://source.example.test/schedule";
        var fetchedAt = DateTimeOffset.Parse("2026-10-16T18:00:00Z");
        var screenings = new[]
        {
            new UgcScreeningDto("later-screening", "cinema-1", laterFilm.Id, date,
                DateTimeOffset.Parse("2026-10-17T20:30:00Z"), null, null, "Original", null, null,
                sourceUrl, fetchedAt, null),
            new UgcScreeningDto("earlier-screening", "cinema-1", earlierFilm.Id, date,
                DateTimeOffset.Parse("2026-10-17T18:30:00Z"), null, null, "Original", null, null,
                sourceUrl, fetchedAt, null)
        };
        return new UgcScheduleDateDto("cinema-1", "Central Cinema", "Brussels", date, sourceUrl,
            fetchedAt, [earlierFilm, laterFilm], screenings);
    }

    private sealed class FakeScheduleCollector(IReadOnlyList<UgcScheduleDateDto> schedules, Exception? failure = null)
        : IScheduleCollector
    {
        public bool WasCalled { get; private set; }
        public DateOnly? RequestedDate { get; private set; }

        public Task<IReadOnlyList<UgcScheduleDateDto>> CollectAsync(DateOnly? requestedDate,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            RequestedDate = requestedDate;
            return failure is null
                ? Task.FromResult(schedules)
                : Task.FromException<IReadOnlyList<UgcScheduleDateDto>>(failure);
        }
    }
}
