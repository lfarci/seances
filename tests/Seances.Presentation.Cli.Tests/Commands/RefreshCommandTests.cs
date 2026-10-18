using Seances.Presentation.Cli.Commands;
using Xunit;

namespace Seances.Presentation.Cli.Tests.Commands;

public sealed class RefreshCommandTests
{
    [Fact]
    public async Task Refresh_requires_a_fixture_or_explicit_live_flag()
    {
        var refresher = new FakeScheduleRefresher();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new RefreshCommand(refresher, output, error, () => null);

        var result = await command.ExecuteAsync(null!, new RefreshSettings(), CancellationToken.None);

        Assert.Equal(2, result);
        Assert.False(refresher.WasCalled);
        Assert.Contains("requires --allow-live", error.ToString());
    }

    [Fact]
    public async Task Fixture_refresh_passes_date_and_explicit_output_path_to_the_refresher()
    {
        var date = new DateOnly(2026, 10, 17);
        var outputPath = Path.Combine(Path.GetTempPath(), "seances-output.json");
        var refresher = new FakeScheduleRefresher();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new RefreshCommand(refresher, output, error, () => null);

        var result = await command.ExecuteAsync(null!, new RefreshSettings
        {
            FixtureDirectory = "fixtures",
            Date = "2026-10-17",
            OutputPath = outputPath
        }, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal("fixtures", refresher.FixtureDirectory);
        Assert.Equal(date, refresher.RequestedDate);
        Assert.Equal(Path.GetFullPath(outputPath), refresher.SnapshotPath);
        Assert.Contains(Path.GetFullPath(outputPath), output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Live_refresh_uses_the_configured_path_when_no_output_is_supplied()
    {
        var configuredPath = Path.Combine(Path.GetTempPath(), "configured-schedule.json");
        var refresher = new FakeScheduleRefresher();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new RefreshCommand(refresher, output, error, () => configuredPath);

        var result = await command.ExecuteAsync(null!, new RefreshSettings { AllowLive = true }, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Null(refresher.FixtureDirectory);
        Assert.Equal(Path.GetFullPath(configuredPath), refresher.SnapshotPath);
    }

    [Fact]
    public async Task Refresh_failure_returns_a_failure_code_and_preservation_message()
    {
        var refresher = new FakeScheduleRefresher(new InvalidOperationException("invalid snapshot"));
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new RefreshCommand(refresher, output, error, () => null);

        var result = await command.ExecuteAsync(null!, new RefreshSettings { FixtureDirectory = "fixtures" },
            CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Contains("the last successful snapshot was preserved", error.ToString());
        Assert.Contains("invalid snapshot", error.ToString());
    }

    private sealed class FakeScheduleRefresher(Exception? failure = null) : IScheduleRefresher
    {
        public bool WasCalled { get; private set; }
        public string? FixtureDirectory { get; private set; }
        public DateOnly? RequestedDate { get; private set; }
        public string? SnapshotPath { get; private set; }

        public Task RefreshAsync(string? fixtureDirectory, DateOnly? requestedDate, string snapshotPath,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            FixtureDirectory = fixtureDirectory;
            RequestedDate = requestedDate;
            SnapshotPath = snapshotPath;
            return failure is null ? Task.CompletedTask : Task.FromException(failure);
        }
    }
}
