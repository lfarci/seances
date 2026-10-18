using Seances.Application;
using Seances.Data.Collection;
using Seances.Data.Storage;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

internal sealed class RefreshCommand : AsyncCommand<RefreshSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context, RefreshSettings settings, CancellationToken cancellationToken)
    {
        if (settings.FixtureDirectory is null && !settings.AllowLive)
        {
            Console.Error.WriteLine("Live UGC refresh requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            var configuredPath = settings.OutputPath is { } outputPath
                ? Path.GetFullPath(outputPath)
                : Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH");
            var snapshotPath = LocalSnapshotStore.ResolvePath(configuredPath);
            using var client = new HttpClient { Timeout = CliCommandDefaults.HttpTimeout };
            var adapter = new UgcAdapter(client, TimeProvider.System);
            var store = new LocalSnapshotStore(snapshotPath);
            await new ScheduleRefreshService(adapter, store, TimeProvider.System)
                .RefreshAsync(settings.FixtureDirectory, settings.RequestedDate, cancellationToken);
            Console.WriteLine($"Published snapshot to {Path.GetFullPath(snapshotPath)}");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Refresh failed; the last successful snapshot was preserved. {error.Message}");
            return 1;
        }
    }
}
