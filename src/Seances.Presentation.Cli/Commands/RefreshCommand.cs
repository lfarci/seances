using Seances.Application;
using Seances.Data.Collection;
using Seances.Data.Storage;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public sealed class RefreshCommand : AsyncCommand<RefreshSettings>
{
    private readonly IScheduleRefresher _refresher;
    private readonly TextWriter _output;
    private readonly TextWriter _error;
    private readonly Func<string?> _configuredSnapshotPath;

    public RefreshCommand() : this(new UgcScheduleRefresher(), Console.Out, Console.Error,
        () => Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH"))
    {
    }

    public RefreshCommand(IScheduleRefresher refresher, TextWriter output, TextWriter error,
        Func<string?> configuredSnapshotPath)
    {
        _refresher = refresher;
        _output = output;
        _error = error;
        _configuredSnapshotPath = configuredSnapshotPath;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context, RefreshSettings settings, CancellationToken cancellationToken)
    {
        if (settings.FixtureDirectory is null && !settings.AllowLive)
        {
            _error.WriteLine("Live UGC refresh requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            var configuredPath = settings.OutputPath is { } outputPath
                ? Path.GetFullPath(outputPath)
                : _configuredSnapshotPath();
            var snapshotPath = LocalSnapshotStore.ResolvePath(configuredPath);
            await _refresher.RefreshAsync(settings.FixtureDirectory, settings.RequestedDate, snapshotPath,
                cancellationToken);
            _output.WriteLine($"Published snapshot to {Path.GetFullPath(snapshotPath)}");
            return 0;
        }
        catch (Exception error)
        {
            _error.WriteLine($"Refresh failed; the last successful snapshot was preserved. {error.Message}");
            return 1;
        }
    }
}
