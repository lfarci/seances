# Collection improvements

Scope confirmed during the 2026-10-19 interview.

## Goal

Collect the schedule fields UGC provides reliably, preserve useful film identifiers, and make missing required information visible. Original-title lookup can use a film detail page when available. Runtime scraping and external metadata providers, including OMDb matching and posters, are deferred.

## Film information

- Collect original titles from schedule cards and, when still missing, UGC film detail pages.
- Prefer the original title for display. French films can retain French titles; original title does not mean English title.
- Preserve the UGC displayed title separately. If no original title is explicitly available, use the UGC title and leave original title unknown; do not translate or guess.
- Preserve the UGC film ID and detail-page URL for future metadata matching. Do not invent an IMDb ID or assume title alone uniquely identifies a film.
- Preserve source-reported screening start and finish separately. Do not infer film runtime from their difference.

## Output and diagnostics

- Keep source-reported screening start and finish in CSV, table, and JSON output. Existing runtime values from saved snapshots remain supported.
- Send progress, warnings, and the completion summary to stderr, preserving stdout for the requested data format.
- Warn once per film when its original title or detail-page URL is missing. Missing optional screening fields should not produce repeated film warnings.
- Normal progress describes collection stages and cinema/date progress. The final summary reports film and screening totals and incomplete film counts.
- Add `--verbose` for detailed diagnostics, including request/retry context and film-detail failures.
- Use typed `ILogger<T>` entries in Collection and Application with structured context; render CLI diagnostics to stderr.
- Missing film information or a failed optional film-detail request produces a warning and preserves available schedule data. Failure to fetch or parse required schedules remains an error and must preserve any existing output file.

## Implementation and verification

Parse only schedule fields and film identifiers needed by current collection workflows. Do not scrape film runtime or screening format when the source does not provide them reliably.

Keep UGC requests, parsing, and external mappings in Collection; workflow coordination in Application; domain concepts in Domain; persistence mappings and validation in Storage; CLI output and log rendering in CLI.

Organize Collection's source-specific code under `Ugc/`, with `Parsing/`, `Models/`, and `Mapping/` subdirectories and matching namespaces. Keep focused parser unit tests separate from HTTP collection tests, with the same directory organization.

Use synthetic fixtures to verify original-title extraction and fallback, repeated-film merging, optional detail failures, deduplicated warnings, output fields, and stderr/stdout separation. Check compatibility with existing saved snapshots when adding film fields.

Live source work remains subject to the existing [source-use policy](source-discovery.md#use-and-permission-status). No live verification has been performed for this design.
