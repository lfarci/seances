using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Dates;

public sealed class DatesSettings : CommandSettings
{
    [CommandOption("--allow-live")]
    [Description("Allow live calendar requests after checking current source directives and permissions.")]
    public bool AllowLive { get; init; }

    [CommandOption("--format <FORMAT>")]
    [Description("Output format: table or json. Defaults to table.")]
    public string? Format { get; init; }

    public string EffectiveFormat => Format?.ToLowerInvariant() ?? "table";

    public override ValidationResult Validate()
    {
        if (EffectiveFormat is not ("table" or "json"))
        {
            return ValidationResult.Error("--format must be table or json.");
        }
        return ValidationResult.Success();
    }
}
