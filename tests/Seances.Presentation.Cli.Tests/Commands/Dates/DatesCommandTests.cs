using Seances.Data.Collection.Ugc.Models;
using Seances.Presentation.Cli.Commands.Dates;
using Xunit;

namespace Seances.Presentation.Cli.Tests.Commands.Dates;

public sealed class DatesCommandTests
{
    [Fact]
    public async Task Requires_live_flag_before_querying_calendars()
    {
        var lister = new FakePublishedDateLister([]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new DatesCommand(lister, output, error);

        var result = await command.ExecuteAsync(null!, new DatesSettings(), CancellationToken.None);

        Assert.Equal(2, result);
        Assert.False(lister.WasCalled);
        Assert.Empty(output.ToString());
        Assert.Contains("--allow-live", error.ToString());
    }

    [Theory]
    [InlineData("table")]
    [InlineData("json")]
    public async Task Prints_published_dates_from_the_calendar_lister(string format)
    {
        var dates = new[] { new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 20) };
        var calendar = new UgcCinemaCalendarDto("2", "UGC De Brouckère", dates);
        var lister = new FakePublishedDateLister([calendar]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new DatesCommand(lister, output, error);

        var result = await command.ExecuteAsync(null!, new DatesSettings { AllowLive = true, Format = format },
            CancellationToken.None);

        Assert.Equal(0, result);
        Assert.True(lister.WasCalled);
        Assert.Contains("2026-10-19", output.ToString());
        Assert.Contains("2026-10-20", output.ToString());
        Assert.Contains("Reading published calendar dates", error.ToString());
        Assert.Contains("Found published dates for 1 cinemas", error.ToString());
    }

    private sealed class FakePublishedDateLister(IReadOnlyList<UgcCinemaCalendarDto> calendars)
        : IPublishedDateLister
    {
        public bool WasCalled { get; private set; }

        public Task<IReadOnlyList<UgcCinemaCalendarDto>> GetPublishedDatesAsync(CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(calendars);
        }
    }
}
