# Séances

A .NET 10 Blazor static-SSR application for browsing local cinema schedule data. The CLI extracts schedule data to stdout or a JSON/CSV file; the web app can read a JSON snapshot file. Website requests only read the snapshot. The versioned JSON format and validator are source-neutral; the current collection adapter targets UGC De Brouckère and UGC Toison d’Or.

## Requirements

- .NET 10 SDK
- Network access is needed only for authorized live collection; fixture collection and serving work offline

## Run locally

Restore and build with `dotnet restore Seances.sln` and `dotnet build Seances.sln`. A reproducible offline demonstration is:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures \
  --output ./schedule.json
SEANCES_SNAPSHOT_PATH="$PWD/schedule.json" dotnet run --project src/Seances.Presentation.Web
```

The fixture responses are synthetic parser cases based on the source structure recorded in `docs/source-discovery.md`; they are clearly identified in their metadata and are not presented as captured UGC responses. The CLI prints a formatted table by default, or JSON/CSV with `--format json` or `--format csv`. Pass `--output ./schedule.json` or `--output ./schedule.csv` to write machine-readable data to a file; the extension selects the format. Relative paths resolve from the CLI's current directory. The web app uses `SEANCES_SNAPSHOT_PATH` or the OS local application-data path by default (`~/.local/share/Seances/schedule.json` on Linux). To browse the JSON file produced by the CLI, set the web app's `SEANCES_SNAPSHOT_PATH` to that same absolute path. The CLI's `--help` shows available options.

See the [CLI reference](docs/cli.md) for command options, fixture format, and output-path behavior.

This project is local-only for now: it is not published or shared with friends. Permission for the current UGC workflow—retrieving schedules, parsing screenings, and storing them in the local JSON snapshot—is **pending written approval**. Do not run live collection unless UGC has confirmed in writing that this specific collection and local use are permitted. The `--allow-live` flag is only a technical guard; it is not permission. See [source discovery and use status](docs/source-discovery.md#use-and-permission-status).

After written approval, collect one currently published service date from both cinemas with `--date yyyy-MM-dd`; without it, collect all dates currently published:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect \
  --allow-live --date 2026-10-19 --output ./schedule.json
```

Replace the example date with one currently published by both cinemas; an unavailable-date error lists that cinema's published dates. `collect` extracts schedule data to stdout or the requested JSON/CSV path; the web app is a separate reader and is not invoked by collection. Before any authorized live run, recheck `https://www.ugc.be/robots.txt` and the applicable source terms. A robots.txt check does not replace written approval. The collector's cinema-list and screening paths begin `/cinemasAjaxAction!` and `/showingsCinemaAjaxAction!`, while the directive observed on 2026-10-17 disallowed `/AjaxAction!`.

The collector spaces requests by at least one second, applies a 20-second request timeout, retries transient HTTP failures at most twice with backoff, and stops a collection after 30 minutes. A failed collection leaves any existing output file intact. Override the shared per-cinema allowance in `appsettings.json` under `Schedule:Allowances`; each missing cinema value defaults to 20 minutes. Source-reported finishes remain unchanged and display as estimates.

Run both the deterministic unit and integration suites with `dotnet test Seances.sln`. Unit tests cover scheduling policy, UGC DTO mapping, and snapshot validation. Integration tests collect from offline synthetic HTML cases, read the resulting temporary JSON snapshot, and request the rendered list and details. No Azure account or external metadata provider is used.
