# CLI reference

The `Seances.Presentation.Cli` app uses Spectre.Console.Cli for command parsing, option validation, and generated help. Run `--help` at the root or after a command:

```sh
dotnet run --project src/Seances.Presentation.Cli -- --help
dotnet run --project src/Seances.Presentation.Cli -- collect --help
```

## `collect`

`collect` extracts schedule data and prints a formatted table by default. Use `--format json` or `--format csv` to write machine-readable output to stdout. Use `--output <path>` to write JSON or CSV to a file; `.json` and `.csv` select the format when `--format` is omitted. Relative output paths resolve from the current directory. File output is atomic, so a failed collection or write preserves an existing destination. Status messages go to stderr.

Use `--fixture <directory>` for offline, repeatable input, or `--allow-live` for authorized UGC collection. The fixture format pairs each `*.html` response with a same-named `*.json` metadata file. Fixture files remain unchanged; committed fixtures are synthetic parser cases and do not contact UGC.

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures \
  --date 2026-10-17 \
  --format json
```

Write JSON or CSV to files:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures \
  --output ./schedule.json

dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures \
  --output ./schedule.csv
```

Omit `--date` to include all dates available in the source or fixture directory. Live collection requires the explicit `--allow-live` guard:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --allow-live --date 2026-10-19 --output ./schedule.json
```

The current UGC source-use policy requires written approval before live collection. `--allow-live` does not grant permission; inspect [the source-use policy](source-discovery.md#use-and-permission-status) first.

JSON output is a self-contained, versioned schedule document. The CLI does not use the web app's snapshot path configuration. Table output is only available on stdout; file output supports JSON and CSV.
