# Séances

A .NET 10 Blazor static-SSR application for browsing a local cinema schedule snapshot produced by the CLI. Website requests only read the snapshot. Refresh is a separate command and publishes only after the complete schedule passes validation. The current source adapter targets UGC De Brouckère and UGC Toison d’Or.

## Requirements

- .NET 10 SDK
- Network access is needed only for authorized live collection; fixture refresh and serving work offline

## Run locally

Restore and build with `dotnet restore Seances.sln` and `dotnet build Seances.sln`. A reproducible offline demonstration is:

```sh
dotnet run --project src/Seances.Presentation.Cli -- refresh --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures
dotnet run --project src/Seances.Presentation.Web
```

The fixture responses are synthetic parser cases based on the source structure recorded in `docs/source-discovery.md`; they are clearly identified in their metadata and are not presented as captured UGC responses. Both entry points use the same snapshot in the OS local application-data directory by default (`~/.local/share/Seances/schedule.json` on Linux). The CLI's `--help` prints the actual path. Set `SEANCES_SNAPSHOT_PATH` to the same absolute path for both processes to override it.

This project is local-only for now: it is not published or shared with friends. Permission for the current UGC workflow—retrieving schedules, parsing screenings, and storing them in the local JSON snapshot—is **pending written approval**. Do not run live collection unless UGC has confirmed in writing that this specific collection and local use are permitted. The `--allow-live` flag is only a technical guard; it is not permission. See [source discovery and use status](docs/source-discovery.md#use-and-permission-status).

After written approval, inspect live collected data without changing the saved snapshot:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect --allow-live --date 2026-10-19
```

After written approval, refresh one already-published service date at both cinemas by passing `--date yyyy-MM-dd`; without it, refresh all dates currently published:

```sh
dotnet run --project src/Seances.Presentation.Cli -- refresh --allow-live --date 2026-10-19
```

Replace the example date with one currently published by both cinemas; an unavailable-date error lists that cinema's published dates. `collect` only prints data, while `refresh` publishes the snapshot read by the web app. Before any authorized live run, recheck `https://www.ugc.be/robots.txt` and the applicable source terms. A robots.txt check does not replace written approval. The collector's cinema-list and screening paths begin `/cinemasAjaxAction!` and `/showingsCinemaAjaxAction!`, while the directive observed on 2026-10-17 disallowed `/AjaxAction!`.

The collector spaces requests by at least one second, applies a 20-second request timeout, retries transient HTTP failures at most twice with backoff, and stops a refresh after 30 minutes. A failed collection leaves the last successful snapshot intact. Override the shared per-cinema allowance in `appsettings.json` under `Schedule:Allowances`; each missing cinema value defaults to 20 minutes. Source-reported finishes remain unchanged and display as estimates.

Run both the deterministic unit and integration suites with `dotnet test Seances.sln`. Unit tests cover scheduling policy, UGC DTO mapping, and snapshot validation. Integration tests refresh from offline synthetic HTML cases, read the resulting temporary JSON snapshot, and request the rendered list and details. No Azure account or external metadata provider is used.
