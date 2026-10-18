using System.ComponentModel;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public sealed class RefreshSettings : ScheduleCommandSettings
{
    [CommandOption("--allow-live")]
    [Description("Allow live refresh after checking current source directives.")]
    public bool AllowLive { get; init; }

    [CommandOption("--fixture <DIRECTORY>")]
    [Description("Refresh from deterministic fixture responses in this directory.")]
    public string? FixtureDirectory { get; init; }

    [CommandOption("--output <PATH>")]
    [Description("Write the snapshot to this path instead of SEANCES_SNAPSHOT_PATH.")]
    public string? OutputPath { get; init; }
}
