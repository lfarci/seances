# CLI reference

The `Seances.Presentation.Cli` app uses Spectre.Console.Cli for command parsing, option validation, and generated help. Run `--help` at the root or after a command to see the supported syntax:

```sh
dotnet run --project src/Seances.Presentation.Cli -- --help
dotnet run --project src/Seances.Presentation.Cli -- refresh --help
```

## `collect`

`collect` fetches schedules and prints them without writing a snapshot. Live collection is disabled unless `--allow-live` is passed. That flag is an operational guard, not permission; the current UGC workflow requires written approval. An optional `--date yyyy-MM-dd` limits collection to a service date.

```sh
dotnet run --project src/Seances.Presentation.Cli -- collect --allow-live --date 2026-10-19
```

## `refresh`

`refresh` validates and publishes a complete versioned JSON snapshot. Provide either `--fixture <directory>` for deterministic local input or `--allow-live` for an authorized live refresh. `--date yyyy-MM-dd` is optional.

Fixture files are inputs and remain unchanged. The current fixture format pairs each `*.html` response with a same-named `*.json` metadata file. The committed integration fixtures are synthetic parser cases, so refreshing from them does not contact UGC.

```sh
dotnet run --project src/Seances.Presentation.Cli -- refresh \
  --fixture tests/Seances.Presentation.Web.Tests/Integration/Fixtures \
  --date 2026-10-17 \
  --output ./schedule.json
```

Without `--output`, the CLI uses `SEANCES_SNAPSHOT_PATH` when set, or the OS local application-data default. `--output` overrides that setting for this invocation; relative paths resolve from the CLI's current directory. A failed refresh leaves the prior snapshot at the selected output path intact.

The web app uses `SEANCES_SNAPSHOT_PATH` or the OS local application-data default; it does not read the CLI's one-off `--output` option. To browse a snapshot written elsewhere, set the web process's `SEANCES_SNAPSHOT_PATH` to that same absolute path:

```sh
export SEANCES_SNAPSHOT_PATH="$PWD/schedule.json"
dotnet run --project src/Seances.Presentation.Web
```

For live UGC collection, inspect [the project’s current source-use policy](source-discovery.md#use-and-permission-status) first. `--allow-live` does not satisfy that policy.
