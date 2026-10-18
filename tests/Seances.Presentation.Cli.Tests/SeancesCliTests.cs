using System.Diagnostics;
using System.Text.Json.Nodes;
using Seances.Data.Storage;
using Seances.Domain;
using Xunit;

namespace Seances.Presentation.Cli.Tests;

public sealed class SeancesCliTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("seances-cli-").FullName;

    [Theory]
    [InlineData("validate")]
    [InlineData("inspect")]
    public async Task Commands_reject_missing_service_dates_even_when_references_match(string command)
    {
        var document = JsonNode.Parse(SnapshotJsonSerializer.Serialize(CreateSchedule()))!;
        foreach (var date in document["publishedDates"]!.AsArray()) date!.AsObject().Remove("date");
        foreach (var screening in document["screenings"]!.AsArray()) screening!.AsObject().Remove("serviceDate");
        var path = Path.Combine(_directory, "invalid.json");
        await File.WriteAllTextAsync(path, document.ToJsonString());

        var (exitCode, output, error) = await RunAsync(command, path);

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.Contains("service date", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("validate", "Valid schedule")]
    [InlineData("inspect", "No screenings")]
    public async Task Commands_accept_a_valid_empty_schedule(string command, string message)
    {
        var schedule = CreateSchedule();
        schedule = schedule with
        {
            Screenings = [],
            Films = [],
            PublishedDates = schedule.PublishedDates.Select(date => date with { ScreeningCount = 0 }).ToArray()
        };
        var path = Path.Combine(_directory, "empty.json");
        await File.WriteAllTextAsync(path, SnapshotJsonSerializer.Serialize(schedule));

        var (exitCode, output, error) = await RunAsync(command, path);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains(message, output);
    }

    [Fact]
    public async Task Inspect_preserves_brackets_in_film_titles_and_source_version_labels()
    {
        var schedule = CreateSchedule();
        schedule = schedule with
        {
            Films = schedule.Films.Select(film => film with { Title = "Film [cut]" }).ToArray(),
            Screenings = schedule.Screenings.Select(screening => screening with { VersionLabel = "[VO]" }).ToArray()
        };
        var path = Path.Combine(_directory, "schedule.json");
        await File.WriteAllTextAsync(path, SnapshotJsonSerializer.Serialize(schedule));

        var (exitCode, output, error) = await RunAsync("inspect", path);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains("Film [cut]", output);
        Assert.Contains("[VO]", output);
    }

    [Fact]
    public async Task Inspect_displays_screenings_chronologically_across_cinemas_without_changing_the_file()
    {
        var path = Path.Combine(_directory, "schedule.json");
        var json = SnapshotJsonSerializer.Serialize(CreateSchedule());
        await File.WriteAllTextAsync(path, json);
        var lastWrite = File.GetLastWriteTimeUtc(path);

        var (exitCode, output, error) = await RunAsync("inspect", path);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains("2026-10-17", output);
        Assert.Contains("20:00", output);
        Assert.Contains("21:00", output);
        Assert.Contains("Zulu Cinema", output);
        Assert.Contains("Alpha Cinema", output);
        Assert.Contains("VOstBIL", output);
        Assert.Contains("VFstNL", output);
        Assert.Contains("Early Film", output);
        Assert.Contains("Late Film", output);
        Assert.Contains("Dawn Film", output);
        Assert.True(output.IndexOf("Early Film", StringComparison.Ordinal) <
                    output.IndexOf("Late Film", StringComparison.Ordinal), output);
        Assert.True(output.IndexOf("Late Film", StringComparison.Ordinal) <
                    output.IndexOf("Dawn Film", StringComparison.Ordinal), output);
        var overnightRow = output.Split('\n').Single(line => line.Contains("Dawn Film", StringComparison.Ordinal));
        Assert.Contains("2026-10-17", overnightRow);
        Assert.Contains("00:30", overnightRow);
        Assert.Equal(json, await File.ReadAllTextAsync(path));
        Assert.Equal(lastWrite, File.GetLastWriteTimeUtc(path));
        Assert.Single(Directory.GetFiles(_directory));
    }

    [Theory]
    [InlineData("validate", "{ invalid", "Invalid schedule JSON")]
    [InlineData("inspect", "{ invalid", "Invalid schedule JSON")]
    [InlineData("validate", "null", "A schedule JSON object is required")]
    [InlineData("inspect", "null", "A schedule JSON object is required")]
    [InlineData("validate", "{\"schemaVersion\":999}", "Unsupported schedule schema version 999")]
    [InlineData("inspect", "{\"schemaVersion\":999}", "Unsupported schedule schema version 999")]
    [InlineData("validate", "{}", "Expected version 1")]
    [InlineData("inspect", "{}", "Expected version 1")]
    [InlineData("validate", "{\"schemaVersion\":1}", "Snapshot arrays are required")]
    [InlineData("inspect", "{\"schemaVersion\":1}", "Snapshot arrays are required")]
    public async Task Commands_reject_invalid_documents_with_an_actionable_error(
        string command, string json, string reason)
    {
        var path = Path.Combine(_directory, "invalid.json");
        await File.WriteAllTextAsync(path, json);

        var (exitCode, output, error) = await RunAsync(command, "invalid.json");

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.Contains("invalid.json", error);
        Assert.Contains(reason, error);
        Assert.Equal(json, await File.ReadAllTextAsync(path));
    }

    [Theory]
    [InlineData("validate", "filmId", "missing-film", "unresolved reference")]
    [InlineData("inspect", "filmId", "missing-film", "unresolved reference")]
    [InlineData("validate", "id", "early", "Screening identities must be unique")]
    [InlineData("inspect", "id", "early", "Screening identities must be unique")]
    [InlineData("validate", "startsAt", "not-a-date", "Invalid schedule JSON")]
    [InlineData("inspect", "startsAt", "not-a-date", "Invalid schedule JSON")]
    public async Task Commands_reject_inconsistent_or_malformed_screenings(
        string command, string field, string value, string reason)
    {
        var document = JsonNode.Parse(SnapshotJsonSerializer.Serialize(CreateSchedule()))!;
        document["screenings"]![0]![field] = value;
        var json = document.ToJsonString();
        var path = Path.Combine(_directory, "invalid.json");
        await File.WriteAllTextAsync(path, json);

        var (exitCode, output, error) = await RunAsync(command, path);

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.Contains(reason, error);
        Assert.Equal(json, await File.ReadAllTextAsync(path));
    }

    [Theory]
    [InlineData("validate")]
    [InlineData("inspect")]
    public async Task Commands_reject_inconsistent_coverage_counts(string command)
    {
        var document = JsonNode.Parse(SnapshotJsonSerializer.Serialize(CreateSchedule()))!;
        document["publishedDates"]![0]!["screeningCount"] = 99;
        var path = Path.Combine(_directory, "invalid.json");
        await File.WriteAllTextAsync(path, document.ToJsonString());

        var (exitCode, output, error) = await RunAsync(command, path);

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.Contains("Published-date screening counts must match", error);
    }

    [Theory]
    [InlineData("validate")]
    [InlineData("inspect")]
    public async Task Commands_report_a_missing_file_without_creating_it(string command)
    {
        var (exitCode, output, error) = await RunAsync(command, "missing.json");

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.Contains("missing.json", error);
        Assert.Contains("Could not find file", error);
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Theory]
    [InlineData("validate")]
    [InlineData("inspect")]
    public async Task Commands_require_an_explicit_input_path(string command)
    {
        var (exitCode, output, error) = await RunAsync(command);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("path", output + error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task Validate_accepts_a_supported_local_schedule_without_changing_the_file()
    {
        var path = Path.Combine(_directory, "schedule with spaces.json");
        var json = SnapshotJsonSerializer.Serialize(CreateSchedule());
        await File.WriteAllTextAsync(path, json);
        var lastWrite = File.GetLastWriteTimeUtc(path);

        var (exitCode, output, error) = await RunAsync("validate", path);

        Assert.Equal(0, exitCode);
        Assert.Contains("Valid schedule", output);
        Assert.Empty(error);
        Assert.Equal(json, await File.ReadAllTextAsync(path));
        Assert.Equal(lastWrite, File.GetLastWriteTimeUtc(path));
        Assert.Single(Directory.GetFiles(_directory));
    }

    private async Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = _directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.Environment["SEANCES_SNAPSHOT_PATH"] = Path.Combine(_directory, "unused.json");
        startInfo.ArgumentList.Add(typeof(SeancesCli).Assembly.Location);
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            return (process.ExitCode, await output, await error);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
    }

    private static ScheduleSnapshot CreateSchedule()
    {
        var date = new DateOnly(2026, 10, 17);
        var checkedAt = DateTimeOffset.Parse("2026-10-16T18:00:00Z");
        const string sourceUrl = "https://source.example.test/schedule";
        return new ScheduleSnapshot(checkedAt, "synthetic",
            [new Cinema("east", "Alpha Cinema", "Brussels", Cinema.BrusselsTimeZoneId),
             new Cinema("west", "Zulu Cinema", "Brussels", Cinema.BrusselsTimeZoneId)],
            [new PublishedDate("east", date, sourceUrl, checkedAt, 2),
             new PublishedDate("west", date, sourceUrl, checkedAt, 1)],
            [new Film("early", "Early Film", 90, "fixture"), new Film("late", "Late Film", 100, "fixture"),
             new Film("night", "Dawn Film", 90, "fixture")],
            [new Screening("night", "east", "night", date, DateTimeOffset.Parse("2026-10-18T00:30:00+02:00"),
                null, null, "Original", null, null, sourceUrl, checkedAt, null),
             new Screening("late", "east", "late", date, DateTimeOffset.Parse("2026-10-17T19:00:00Z"),
                null, null, "VFstNL", null, null, sourceUrl, checkedAt, null),
             new Screening("early", "west", "early", date, DateTimeOffset.Parse("2026-10-17T18:00:00Z"),
                null, null, "VOstBIL", null, null, sourceUrl, checkedAt, null)]);
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
