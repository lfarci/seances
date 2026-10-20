# Séances CLI

The CLI extracts cinema schedule data and can print it as a formatted table, JSON, or CSV. It also validates and inspects existing local JSON schedules. It uses Spectre.Console.Cli for command parsing, option validation, and generated help.

Run commands from the repository root with the .NET 10 SDK:

```sh
dotnet run --project src/Seances.Presentation.Cli -- --help
```

## Collect schedule data

Fixture collection works offline. With no output options, the CLI prints a formatted table:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures
```

Print machine-readable output to stdout with `--format`:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures --format json

dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures --format csv
```

Write JSON or CSV to a file with `--output`. The `.json` or `.csv` extension selects the format. You can use `--format` to select the format when the destination has another extension:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures --output ./schedule.json

dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures --output ./schedule.csv
```

Fixture files are read-only inputs. Each `*.html` response is paired with a same-named `*.json` metadata file. The committed fixtures are synthetic parser cases and do not contact UGC. Omit date options to include all available dates, repeat `--date yyyy-MM-dd` to select specific service dates, or use `--from yyyy-MM-dd --to yyyy-MM-dd` for an inclusive range.

For live collection, include `--allow-live`:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --allow-live --date 2026-10-19 --output ./schedule.json
```

Live collection is currently paused pending written approval for the UGC workflow. `--allow-live` is a technical guard, not permission. Review the [source-use policy](../../docs/source-discovery.md#use-and-permission-status) before any live run.

## Validate and inspect a local schedule

```sh
dotnet run --project src/Seances.Presentation.Cli -- validate ./schedule.json
dotnet run --project src/Seances.Presentation.Cli -- inspect ./schedule.json
```

Both commands require an explicit local JSON path, work offline, and leave the file unchanged. Relative paths resolve from the current directory. `validate` checks the document version and schedule consistency. `inspect` applies the same validation and displays screenings chronologically across cinemas, including their service dates, local start times, films, cinemas, and source version labels. Valid empty schedules are accepted. Success returns exit status `0`; invalid or unreadable input returns `1` and an error on stderr.

## List published dates

`dates --allow-live` lists the published dates for each cinema without retrieving screening data. Add `--format json` for machine-readable output. Live requests require written approval under the [UGC source-use policy](../../docs/source-discovery.md#use-and-permission-status); `--allow-live` does not grant permission.

## Collection options

| Option | Description |
| --- | --- |
| `--format <FORMAT>` | `table`, `json`, or `csv`. Defaults to a table on stdout. |
| `--output <PATH>` | Write JSON or CSV to a file. `.json` and `.csv` select a format when `--format` is omitted. Relative paths resolve from the current directory. |
| `--date <yyyy-MM-dd>` | Select a service date. Repeat the option for multiple dates; omit date options to include all available dates. |
| `--from <yyyy-MM-dd> --to <yyyy-MM-dd>` | Select published service dates in the inclusive range. Use both options together and do not combine them with `--date`. |
| `--fixture <DIRECTORY>` | Read fixture responses instead of making live requests. |
| `--allow-live` | Enable live UGC requests when no fixture directory is supplied. |

JSON output is a validated, versioned schedule document. File output is written atomically, so a failed collection or write preserves any existing destination. Status messages go to stderr, keeping stdout usable for pipes.

See the full [CLI reference](../../docs/cli.md) for more examples and output details.
