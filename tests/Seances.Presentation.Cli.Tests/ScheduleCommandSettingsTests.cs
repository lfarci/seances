using Seances.Presentation.Cli.Commands;
using Xunit;

namespace Seances.Presentation.Cli.Tests;

public sealed class ScheduleCommandSettingsTests
{
    [Fact]
    public void Date_is_optional()
    {
        var settings = new RefreshSettings();

        Assert.True(settings.Validate().Successful);
        Assert.Null(settings.RequestedDate);
    }

    [Fact]
    public void Iso_date_is_valid_and_exposed_as_a_date_only()
    {
        var settings = new CollectSettings { Date = "2026-10-17" };

        Assert.True(settings.Validate().Successful);
        Assert.Equal(new DateOnly(2026, 10, 17), settings.RequestedDate);
    }

    [Theory]
    [InlineData("17-10-2026")]
    [InlineData("2026-02-30")]
    public void Non_iso_or_invalid_calendar_date_is_rejected(string date)
    {
        var settings = new RefreshSettings { Date = date };

        var result = settings.Validate();

        Assert.False(result.Successful);
        Assert.Equal("--date must be an ISO calendar date such as 2026-10-17.", result.Message);
    }
}
