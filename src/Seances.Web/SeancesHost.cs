using Seances.Application;
using Seances.Data.Collection;
using Seances.Web.Components;
using Seances.Data.Storage;

internal static class SeancesHost
{
    private const string RefreshFlag = "--refresh";
    private const string FixtureFlag = "--fixture";
    private const string AllowLiveFlag = "--allow-live";
    private const string DateFlag = "--date";
    private const string DefaultSnapshotPath = "data/schedule.json";
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

    public static async Task<int> RunAsync(string[] args)
    {
        var snapshotPath = Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH") ?? DefaultSnapshotPath;
        if (args.Contains(RefreshFlag, StringComparer.Ordinal))
            return await RefreshAsync(args, snapshotPath);

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorComponents();
        builder.Services.AddSingleton(_ => new LocalSnapshotStore(snapshotPath));
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapRazorComponents<App>();
        await app.RunAsync();
        return 0;
    }

    private static async Task<int> RefreshAsync(string[] args, string snapshotPath)
    {
        var dateIndex = Array.IndexOf(args, DateFlag);
        DateOnly? requestedDate = null;
        if (dateIndex >= 0)
        {
            if (dateIndex + 1 >= args.Length || !DateOnly.TryParseExact(args[dateIndex + 1], "yyyy-MM-dd", out var parsedDate))
            {
                Console.Error.WriteLine("--date must be an ISO calendar date such as 2026-10-17.");
                return 2;
            }
            requestedDate = parsedDate;
        }

        var fixtureIndex = Array.IndexOf(args, FixtureFlag);
        var fixtureDirectory = fixtureIndex >= 0 && fixtureIndex + 1 < args.Length ? args[fixtureIndex + 1] : null;
        if (fixtureDirectory is null && !args.Contains(AllowLiveFlag, StringComparer.Ordinal))
        {
            Console.Error.WriteLine("Live UGC refresh requires --allow-live after checking current source directives.");
            return 2;
        }

        try
        {
            using var client = new HttpClient { Timeout = HttpTimeout };
            var adapter = new UgcAdapter(client, TimeProvider.System);
            var store = new LocalSnapshotStore(snapshotPath);
            await new ScheduleRefreshService(adapter, store, TimeProvider.System).RefreshAsync(fixtureDirectory, requestedDate);
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
