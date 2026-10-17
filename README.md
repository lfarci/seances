# Séances

A .NET 10 Blazor static-SSR application that publishes a local schedule snapshot for UGC De Brouckère and UGC Toison d’Or. Website requests only read the snapshot. Refresh is a separate command and publishes only after the complete collection passes validation.

## Requirements

- .NET 10 SDK
- Network access for live collection; fixture refresh and serving work offline

## Run locally

Restore and build with `dotnet restore Seances.sln` and `dotnet build Seances.sln`. A reproducible offline demonstration is:

```sh
dotnet run --project src/Seances.Presentation.Cli -- refresh --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures
dotnet run --project src/Seances.Presentation.Web
```

The fixture responses are synthetic parser cases based on the source structure recorded in `docs/source-discovery.md`; they are clearly identified in their metadata and are not presented as captured UGC responses. The snapshot defaults to `data/schedule.json`. Set `SEANCES_SNAPSHOT_PATH` to use a different local path.

Inspect live collected data without changing the saved snapshot:

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect --allow-live --date 2026-10-17
```

To refresh one already-published service date at both cinemas, pass `--date yyyy-MM-dd`; without it, refresh all dates currently published:

```sh
dotnet run --project src/Seances.Presentation.Cli -- refresh --allow-live --date 2026-10-17
```

The `--allow-live` flag is an explicit guard. Before using it, recheck `https://www.ugc.be/robots.txt` and the applicable source terms, then run one bounded check. Discovery on 2026-10-16 found that robots.txt disallowed `/AjaxAction!`, while the observed fragment path began `/showingsCinemaAjaxAction!`; these are different literal prefixes. That historical observation is not a current authorization or an API stability commitment. The live directive check could not be repeated from this restricted environment, so no live UGC request was made during implementation.

The collector spaces requests by at least one second, applies a 20-second request timeout, retries transient HTTP failures at most twice with backoff, and stops a refresh after 30 minutes. A failed collection leaves the last successful snapshot intact. Override the shared per-cinema allowance in `appsettings.json` under `Schedule:Allowances`; each missing cinema value defaults to 20 minutes. Source-reported finishes remain unchanged and display as estimates.

Run both the deterministic unit and integration suites with `dotnet test Seances.sln`. Unit tests cover scheduling policy, UGC DTO mapping, and snapshot validation. Integration tests refresh from offline synthetic HTML cases, read the resulting temporary JSON snapshot, and request the rendered list and details. No Azure account or external metadata provider is used.
