using Seances.Collection;
using Seances.Components;
using Seances.Storage;

var snapshotPath = Environment.GetEnvironmentVariable("SEANCES_SNAPSHOT_PATH") ?? "data/schedule.json";
var fixtureIndex = Array.IndexOf(args, "--fixture");
var fixtureDirectory = fixtureIndex >= 0 && fixtureIndex + 1 < args.Length ? args[fixtureIndex + 1] : null;
var dateIndex = Array.IndexOf(args, "--date");
DateOnly? requestedDate = dateIndex >= 0 && dateIndex + 1 < args.Length && DateOnly.TryParseExact(args[dateIndex + 1], "yyyy-MM-dd", out var dateValue) ? dateValue : null;
if (args.Contains("--refresh", StringComparer.Ordinal))
{
    if (dateIndex >= 0 && requestedDate is null)
    {
        Console.Error.WriteLine("--date must be an ISO calendar date such as 2026-10-17.");
        Environment.ExitCode = 2;
        return;
    }
    if (fixtureDirectory is null && !args.Contains("--allow-live", StringComparer.Ordinal))
    {
        Console.Error.WriteLine("Live UGC refresh requires --allow-live after checking current source directives.");
        Environment.ExitCode = 2;
        return;
    }
    try
    {
        using var services = new ServiceCollection()
            .AddSingleton(TimeProvider.System)
            .AddHttpClient<UgcAdapter>(client => client.Timeout = TimeSpan.FromSeconds(20))
            .Services
            .AddSingleton(_ => new LocalSnapshotStore(snapshotPath))
            .AddTransient<ScheduleRefresher>()
            .BuildServiceProvider();
        await services.GetRequiredService<ScheduleRefresher>().RefreshAsync(fixtureDirectory, requestedDate);
        Console.WriteLine($"Published snapshot to {Path.GetFullPath(snapshotPath)}");
    }
    catch (Exception error)
    {
        Console.Error.WriteLine($"Refresh failed; the last successful snapshot was preserved. {error.Message}");
        Environment.ExitCode = 1;
    }
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton(_ => new LocalSnapshotStore(snapshotPath));
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();

public partial class Program { }
