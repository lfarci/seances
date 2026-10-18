using System.ComponentModel;
using System.Globalization;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Seances.Presentation.Cli.Commands;

public abstract class ScheduleCommandSettings : CommandSettings
{
    [CommandOption("--date <DATE>")]
    [Description("Limit the operation to this ISO service date (yyyy-MM-dd).")]
    public string? Date { get; init; }

    public DateOnly? RequestedDate => Date is null
        ? null
        : DateOnly.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public override ValidationResult Validate()
    {
        if (Date is not null && !DateOnly.TryParseExact(
                Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return ValidationResult.Error("--date must be an ISO calendar date such as 2026-10-17.");

        return ValidationResult.Success();
    }
}
