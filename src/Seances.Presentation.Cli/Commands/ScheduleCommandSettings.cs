using System.ComponentModel;
using System.Globalization;
using Seances.Domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public abstract class ScheduleCommandSettings : CommandSettings
{
    [CommandOption("--date <DATE>")]
    [Description("Select one or more ISO service dates (yyyy-MM-dd); repeat the option for each date.")]
    public string[]? Dates { get; init; }

    [CommandOption("--from <DATE>")]
    [Description("Select published service dates on or after this ISO date; use with --to.")]
    public string? From { get; init; }

    [CommandOption("--to <DATE>")]
    [Description("Select published service dates on or before this ISO date; use with --from.")]
    public string? To { get; init; }

    public ScheduleDateSelection? DateSelection => Dates is not null
        ? ScheduleDateSelection.ForDates(Dates.Select(ParseDate))
        : From is not null && To is not null
            ? ScheduleDateSelection.Between(ParseDate(From), ParseDate(To))
            : null;

    public override ValidationResult Validate()
    {
        var result = base.Validate();
        if (!result.Successful)
        {
            return result;
        }
        if (Dates is { Length: 0 })
        {
            return ValidationResult.Error("Provide at least one value for --date.");
        }
        if (Dates is not null && (From is not null || To is not null))
        {
            return ValidationResult.Error("Use repeated --date options or the --from/--to range, not both.");
        }
        if ((From is null) != (To is null))
        {
            return ValidationResult.Error("--from and --to must be provided together.");
        }
        var values = (Dates ?? []).Concat(new[] { From, To }.OfType<string>());
        if (values.Any(date => !DateOnly.TryParseExact(
                date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)))
        {
            return ValidationResult.Error("Dates must use ISO calendar format yyyy-MM-dd, such as 2026-10-17.");
        }
        if (From is not null && To is not null && ParseDate(From) > ParseDate(To))
        {
            return ValidationResult.Error("--from must be on or before --to.");
        }

        return ValidationResult.Success();
    }

    private static DateOnly ParseDate(string value) =>
        DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
