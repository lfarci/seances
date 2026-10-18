using Seances.Presentation.Cli.Commands;
using Seances.Presentation.Cli.Commands.Collect;
using Xunit;

namespace Seances.Presentation.Cli.Tests;

public sealed class ScheduleCommandSettingsTests
{
    [Fact]
    public void Date_is_optional()
    {
        var settings = new CollectSettings();

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
        var settings = new CollectSettings { Date = date };

        var result = settings.Validate();

        Assert.False(result.Successful);
        Assert.Equal("--date must be an ISO calendar date such as 2026-10-17.", result.Message);
    }

    [Fact]
    public void Json_output_extension_selects_json_format()
    {
        var settings = new CollectSettings { OutputPath = "schedule.json" };

        Assert.True(settings.Validate().Successful);
        Assert.Equal("json", settings.EffectiveFormat);
    }

    [Fact]
    public void Unsupported_output_extension_needs_an_explicit_format()
    {
        var result = new CollectSettings { OutputPath = "schedule.data" }.Validate();

        Assert.False(result.Successful);
        Assert.Contains("Choose --format json or csv", result.Message);
    }

    [Fact]
    public void Table_cannot_be_written_to_a_file()
    {
        var result = new CollectSettings { OutputPath = "schedule.csv", Format = "table" }.Validate();

        Assert.False(result.Successful);
        Assert.Contains("table format can only be written to stdout", result.Message);
    }
}
