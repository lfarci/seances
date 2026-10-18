using Seances.Presentation.Cli.Commands;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli;

internal static class SeancesCli
{
    public static async Task<int> RunAsync(string[] args)
    {
        var app = new CommandApp();
        app.Configure(configuration =>
        {
            configuration.SetApplicationName("seances");
            configuration.AddCommand<CollectCommand>("collect")
                .WithDescription("Collect and print UGC schedules without publishing a snapshot.")
                .WithExample("collect", "--allow-live", "--date", "2026-10-17");
            configuration.AddCommand<RefreshCommand>("refresh")
                .WithDescription("Publish a validated schedule snapshot for the web app.")
                .WithExample("refresh", "--fixture", "tests/Seances.Presentation.Web.Tests/Integration/Fixtures",
                    "--output", "./schedule.json");
        });

        return await app.RunAsync(args);
    }
}
