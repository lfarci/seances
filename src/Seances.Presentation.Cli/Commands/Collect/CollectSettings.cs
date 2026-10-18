using System.ComponentModel;
using Seances.Presentation.Cli.Commands;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands.Collect;

public sealed class CollectSettings : ScheduleCommandSettings
{
    [CommandOption("--allow-live")]
    [Description("Allow live collection after checking current source directives.")]
    public bool AllowLive { get; init; }

    [CommandOption("--fixture <DIRECTORY>")]
    [Description("Read deterministic fixture responses instead of making live requests.")]
    public string? FixtureDirectory { get; init; }

    [CommandOption("--output <PATH>")]
    [Description("Write JSON or CSV to this path. The extension selects the format by default.")]
    public string? OutputPath { get; init; }

    [CommandOption("--format <FORMAT>")]
    [Description("Output format: table, json, or csv. Defaults to table on stdout.")]
    public string? Format { get; init; }

    public string EffectiveFormat => Format?.ToLowerInvariant() ?? (OutputPath is null
        ? "table"
        : Path.GetExtension(OutputPath).ToLowerInvariant() switch
        {
            ".json" => "json",
            ".csv" => "csv",
            _ => string.Empty
        });

    public override ValidationResult Validate()
    {
        var result = base.Validate();
        if (!result.Successful) return result;
        if (OutputPath is not null && string.IsNullOrWhiteSpace(OutputPath))
            return ValidationResult.Error("--output must be a file path.");
        if (Format is not null && EffectiveFormat is not ("table" or "json" or "csv"))
            return ValidationResult.Error("--format must be table, json, or csv.");
        if (OutputPath is not null && EffectiveFormat.Length == 0)
            return ValidationResult.Error("Choose --format json or csv when the output path does not end in .json or .csv.");
        if (OutputPath is not null && EffectiveFormat == "table")
            return ValidationResult.Error("The table format can only be written to stdout.");
        return ValidationResult.Success();
    }
}
