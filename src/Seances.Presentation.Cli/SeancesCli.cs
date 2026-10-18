using Seances.Presentation.Cli.Commands.Collect;
using Seances.Presentation.Cli.Commands.Inspect;
using Seances.Presentation.Cli.Commands.Validate;
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
            configuration.AddCommand<ValidateCommand>("validate")
                .WithDescription("Validate an existing local schedule JSON file.")
                .WithExample("validate", "./schedule.json");
            configuration.AddCommand<InspectCommand>("inspect")
                .WithDescription("Display screenings from a local schedule JSON file chronologically.")
                .WithExample("inspect", "./schedule.json");
            configuration.AddCommand<CollectCommand>("collect")
                .WithDescription("Extract schedule data as a table, JSON, or CSV.")
                .WithExample("collect", "--fixture", "tests/Seances.Presentation.Web.Tests/Integration/Fixtures",
                    "--format", "json")
                .WithExample("collect", "--fixture", "tests/Seances.Presentation.Web.Tests/Integration/Fixtures",
                    "--output", "./schedule.csv");
        });

        return await app.RunAsync(args);
    }
}
