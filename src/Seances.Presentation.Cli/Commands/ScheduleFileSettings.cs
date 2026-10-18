using System.ComponentModel;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public sealed class ScheduleFileSettings : CommandSettings
{
    [CommandArgument(0, "<path>")]
    [Description("Path to an existing local schedule JSON file.")]
    public string Path { get; init; } = "";
}
