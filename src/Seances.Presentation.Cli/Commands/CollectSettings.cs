using System.ComponentModel;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public sealed class CollectSettings : ScheduleCommandSettings
{
    [CommandOption("--allow-live")]
    [Description("Allow live collection after checking current source directives.")]
    public bool AllowLive { get; init; }
}
