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

Fixture files are read-only inputs. Each `*.html` response is paired with a same-named `*.json` metadata file. The committed fixtures are synthetic parser cases and do not contact UGC. Omit `--date` to include all dates available in the input; pass `--date yyyy-MM-dd` to select one service date.

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

## Collection options

| Option | Description |
| --- | --- |
| `--format <FORMAT>` | `table`, `json`, or `csv`. Defaults to a table on stdout. |
| `--output <PATH>` | Write JSON or CSV to a file. `.json` and `.csv` select a format when `--format` is omitted. Relative paths resolve from the current directory. |
| `--date <yyyy-MM-dd>` | Limit collection to one service date. Omit it to include all available dates. |
| `--fixture <DIRECTORY>` | Read fixture responses instead of making live requests. |
| `--allow-live` | Enable live UGC requests when no fixture directory is supplied. |

JSON output is a validated, versioned schedule document. File output is written atomically, so a failed collection or write preserves any existing destination. Status messages go to stderr, keeping stdout usable for pipes.

See the full [CLI reference](../../docs/cli.md) for more examples and output details.
