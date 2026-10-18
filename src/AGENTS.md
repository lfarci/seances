# Production code

- Keep `Program.cs` limited to application bootstrapping. Put web composition in Presentation.Web and focused registration methods when they improve clarity.
- Put manual schedule extraction commands in `Seances.Presentation.Cli`; keep web startup focused on serving the configured snapshot.
- Keep one primary class, record, or interface per file; match filenames and namespaces to the type and folder.
- Keep framework and transport dependencies out of Domain. Parse external formats in Collection and persist snapshots in Storage, mapping to domain models at those boundaries.
- Use concise XML documentation for public types that represent an external schema or a non-obvious domain concept.
- Keep validation rules small and named; use constants for protocol values and policy defaults.
