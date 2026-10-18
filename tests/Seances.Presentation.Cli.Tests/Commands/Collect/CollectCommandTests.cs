using Seances.Domain;
using Seances.Presentation.Cli.Commands.Collect;
using Xunit;

namespace Seances.Presentation.Cli.Tests.Commands.Collect;

public sealed class CollectCommandTests
{
    [Fact]
    public async Task Live_collection_requires_the_explicit_flag()
    {
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings { Format = "json" },
            CancellationToken.None);

        Assert.Equal(2, result);
        Assert.False(collector.WasCalled);
        Assert.Contains("requires --allow-live", error.ToString());
    }

    [Fact]
    public async Task Default_output_is_a_formatted_table()
    {
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings { FixtureDirectory = "fixtures" },
            CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Contains("Cinema", output.ToString());
        Assert.Contains("Film, One", output.ToString());
        Assert.Contains("20:30", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Json_format_writes_json_to_stdout()
    {
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings
        {
            FixtureDirectory = "fixtures",
            Format = "json"
        }, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Contains("\"schemaVersion\": 1", output.ToString());
        Assert.Contains("\"title\": \"Film, One\"", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Csv_format_writes_escaped_csv_to_stdout()
    {
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings
        {
            FixtureDirectory = "fixtures",
            Format = "csv"
        }, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Contains("CinemaId,Cinema,ServiceDate,ScreeningId,FilmId,Film,StartsAt", output.ToString());
        Assert.Contains("\"Film, One\"", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Output_extension_selects_json_file_format()
    {
        var directory = CreateTemporaryDirectory();
        var outputPath = Path.Combine(directory, "schedule.json");
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);
        try
        {
            var result = await command.ExecuteAsync(null!, new CollectSettings
            {
                FixtureDirectory = "fixtures",
                OutputPath = outputPath
            }, CancellationToken.None);

            var json = await File.ReadAllTextAsync(outputPath);
            Assert.Equal(0, result);
            Assert.Contains("\"schemaVersion\": 1", json);
            Assert.Contains("Wrote json to", error.ToString());
            Assert.Empty(output.ToString());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Csv_format_writes_a_file_when_an_output_path_is_given()
    {
        var directory = CreateTemporaryDirectory();
        var outputPath = Path.Combine(directory, "schedule.csv");
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);
        try
        {
            var result = await command.ExecuteAsync(null!, new CollectSettings
            {
                FixtureDirectory = "fixtures",
                Format = "csv",
                OutputPath = outputPath
            }, CancellationToken.None);

            var csv = await File.ReadAllTextAsync(outputPath);
            Assert.Equal(0, result);
            Assert.StartsWith("CinemaId,Cinema,ServiceDate,ScreeningId,FilmId,Film,StartsAt", csv);
            Assert.Contains("\"Film, One\"", csv);
            Assert.Empty(output.ToString());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Table_format_cannot_be_written_to_a_file()
    {
        var collector = new FakeScheduleCollector(CreateSchedule());
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings
        {
            FixtureDirectory = "fixtures",
            Format = "table",
            OutputPath = "schedule.txt"
        }, CancellationToken.None);

        Assert.Equal(2, result);
        Assert.False(collector.WasCalled);
        Assert.Contains("table format can only be written to stdout", error.ToString());
    }

    [Fact]
    public async Task Collection_failure_returns_a_failure_code_and_writes_the_error()
    {
        var collector = new FakeScheduleCollector(CreateSchedule(), new InvalidOperationException("source unavailable"));
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new CollectCommand(collector, output, error);

        var result = await command.ExecuteAsync(null!, new CollectSettings { FixtureDirectory = "fixtures" },
            CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Contains("Collection failed. source unavailable", error.ToString());
    }

    private static ScheduleSnapshot CreateSchedule()
    {
        var cinema = new Cinema("cinema-1", "Central Cinema", "Brussels", Cinema.BrusselsTimeZoneId);
        var film = new Film("film-1", "Film, One", 90, "fixture");
        const string sourceUrl = "https://source.example.test/schedule";
        var fetchedAt = DateTimeOffset.Parse("2026-10-16T18:00:00Z");
        var screening = new Screening("screening-1", cinema.Id, film.Id, new DateOnly(2026, 10, 17),
            DateTimeOffset.Parse("2026-10-17T18:30:00Z"), null, "Room 1", "Original", "2D", null,
            sourceUrl, fetchedAt, null);
        return new ScheduleSnapshot(fetchedAt, "fixtures", [cinema],
            [new PublishedDate(cinema.Id, screening.ServiceDate, sourceUrl, fetchedAt, 1)], [film], [screening]);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "seances-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private sealed class FakeScheduleCollector(ScheduleSnapshot schedule, Exception? failure = null)
        : IScheduleCollector
    {
        public bool WasCalled { get; private set; }

        public Task<ScheduleSnapshot> CollectAsync(string? fixtureDirectory, DateOnly? requestedDate,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            return failure is null ? Task.FromResult(schedule) : Task.FromException<ScheduleSnapshot>(failure);
        }
    }
}
