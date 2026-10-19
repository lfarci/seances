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
        Assert.Null(settings.DateSelection);
    }

    [Fact]
    public void Iso_date_is_valid_and_exposed_as_a_date_only()
    {
        var settings = new CollectSettings { Dates = ["2026-10-17"] };

        Assert.True(settings.Validate().Successful);
        Assert.Equal([new DateOnly(2026, 10, 17)], settings.DateSelection!.Dates);
    }

    [Fact]
    public void Multiple_iso_dates_are_exposed_in_sorted_distinct_order()
    {
        var settings = new CollectSettings { Dates = ["2026-10-18", "2026-10-17", "2026-10-18"] };

        Assert.True(settings.Validate().Successful);
        Assert.Equal([new DateOnly(2026, 10, 17), new DateOnly(2026, 10, 18)], settings.DateSelection!.Dates);
    }

    [Fact]
    public void Inclusive_date_range_is_valid()
    {
        var settings = new CollectSettings { From = "2026-10-17", To = "2026-10-19" };

        Assert.True(settings.Validate().Successful);
        Assert.True(settings.DateSelection!.Includes(new DateOnly(2026, 10, 17)));
        Assert.True(settings.DateSelection.Includes(new DateOnly(2026, 10, 19)));
        Assert.False(settings.DateSelection.Includes(new DateOnly(2026, 10, 20)));
    }

    [Fact]
    public void Range_requires_both_bounds_and_cannot_be_combined_with_exact_dates()
    {
        var missingBound = new CollectSettings { From = "2026-10-17" }.Validate();
        var mixedSelection = new CollectSettings
        {
            Dates = ["2026-10-17"],
            From = "2026-10-17",
            To = "2026-10-19"
        }.Validate();

        Assert.Equal("--from and --to must be provided together.", missingBound.Message);
        Assert.Equal("Use repeated --date options or the --from/--to range, not both.", mixedSelection.Message);
    }

    [Fact]
    public void Reversed_range_is_rejected()
    {
        var result = new CollectSettings { From = "2026-10-20", To = "2026-10-19" }.Validate();

        Assert.Equal("--from must be on or before --to.", result.Message);
    }

    [Theory]
    [InlineData("17-10-2026")]
    [InlineData("2026-02-30")]
    public void Non_iso_or_invalid_calendar_date_is_rejected(string date)
    {
        var settings = new CollectSettings { Dates = [date] };

        var result = settings.Validate();

        Assert.False(result.Successful);
        Assert.Equal("Dates must use ISO calendar format yyyy-MM-dd, such as 2026-10-17.", result.Message);
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
