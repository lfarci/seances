# CLI reference

The `Seances.Presentation.Cli` app uses Spectre.Console.Cli for command parsing, option validation, and generated help. Run `--help` at the root or after a command:

```sh
dotnet run --project src/Seances.Presentation.Cli -- --help
dotnet run --project src/Seances.Presentation.Cli -- collect --help
dotnet run --project src/Seances.Presentation.Cli -- validate --help
dotnet run --project src/Seances.Presentation.Cli -- inspect --help
```

## `collect`

`collect` extracts schedule data and prints a formatted table by default. Use `--format json` or `--format csv` to write machine-readable output to stdout. Use `--output <path>` to write JSON or CSV to a file; `.json` and `.csv` select the format when `--format` is omitted. Relative output paths resolve from the current directory. File output is atomic, so a failed collection or write preserves an existing destination. Status messages go to stderr.

Film titles prefer an explicitly reported original title, in the original language, and fall back to the UGC title. JSON preserves `originalTitle`, `sourceTitle`, and `detailUrl` alongside the film ID. Existing version 1 JSON snapshots without these additional fields remain readable. CSV and table output retain runtime support for saved data, but collection does not scrape runtime or screening format. Finish times after the service date include the calendar date.

Collection merges available identifiers and original-title metadata for repeated films before fetching each linked film detail page, at most once per run, when the original title is missing. Missing fields remain unknown; runtime is never inferred from screening start and finish. Optional film-detail failures preserve the available schedule; required schedule failures return a nonzero exit status.

Collection and Application use typed `ILogger<T>` entries with structured cinema, date, film, request, and retry properties. The CLI renders these entries to stderr: normal logs show progress, one warning per film with missing original title or detail URL, and a completion summary. Missing screening finishes are summarized separately. Add `--verbose` to include debug entries for requests, retries, and failures. Warnings do not change a successful collection's exit status.

Use `--fixture <directory>` for offline, repeatable input, or `--allow-live` for authorized UGC collection. Repeat `--date` to select specific service dates, or use inclusive `--from` and `--to` bounds to select a range. A range includes dates published by each cinema within those bounds. The fixture format pairs each `*.html` response with a same-named `*.json` metadata file. Fixture files remain unchanged; committed fixtures are synthetic parser cases and do not contact UGC.

Optional film-detail fixtures live in `films/<escaped-film-id>.html` under the fixture directory (for example, `films/42.html`). IDs use URI percent escaping. Missing film fixtures leave the collected metadata as-is and never trigger network requests.

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
  --allow-live --date 2026-10-19 --date 2026-10-20 --output ./schedule.json
```

Select an inclusive date range:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --allow-live --from 2026-10-19 --to 2026-10-25 --output ./schedule.json
```

The current UGC source-use policy requires written approval before live collection. `--allow-live` does not grant permission; inspect [the source-use policy](source-discovery.md#use-and-permission-status) first.

## `dates`

List the published service dates for each configured cinema without requesting screening fragments:

```sh
dotnet run --project src/Seances.Presentation.Cli -- dates --allow-live
```

The command prints a table by default; use `--format json` for machine-readable output. Live calendar requests are subject to the same written-approval policy as schedule collection. `--allow-live` is an operational guard and does not grant permission.

JSON output is a self-contained, versioned schedule document. The CLI does not use the web app's snapshot path configuration. Table output is only available on stdout; file output supports JSON and CSV.

## `validate <path>`

Validate an existing local JSON schedule without collecting data:

```sh
dotnet run --project src/Seances.Presentation.Cli -- validate ./schedule.json
```

The command checks the supported schema version, required fields, unique identities, resolved references, and screening coverage counts using the same Storage validation as the web app. A valid schedule, including an explicitly empty one, prints a success message and returns exit status `0`. Malformed, unsupported, inconsistent, or unreadable files produce a diagnostic on stderr and exit status `1`.

## `inspect <path>`

Display the screenings in an existing local JSON schedule:

```sh
dotnet run --project src/Seances.Presentation.Cli -- inspect ./schedule.json
```

Inspection validates the file first, then prints a table sorted by screening start across all cinemas. Rows show the service date, cinema-local start and source-reported finish, film runtime, cinema, film, verbatim source version label, and available format and room. An after-midnight start retains its recorded service date. A valid empty schedule prints an explicit no-screenings message. Success returns `0`; invalid or unreadable input returns `1` with a diagnostic on stderr.

Both commands require an explicit path. Relative input paths resolve from the current directory. They read only that local file, make no source requests, and leave the file unchanged. They do not use `SEANCES_SNAPSHOT_PATH`. Invalid command syntax returns a nonzero exit status.
